using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Input;
using LR.Manager.Scene;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.UI.Enum;
using LR.UI.GameScene.GlobalRecord;
using LR.UI.GameScene.LocalRecord;
using LR.UI.Indicator;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.GameScene.Stage
{
  public class UIStageSuccessPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public GlobalManager globalManager;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IGameModeService gameModeService;
      [Inject] public IGameDataSetter gameDataSetter;
      [Inject] public IUIIndicatorService indicatorService;
      [Inject] public ISceneLoader sceneLoader;
      [Inject] public IStageStateHandler stageService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IUIDepthService depthService;
      [Inject] public IStageStateProvider stageStateProvider;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public StageManager stageManager;
      [Inject] public IStageRecorderService stageRecorderService;
      [Inject] public UISO uiSO;
      [Inject] public ISFXController sfxController;
      [Inject] public IBGMController bgmController;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public ColorSO colorSO;
      [Inject] public IStoreTypeProvider storeTypeProvider;
      [Inject] public ILeaderBoardService leaderBoardService;
    }

    private readonly Model model;
    private readonly UIStageSuccessView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer perfectIconShakeCTS = new();
    private readonly CTSContainer clearStarCTS = new();
    private readonly UIStageLocalRecordPresenter localRecordPresenter;
    private readonly UIGlobalRecordPresenter globalRecordPresenter;
    private IUIIndicatorPresenter currentIndicator;
    private bool isStartPlayed = false;

    public UIStageSuccessPresenter(Model model, UIStageSuccessView view)
    {
      this.model = model;
      this.view = view;

      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();
      var stageIndex = (chapter - 1) * StageConst.StageUnit + stage;

      var isDemo = model.gameModeService.GetCurrentGameMode() == IGameModeService.GameMode.Demo;
      var isLastStage = stageIndex == model.gameDataProvider.StageDataCount;
      view.DemoObject.SetActive(isDemo && isLastStage);

      var currentDifficulty = model.difficultyService.CurrentDifficulty;
      view.RestartPerfectIcon.OutlineImage.color = model.colorSO.GetDifficultyClearColor(currentDifficulty);

      view.RestartSubmitDirectionSet.Subscribe(
        onPerformed: OnRestart);

      if(model.storeTypeProvider.StoreType == StoreType.Steam)
      {        
        var localRecordModel = model.diContainer.Instantiate<UIStageLocalRecordPresenter.Model>(new object[] { } );
        localRecordPresenter = new(localRecordModel, view.LocalRecordView);
        localRecordPresenter.AttachOnDestroy(view.gameObject);

        stageIndex = (chapter - 1) * StageConst.StageUnit + stage;

        var difficulty = model.difficultyService.CurrentDifficulty;
        var leaderboardKey = string.Format(StoreKeys.StageLeaderboardKeyFormat, stageIndex, difficulty);

        var globalRecordModel = model.diContainer.Instantiate<UIGlobalRecordPresenter.Model>(new object[] { leaderboardKey });
        globalRecordPresenter = new(globalRecordModel, view.GlobalRecordView);
        globalRecordPresenter.AttachOnDestroy(view.gameObject);
      }
      else
      {
        view.LocalRecordView.gameObject.SetActive(false);
        view.GlobalRecordView.gameObject.SetActive(false);
      }

      InitializeNextButton();
      InitializeStarIcons();

      subscribeHandle = new SubscribeHandle(
        () =>
        {
          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
          model.depthService.RaiseDepth(GetFirstButton().gameObject);
          model.inputActionSubscriber.SubscribePhase(LRInputType.StageRestart, OnRestart, InputPhase.Performed);
          view.QuitSubmitDirectionSet.Subscribe(OnQuit);
        },
        () =>
        {
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.StageRestart, OnRestart, InputPhase.Performed);
          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
          model.depthService.LowerDepth();
          view.QuitSubmitDirectionSet.Unsubscribe(OnQuit);
          if (currentIndicator != null)
            ReleaseIndicator();
        });
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      clearStarCTS.Dispose();
      perfectIconShakeCTS?.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      if (!isStartPlayed)
      {
        clearStarCTS.Cancel();
        clearStarCTS.Create();
        var starToken = clearStarCTS.token;
        PlayStarsAsync(starToken).Forget();
      }

      localRecordPresenter?.ActivateAsync(isImmediately, token).Forget();
      globalRecordPresenter?.ActivateAsync(isImmediately, token).Forget();

      var isDoctorExist = model.stageManager.StageDataContainer.IsDoctorExist;
      var isPerfect = model.stageRecorderService.IsThisSuccessIsPerfect();
      view.NormalDoctorImage.enabled = isDoctorExist && !isPerfect;
      view.PerfectDoctorImage.enabled = isDoctorExist && isPerfect;

      if (isPerfect)
      {
        var chapter = model.gameDataProvider.GetSelectedChapter();
        var stage = model.gameDataProvider.GetSelectedStage();
        var stageIndex = (chapter - 1) * StageConst.StageUnit + stage;
        var isDemo = model.gameModeService.GetCurrentGameMode() == IGameModeService.GameMode.Demo;
        var isLastStage = stageIndex == model.gameDataProvider.StageDataCount;
        if(!(isDemo && isLastStage))
          PlayPerfectIcons();
      }        
      else
      {
        model.sfxController.PlayOnce(AudioSourceType.CenterUI, SFX.StageComplete);
        StopPerfectIcons();
      }         

      view.RestartPerfectIcon.gameObject.SetActive(IsRestartPerfactable());

      await GetNewIndicatorAsync();            
      await view.ShowAsync(isImmediately, token);
      subscribeHandle.Subscribe();
      model.depthService.SelectTopObject();
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      clearStarCTS.Cancel();
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmediately, token);
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void InitializeNextButton()
    {
      if (IsNextStageExist())
      {
        view.NextSubmitDirectionSet.Subscribe(
                onPerformed: OnNext);
      }
      else
      {
        view.NextSubmitDirectionSet.gameObject.SetActive(false);
        view.QuitSelectable.AddNavigation(Direction.Right, null);
      }
    }

    private void InitializeStarIcons()
    {
      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();

      if (model.gameDataProvider.IsClearStage(chapter, stage, out var clearDifficulty))
      {
        foreach (var set in view.DifficultyStartSets)
        {
          var isDifficultyCleared = (int)set.Difficulty <= (int)clearDifficulty;
          if (isDifficultyCleared)
            set.NotYet.SetActive(false);
          else
          {
            set.SuccessImage.SetAlpha(0.0f);
            set.SuccessImage.rectTransform.localScale = Vector3.one * model.uiSO.Stage.StarBeginScale;
          }
        }

      }
      else
      {
        foreach (var set in view.DifficultyStartSets)
        {
          set.SuccessImage.SetAlpha(0.0f);
          set.SuccessImage.rectTransform.localScale = Vector3.one * model.uiSO.Stage.StarBeginScale;
        }
      }
    }

    private async UniTask PlayStarsAsync(CancellationToken token)
    {
      isStartPlayed = true;
      try
      {
        var currentDifficulty = model.difficultyService.CurrentDifficulty;
        var scaleDuration = model.uiSO.Stage.StarSclaeDuration;
        foreach (var set in view.DifficultyStartSets)
        {
          if (set.NotYet.activeSelf && (int)set.Difficulty <= (int)currentDifficulty)
          {            
            await UniTask.WaitForSeconds(model.uiSO.Stage.StarBeginDelay, false, PlayerLoopTiming.Update, token);
            set.SuccessImage.SetAlpha(model.uiSO.Stage.StarBeginAlpha);
            DOTween
              .Sequence()
              .Join(set.SuccessImage.rectTransform.DOScale(Vector3.one, scaleDuration))
              .Join(set.SuccessImage.DOFade(1.0f, scaleDuration))
              .OnComplete(() =>
              {
                StarPumpAsync(set.SuccessImage.rectTransform, token).Forget();
              })
              .ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
          }            
        }
        
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask StarPumpAsync(RectTransform rectTransform, CancellationToken token)
    {
      try
      {
        var scale = model.uiSO.Stage.StarPumpScale;
        var duration = model.uiSO.Stage.StarPumpDuration;
        await DOTween
          .Sequence()
          .Append(rectTransform.DOScale(scale, duration))
          .Append(rectTransform.DOScale(1.0f, duration))
          .SetLoops(-1)
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException)
      {

      }
    }

    private bool IsRestartPerfactable()
    {
      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();
      var difficulty = model.difficultyService.CurrentDifficulty;
      return !model.gameDataProvider.IsPerfect(chapter, stage, difficulty);
    }

    private void PlayPerfectIcons()
    {
      perfectIconShakeCTS.Cancel();
      perfectIconShakeCTS.Create();
      var token = perfectIconShakeCTS.token;

      model.sfxController.PlayOnce(AudioSourceType.CenterUI, SFX.PerfectSuccess);
      if (model.bgmController.CurrentVolume >= 0.0f)
        PerfectBGMVolumeAsync(token).Forget();

      for(int i = 0; i < view.PerfectIcons.Count; i++)
        ScaleIconsAsync(i, token).Forget();

      foreach (var icon in view.PerfectIcons)
        icon.gameObject.SetActive(true);
    }

    private async UniTask PerfectBGMVolumeAsync(CancellationToken token)
    {
      var initializedVolume = model.bgmController.CurrentVolume;
      var targetVolume = initializedVolume * 0.2f;
      try
      {
        model.bgmController.UpdateVolume(targetVolume);
        await UniTask.WaitForSeconds(3.0f, true, PlayerLoopTiming.Update, token);        
      }
      catch (OperationCanceledException) { }
      finally
      {
        model.bgmController.UpdateVolume(initializedVolume);
      }
    }

    private async UniTask ScaleIconsAsync(int index, CancellationToken token)
    {
      try
      {
        var iconCount = view.PerfectIcons.Count;
        var waitDelayUnit = Mathf.Min(index, iconCount - 1 - index);
        var waitDelay = model.uiSO.Stage.PerfectLogoScaleDelay * waitDelayUnit;
        await UniTask.WaitForSeconds(waitDelay, true, PlayerLoopTiming.Update, token);
        var targetRectTransform = view.PerfectIcons[index];
        targetRectTransform.localScale = Vector3.one;
        var targetScale = model.uiSO.Stage.PerfectLogoScaleValue;
        var scaleDuration = model.uiSO.Stage.PerfectLogoScaleDuration;
        DOTween
          .Sequence()
          .Append(targetRectTransform.DOScale(targetScale, scaleDuration))
          .Append(targetRectTransform.DOScale(1.0f, scaleDuration))
          .SetLoops(-1)
          .ToUniTask(TweenCancelBehaviour.Kill, token)
          .Forget();
      }
      catch (OperationCanceledException) { }      
    }

    private void StopPerfectIcons()
    {
      perfectIconShakeCTS.Cancel();
      foreach (var icon in view.PerfectIcons)
        icon.gameObject.SetActive(false);
    }

    private RectTransform GetFirstButton()
      => IsNextStageExist() ? view.NextSubmitDirectionSet.RectTransform : view.RestartSelectable.GetComponent<RectTransform>();

    private async void OnRestart()
    {
      if (model.stageStateProvider.GetState() != StageEnum.State.Success)
        return;
      if (GetVisibleState() != VisibleState.Showen)
        return;

      currentIndicator.PlayGoodSubmitSFX();
      await DeactivateAsync();
      model.stageService.RestartAsync().Forget();
    }

    private void OnQuit()
    {
      if (model.stageStateProvider.GetState() != StageEnum.State.Success)
        return;

      model.globalManager.DisposeSpeedRunManager();
      currentIndicator.PlayBadSubmitSFX();      
      subscribeHandle.Unsubscribe();
      model.gameDataSetter.ResetSelectedStage();
      model.sceneLoader.LoadSceneAsync(SceneType.Lobby).Forget();
    }

    private async UniTask GetNewIndicatorAsync()
    {
      currentIndicator = await model.indicatorService.GetNewAsync(view.IndicatorRoot, GetFirstButton());
    }

    private void ReleaseIndicator()
    {
      model.indicatorService.ReleaseTopIndicator();
      currentIndicator = null;
    }

    private void OnSelectedGameObjectEnter(GameObject gameObject)
    {
      if (gameObject.TryGetComponent<Selectable>(out var selectable))
        currentIndicator.SetLeftInputGuide(selectable.navigation);

      currentIndicator.MoveAsync(gameObject);
    }

    private void OnNext()
    {
      if (model.stageStateProvider.GetState() != StageEnum.State.Success)
        return;

      subscribeHandle.Unsubscribe();

      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();
      
      stage += 1;
      var addChapter = stage > StageConst.StageUnit;
      model.gameDataSetter.SetSelectedStage(addChapter ? chapter + 1 : chapter, addChapter ? 1 : stage);
      model.sceneLoader.ReloadCurrentSceneAsync().Forget();
    }

    private bool IsNextStageExist()
    {
      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();

      stage += 1;
      if (stage > StageConst.StageUnit)
      {
        chapter++;
        stage = 1;
      }
      return model.gameDataProvider.IsStageExist(chapter, stage);
    }
  }
}