using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.Manager.UI;
using LR.UI.Enum;
using LR.Manager.Scene;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using LR.Manager.GameDataManager;
using Cysharp.Threading.Tasks.Triggers;

namespace LR.UI.Lobby
{
  public class UIStageButtonPresenter : IUIPresenter
  {
    public class Model
    {      
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IGameDataSetter gameDataSetter;
      [Inject] public IGameModeService gameModeService;
      [Inject] public ColorSO colorSO;
      [Inject] public UISO uiSO;
      [Inject] public ISceneLoader sceneLoader;
      [Inject] public IUIIndicatorService indicatorService;
      [Inject] public ISFXController sfxController;
      [Inject] public UIPresenterContainer presenterContainer;
      [Inject] public IDifficultyService difficultyService;

      [Inject] public int chapter;
      [Inject] public int stage;
      [Inject] public UnityAction onSelectStage;
    }

    private readonly Model model;
    private readonly UIStageButtonView view;

    private readonly bool isButtonEnable;

    public UIStageButtonPresenter(Model model, UIStageButtonView view)
    {
      this.model = model;
      this.view = view;

      var clearStageIndex = model.gameDataProvider.GetMaxClearIndex();
      var currentIndex = Mathf.Max(0, (model.chapter - 1)) * StageConst.StageUnit + model.stage;
      isButtonEnable = currentIndex <= clearStageIndex + 1 &&
                       currentIndex <= model.gameDataProvider.StageDataCount;

      view.SubCanvasGroup.alpha = isButtonEnable ? 1.0f : 0.4f;
      view.DirectionSet.enabled = isButtonEnable;      
      if (isButtonEnable)
        view.DirectionSet.Subscribe(OnSubmit);
      
      var isClearStage = currentIndex < clearStageIndex + 1;
      view.Sweep.enabled = isClearStage;
      if (isClearStage)
      {
        model.gameDataProvider.IsClearStage(model.chapter, model.stage, out var difficulty);
        var targetColor = model.colorSO.GetDifficultyClearColor(difficulty);

        view.Outline.color = targetColor;
        view.BackgroundImage.color = targetColor;

        var chapter = model.chapter;
        var stage = model.stage;
        var isPerfect = model.gameDataProvider.IsPerfect(chapter, stage, difficulty);
        view.PerfectIconAnimator.gameObject.SetActive(isPerfect);
      }
      else
        view.PerfectIconAnimator.gameObject.SetActive(false);
      

      view.HideAsync(true).Forget();
      view.TMP.text = model.stage.ToString();
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      var waitDuration = isImmediately ? 0.0f : model.uiSO.Lobby.StageButtonShowInterval * model.stage;
      await UniTask.WaitForSeconds(waitDuration, false, PlayerLoopTiming.Update, token);

      await view.ShowAsync(isImmediately, token);
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      var waitDuration = isImmediately ? 0.0f : model.uiSO.Lobby.StageButtonShowInterval * (StageConst.StageUnit - model.stage);
      await UniTask.WaitForSeconds(waitDuration, false, PlayerLoopTiming.Update, token);

      await view.HideAsync(isImmediately, token);
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnSubmit()
    {
      view.DirectionSet.Selectable.enabled = false;
      view.DirectionSet.Unsubscribe(OnSubmit);
      model.indicatorService.ReleaseTopIndicator();
      model.onSelectStage?.Invoke();

      ChangeSceneAsync().Forget();
    }

    private async UniTask ChangeSceneAsync()
    {      
      await UniTask.WaitForSeconds(model.uiSO.Lobby.StageBeginDelay);
      if (model.gameModeService.IsSpeedRun)
        model.gameModeService.SetGameMode(IGameModeService.GameMode.None);
        
      model.gameDataSetter.SetSelectedStage(model.chapter, model.stage);
      model.sceneLoader.LoadSceneAsync(SceneType.Game).Forget();
    }
	}
}