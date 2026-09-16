using Cysharp.Threading.Tasks;
using LR.Manager.Stage;
using LR.Stage.StageDataContainer;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using LR.Manager.UI;
using LR.Manager.Input;
using Zenject;
using LR.UI.GameScene.Speedrun;
using LR.Manager.GameDataManager;

namespace LR.UI.GameScene.Stage
{
  public class UIStageRootPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public IStageStateHandler stageStateHandler;
      [Inject] public IStageStateProvider stageStateProvider;
      [Inject] public IStageEventSubscriber stageEventSubscriber;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IGameModeService gameModeService;
      [Inject] public InputActionManager inputActionManager;
      [Inject] public StageDataContainer stageDataContainer;
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public IStageFailDataSubscriber stageFailDataSubscriber;
      [Inject] public IRedPillModeService redPillModeService;
      [Inject] public ColorSO colorSO;
    }

    private readonly Model model;
    private readonly UIStageRootView view;

    private UIStageBeginPresenter beginPresenter;
    private UIStageFailPresenter failPresenter;
    private UIStageSuccessPresenter successPresenter;
    private UISpeedrunCompletePresenter speedrunCompletePresenter;
    private UIStagePausePresenter pausePresenter;
    private UIStageRestartPresenter restartPresenter;
    private UIPracticePresenter practicePresenter;
    private UIPerfectNoticePresenter perfectNoticePresenter;

    public UIStageRootPresenter(Model model, UIStageRootView view)
    {
      this.model = model;
      this.view = view;

      var isDemo = model.gameModeService.GetCurrentGameMode() == IGameModeService.GameMode.Demo;
      view.DemoObject.SetActive(isDemo);

      InitializeTexts();

      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();            

      if (model.gameModeService.IsSpeedRun)
      {
        view.SuccessView.gameObject.SetActive(false);
        var index = (chapter - 1) * StageConst.StageUnit + stage;
        var isLastSpeedrun = index == model.gameDataProvider.StageDataCount;
        if (isLastSpeedrun)
        {
          CreateSpeedrunCompetePresenter();
          speedrunCompletePresenter.DeactivateAsync(true).Forget();
        }
        else
        {
          view.SpeedrunCompleteView.gameObject.SetActive(false);
        }

        view.PracticeView.gameObject.SetActive(false);
      }
      else
      {
        CreateSuccessPresenter();
        view.SpeedrunCompleteView.gameObject.SetActive(false);
        successPresenter.DeactivateAsync(true).Forget();

        CreatePracticePresenter();
      }

      CreateBeginPresenter();
      CreateFailPresenter();      
      CreatePausePresenter();
      CreateRestartPresenter();
      CreatePerfectNoticePresenter();

      beginPresenter.DeactivateAsync(true).Forget();
			failPresenter.DeactivateAsync(true).Forget();      
      pausePresenter.DeactivateAsync(true).Forget();

			model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.BeforeShowComplete, OnBeforeShowComplete);
			model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Exhausted, OnStageFailed);      
      model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.AfterDialogueComplete, OnAfterDialogueComplete);

      model.
        presenterContainer
        .Add(this);
    }

    private void InitializeTexts()
    {
      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();

      view.StageInfoTMP.text = $"{chapter}-{stage}";
      var currentDifficulty = model.difficultyService.CurrentDifficulty;
      view.DifficultyTMP.text = currentDifficulty.ToString();
      view.DifficultyTMP.color = model.colorSO.GetDifficultyClearColor(currentDifficulty);

      view.RestartText.StringReference.Arguments = new object[] { (int)0 };
      view.RestartText.RefreshString();
      view.FailCountText.StringReference.Arguments = new object[] { (int)0 };
      view.FailCountText.RefreshString();
      view.BonusTimeText.StringReference.Arguments = new object[] { 0.0f.ToString("f1") };
      view.BonusTimeText.RefreshString();
      model.stageFailDataSubscriber.SubscribeOnChanged(IStageFailDataSubscriber.DataType.Restart, restartCount =>
      {
        view.RestartText.StringReference.Arguments = new object[] { (int)restartCount };
        view.RestartText.RefreshString();
      });
      model.stageFailDataSubscriber.SubscribeOnChanged(IStageFailDataSubscriber.DataType.Failure, failCount =>
      {
        view.FailCountText.StringReference.Arguments = new object[] { (int)failCount };
        view.FailCountText.RefreshString();
      });
      model.stageFailDataSubscriber.SubscribeOnChanged(IStageFailDataSubscriber.DataType.BonusTime, bonusTime =>
      {
        view.BonusTimeText.StringReference.Arguments = new object[] { bonusTime.ToString("f1") };
        view.BonusTimeText.RefreshString();
      });

      var isRedPillMode = model.redPillModeService.IsRedPillEnable;
      view.RestartText.gameObject.SetActive(isRedPillMode);
      view.FailCountText.gameObject.SetActive(isRedPillMode);
      view.BonusTimeText.gameObject.SetActive(isRedPillMode);

      model.redPillModeService.SubscribeOnChanged(OnRedPillModeChanged);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      if (view)
        view.DestroySelf();

      UnsubscribePauseInput();

      model.redPillModeService.UnsubscribeOnChanged(OnRedPillModeChanged);
      model.
        presenterContainer
        .Remove(this);
    }

    public VisibleState GetVisibleState()
      => VisibleState.Showen;

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      beginPresenter.EnableInput(false);
      await view.HideAsync(isImmediately, token);
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await view.ShowAsync(isImmediately, token);      
      SubscribePauseInput();      
    }

    private void OnRedPillModeChanged(bool isRedPillMode)
    {
      view.RestartText.gameObject.SetActive(isRedPillMode);
      view.FailCountText.gameObject.SetActive(isRedPillMode);
      view.BonusTimeText.gameObject.SetActive(isRedPillMode);
    }

    private void OnBeforeShowComplete()
    {
      ActivateAsync().Forget();
      beginPresenter.ActivateAsync().Forget();
      beginPresenter.EnableInput(true);
    }

    private void OnAfterDialogueComplete()
    {
      if (model.gameModeService.IsSpeedRun)
      {
        var chapter = model.gameDataProvider.GetSelectedChapter();
        var stage = model.gameDataProvider.GetSelectedStage();
        var index = (chapter - 1) * StageConst.StageUnit + stage;

        var isLastSpeedrun =  index == model.gameDataProvider.StageDataCount;
        if (isLastSpeedrun)
          speedrunCompletePresenter.ActivateAsync().Forget();

        return;
      }        

      successPresenter.ActivateAsync().Forget();
    }

    private void CreateBeginPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIStageBeginPresenter.Model>(new object[] {this.model.stageDataContainer.stageGimmick});
      var beginView = view.BeginView;
      beginPresenter = new UIStageBeginPresenter(model, beginView);
      beginPresenter.AttachOnDestroy(view.gameObject);
    }

    private void CreateFailPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIStageFailPresenter.Model>();
      var failView = view.FailView;
      failPresenter = new UIStageFailPresenter(model, failView);
      failPresenter.AttachOnDestroy(view.gameObject);
    }

    private void CreateSuccessPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIStageSuccessPresenter.Model>();
      var view = this.view.SuccessView;
      successPresenter = new UIStageSuccessPresenter(model, view);
      successPresenter.AttachOnDestroy(this.view.gameObject);
    }

    private void CreateSpeedrunCompetePresenter()
    {
      var model = this.model.diContainer.Instantiate<UISpeedrunCompletePresenter.Model>();
      var view = this.view.SpeedrunCompleteView;
      speedrunCompletePresenter = new UISpeedrunCompletePresenter(model, view);
      speedrunCompletePresenter.AttachOnDestroy(this.view.gameObject);      
    }

    private void CreatePausePresenter()
    {
      var model = this.model.diContainer.Instantiate<UIStagePausePresenter.Model>();
      var view = this.view.PauseView;
      pausePresenter = new UIStagePausePresenter(model, view);
      pausePresenter.AttachOnDestroy(this.view.gameObject);
    }

    private void CreateRestartPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIStageRestartPresenter.Model>();
      var view = this.view.RestartView;
      restartPresenter = new UIStageRestartPresenter(model, view);
      restartPresenter.AttachOnDestroy(this.view.gameObject);
      restartPresenter.ActivateAsync().Forget();
    }

    private void CreatePracticePresenter()
    {
      var model = this.model.diContainer.Instantiate<UIPracticePresenter.Model>();
      var view = this.view.PracticeView;
      practicePresenter = new UIPracticePresenter(model, view);
      practicePresenter.AttachOnDestroy(this.view.gameObject);
      practicePresenter.ActivateAsync().Forget();
    }

    private void CreatePerfectNoticePresenter()
    {
      var model = this.model.diContainer.Instantiate<UIPerfectNoticePresenter.Model>();
      var view = this.view.PerfectNoticeView;
      perfectNoticePresenter = new(model, view);
      perfectNoticePresenter.AttachOnDestroy(this.view.gameObject);
    }

    private void SubscribePauseInput()
    {
      model.inputActionManager.SubscribePhase(LRInputType.Pause, OnPauseInputPerformed, InputPhase.Performed);
      model.inputActionManager.SubscribePhase(LRInputType.StageRestart, OnRestartInputPerformed, InputPhase.Performed);
    }

    private void UnsubscribePauseInput()
    {
      model.inputActionManager.UnsubscribePhase(LRInputType.Pause, OnPauseInputPerformed, InputPhase.Performed);
      model.inputActionManager.UnsubscribePhase(LRInputType.StageRestart, OnRestartInputPerformed, InputPhase.Performed);
    }

    private void OnPauseInputPerformed()
    {
      var currentStageState = model.stageStateProvider.GetState();

      if (currentStageState == StageEnum.State.Ready)
      {
        beginPresenter.DeactivateAsync().Forget();
        model.stageStateHandler.BeginStage();
      }        

      else if (currentStageState != StageEnum.State.Playing && currentStageState != StageEnum.State.RestartWait)
        return;

      pausePresenter.ActivateAsync().Forget();
    }

    private void OnRestartInputPerformed()
    {
      if (!model.stageStateProvider.IsPlayingState)
        return;

      model.stageStateHandler.RestartAsync().Forget();
    }

    #region Callbacks
    private void OnStageFailed()
    {
      if (model.gameModeService.IsSpeedRun)
        return;

      failPresenter.ActivateAsync().Forget();
    }
    #endregion
  }
}