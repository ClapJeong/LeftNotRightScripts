using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Input;
using LR.Manager.Scene;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Manager.Stage.Practice;
using LR.Manager.UI;
using LR.UI.DifficultySettting;
using LR.UI.Enum;
using LR.UI.Indicator;
using LR.UI.VolumeControl;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.GameScene.Stage
{
  public class UIStagePausePresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public GlobalManager globalManager;
      [Inject] public IVolumeProvider volumeProvider;
      [Inject] public IVolumeController volumeController;
      [Inject] public UISO uiSO;
      [Inject] public IUIIndicatorService indicatorService;
      [Inject] public ISceneLoader sceneLoader;
      [Inject] public IStageStateHandler stageService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IUIDepthService depthService;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public IGameDataSetter gameDataSetter;
      [Inject] public IPracticeSubscriber practiceSubscriber;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public IGameModeService gameModeService;
      [Inject] public IGameDataProvider gameDataProvider;
    }

    private readonly Model model;
    private readonly UIStagePauseView view;

    private readonly UIVolumeControlPresenter volumeControlPresenter;
    private readonly UIRedPillButtonPresenter redPillButtonPresenter;
    private readonly UIDifficultyPresenter difficultyPresenter;
    private readonly SubscribeHandle subscribeHandle;
    private IUIIndicatorPresenter currentIndicator;

    public UIStagePausePresenter(Model model, UIStagePauseView view)
    {
      this.view = view;
      this.model = model;

      var volumeControlModel = model.diContainer.Instantiate<UIVolumeControlPresenter.Model>();
      volumeControlPresenter = new(volumeControlModel, view.VolumeControlView);
      volumeControlPresenter.AttachOnDestroy(view.gameObject);

      var redPillModel = model.diContainer.Instantiate<UIRedPillButtonPresenter.Model>();
      redPillButtonPresenter = new(redPillModel, view.RedPillButtonView);
      redPillButtonPresenter.AttachOnDestroy(view.gameObject);
      redPillButtonPresenter.ActivateAsync(true).Forget();

      if(model.gameModeService.GetCurrentGameMode() != IGameModeService.GameMode.SpeedRun)
      {
        var difficultyModel = model.diContainer.Instantiate<UIDifficultyPresenter.Model>(new object[]
      {
        (UnityAction<UIDifficultyView.DifficultyButtonSet>)(_ =>
        {
          DeactivateAsync(true).Forget();
          model.sceneLoader.ReloadCurrentSceneAsync();
        })
      });
        difficultyPresenter = new(difficultyModel, view.DifficultyView);
        difficultyPresenter.AttachOnDestroy(view.gameObject);
        difficultyPresenter.ActivateAsync(true).Forget();
      }
      else
      {
        view.DifficultyView.gameObject.SetActive(false);
      }

      view.ResumeSubmitDirectionSet.Subscribe(
        onPerformed: OnResume);

      view.QuitSubmitDirectionSet.Subscribe(
        onPerformed: OnQuit);

      var currentDifficulty = model.difficultyService.CurrentDifficulty;
      foreach (var difficultySet in view.DifficultyView.DifficultyButtonSets)
        if (difficultySet.Difficulty == currentDifficulty)
          view.VolumeControlView.MasterVolumeSet.Selectable.AddNavigation(Direction.Up, difficultySet.Selectable);

      subscribeHandle = new SubscribeHandle(
        () =>
        {         
          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
          volumeControlPresenter.ActivateAsync().Forget();
          model.depthService.RaiseDepth(view.ResumeSubmitDirectionSet.gameObject);

          model.inputActionSubscriber.SubscribePhase(LRInputType.StageRestart, OnRestart, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.Pause, OnResume, InputPhase.Performed);

          model.practiceSubscriber.SubscribeOnPractice(OnPracticeChanged);
        },
        () =>
        {
          volumeControlPresenter.DeactivateAsync().Forget();

          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
          model.depthService.LowerDepth();

          model.inputActionSubscriber.UnsubscribePhase(LRInputType.StageRestart, OnRestart, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.Pause, OnResume, InputPhase.Performed);

          model.practiceSubscriber.UnsubscribeOnPractice(OnPracticeChanged);
          if (currentIndicator != null)
            ReleaseIndicator();
        });
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      volumeControlPresenter.Dispose();
      redPillButtonPresenter.Dispose();
      difficultyPresenter?.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await GetNewIndicatorAsync();
      model.stageService.Pause();  
      await view.ShowAsync(isImmediately, token);
      subscribeHandle.Subscribe();
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {      
      subscribeHandle.Unsubscribe();      
      await view.HideAsync(isImmediately, token);
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnPracticeChanged(bool isPractice)
    {
      DeactivateAsync().Forget();
    }

    private void OnRestart()
    {
      if (model.stageService.IsRestartDelay)
        return;

      currentIndicator?.PlayGoodSubmitSFX();
      DeactivateAsync(true).Forget();
      model.stageService.RestartAsync().Forget();
    }

    private void OnResume()
    {
      currentIndicator?.PlayGoodSubmitSFX();
      DeactivateAsync().Forget();
      model.stageService.Resume();      
    }

    private void OnQuit()
    {
      currentIndicator?.PlayBadSubmitSFX();
      subscribeHandle.Unsubscribe();
      model.gameDataSetter.ResetSelectedStage();
      model.globalManager.StopSpeedRun(false);
      model.globalManager.DisposeSpeedRunManager();
      model.sceneLoader.LoadSceneAsync(SceneType.Lobby).Forget();
    }

    private async UniTask GetNewIndicatorAsync()
    {
      currentIndicator = await model.indicatorService.GetNewAsync(view.IndicatorRoot, view.ResumeSubmitDirectionSet.RectTransform);
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

      if (gameObject == view.QuitSubmitDirectionSet.gameObject || gameObject == view.ResumeSubmitDirectionSet.gameObject)
      {        
        view.VolumeControlView.SFXVolumeSet.Selectable.AddNavigation(Direction.Down, gameObject.GetComponent<Selectable>());
      }
      else
      {
        foreach (var difficultySet in view.DifficultyView.DifficultyButtonSets)
          if (gameObject == difficultySet.Selectable.gameObject)
            view.VolumeControlView.MasterVolumeSet.Selectable.AddNavigation(Direction.Up, difficultySet.Selectable);
      }

      UpdateSpeedrunGuideCanvasgroup(gameObject);
    }

    private void UpdateSpeedrunGuideCanvasgroup(GameObject gameObject)
    {
      var isSpeedRun = model.gameModeService.IsSpeedRun;
      if (isSpeedRun)
      {
        var chapter = model.gameDataProvider.GetSelectedChapter();
        var stage = model.gameDataProvider.GetSelectedStage();
        var index = chapter * StageConst.StageUnit + stage;
        var isLastStage = index == model.gameDataProvider.StageDataCount;
        if (!isLastStage)
        {
          var isQuitButton = gameObject == view.QuitSubmitDirectionSet.gameObject;
          view.SpeedRunQuitGuideCanvasGroup.alpha = isQuitButton ? 1.0f : 0.0f;
        }
      }
    }
  }
}