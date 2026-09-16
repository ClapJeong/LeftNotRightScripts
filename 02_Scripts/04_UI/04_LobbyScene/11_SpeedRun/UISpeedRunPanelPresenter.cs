using Cysharp.Threading.Tasks;
using LR.UI.Enum;
using LR.UI.Indicator;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using LR.Manager.UI;
using LR.Manager.Scene;
using LR.Manager.GameDataManager;
using Zenject;
using LR.Manager.Store;

namespace LR.UI.Lobby
{
  public class UISpeedRunPanelPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public GlobalManager globalManager;
      [Inject] public IGameModeService gameModeService;
      [Inject] public IGameDataSetter gameDataSetter;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public ISpeedRunDataProvider speedRunDataProvider;
      [Inject] public ISceneLoader sceneLoader;
      [Inject] public IUIDepthService depthService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IGameDataIOController gameDataIOController;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public IStoreTypeProvider storeTypeProvider;
      [Inject] public IUIIndicatorPresenter indicator;
      [Inject] public UnityAction onExit;
    }

    private readonly Model model;
    private readonly UISpeedRunPanelView view;

    private readonly UISpeedrunLeaderboardPresenter safestLeaderboardPresenter;
    private readonly UISpeedrunLeaderboardPresenter fastestLeaderboardPresenter;

    private readonly SubscribeHandle subscribeHandle;
    private bool isFirstIndiactorMove = true;

    public UISpeedRunPanelPresenter(Model model, UISpeedRunPanelView view)
    {
      this.model = model;
      this.view = view;

      var isSpeedRunAble = model.gameDataProvider.IsAllClear();
      if (isSpeedRunAble)
      {
        view.DisableRoot.SetActive(false);

        var fastestData = model.speedRunDataProvider.GetFastedSpeedRunData();
        var fastestTimeSpan = TimeSpan.FromSeconds(fastestData.Second);
        view.FastestDataTMPSet.Time.text = fastestTimeSpan.ToString(@"mm\:ss\.ff");
        view.FastestDataTMPSet.Restart.text = fastestData.RestartCount.ToString();

        var safestData = model.speedRunDataProvider.GetSafestSpeedRunData();
        var safestTimeSpan = TimeSpan.FromSeconds(safestData.Second);
        view.SafestDataTMPSet.Time.text = safestTimeSpan.ToString(@"mm\:ss\.ff");
        view.SafestDataTMPSet.Restart.text = safestData.RestartCount.ToString();

        var isResumable = model.gameDataProvider.TryGetSpeedrunResumeData(out var resumeChapter, out var resumeStage);
        view.ResumedirectionSet.CanvasGroup.alpha = isResumable ? 1.0f : 0.4f;

        if (isResumable)
        {
          var runniungData = model.speedRunDataProvider.TryGetRunningData(out var runningData);
          var runningSpan = TimeSpan.FromSeconds(runningData.Second);
          view.ResumePreviewText.text = runningSpan.ToString(@"mm\:ss\.ff") +
          $"\n{resumeChapter}-{resumeStage}";

          view.FastestLeaderboardView.Selectable.AddNavigation(Direction.Right, view.ResumedirectionSet.Selectable);
          view.SafestLeaderboardView.Selectable.AddNavigation(Direction.Left, view.ResumedirectionSet.Selectable);
        }
        else
        {
          view.PlayDirectionSet.Selectable.AddNavigation(Direction.Up, null);
          view.FastestLeaderboardView.Selectable.AddNavigation(Direction.Right, view.PlayDirectionSet.Selectable);
          view.SafestLeaderboardView.Selectable.AddNavigation(Direction.Left, view.PlayDirectionSet.Selectable);
        }
        view.ResumePreviewPanel.SetActive(false);
      }
      else
      {
        view.FastestLeaderboardView.gameObject.SetActive(false);
        view.SafestLeaderboardView.gameObject.SetActive(false);

        view.EnableRoot.SetActive(false);
        view.PlayButtonCanvasGroup.alpha = 0.4f;
        view.ResumedirectionSet.CanvasGroup.alpha = 0.4f;

        view.FastestLeaderboardView.Selectable.AddNavigation(Direction.Right, view.ExitDirectionSet.Selectable);
        view.SafestLeaderboardView.Selectable.AddNavigation(Direction.Left, view.ExitDirectionSet.Selectable);
      }

      if (model.storeTypeProvider.StoreType == StoreType.Steam)
      {
        var fastestModel = model.diContainer.Instantiate<UISpeedrunLeaderboardPresenter.Model>(new object[] { StoreKeys.FastestSpeedRun, true });
        fastestLeaderboardPresenter = new(fastestModel, view.FastestLeaderboardView);
        fastestLeaderboardPresenter.AttachOnDestroy(view.gameObject);

        var safestModel = model.diContainer.Instantiate<UISpeedrunLeaderboardPresenter.Model>(new object[] { StoreKeys.SafestSpeedRun, false });
        safestLeaderboardPresenter = new(safestModel, view.SafestLeaderboardView);
        safestLeaderboardPresenter.AttachOnDestroy(view.gameObject);
      }
      else
      {
        view.FastestLeaderboardView.gameObject.SetActive(false);
        view.SafestLeaderboardView.gameObject.SetActive(false);
      }

      subscribeHandle = new(
        () =>
        {
          view.ExitDirectionSet.Subscribe(model.onExit);
          if (isSpeedRunAble)
          {
            view.PlayDirectionSet.Subscribe(OnPlaySpeedrun);
            if (model.gameDataProvider.TryGetSpeedrunResumeData(out var resumeChapter, out var resumeStage))
              view.ResumedirectionSet.Subscribe(OnResumeSpeedrun);
          }
          
          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObject);
          model.depthService.RaiseDepth(view.ExitDirectionSet.RectTransform.gameObject);
        },
        () =>
        {
          if (isSpeedRunAble)
          {
            view.PlayDirectionSet.Unsubscribe(OnPlaySpeedrun);
            view.ResumedirectionSet.Unsubscribe(OnResumeSpeedrun);
          }

          view.ExitDirectionSet.Unsubscribe(model.onExit);          
          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObject);
          model.depthService.LowerDepth();
        });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {      
      isFirstIndiactorMove = true;
      await view.ShowAsync(isImmedieately, token);
      subscribeHandle.Subscribe();

      if(safestLeaderboardPresenter != null)
       await safestLeaderboardPresenter.ActivateAsync(isImmedieately, token);
      fastestLeaderboardPresenter?.ActivateAsync(isImmedieately, token).Forget();
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnPlaySpeedrun()
    {
      subscribeHandle.Unsubscribe();

      model.gameDataIOController.UpdateSpeedrunResumeData(0, 0);
      model.indicator.PlayGoodSubmitSFX();

      model.gameDataIOController.ResetSpeedrunResumeData();
      model.globalManager.PlaySpeedrun();
      model.difficultyService.SetDifficulty(IDifficultyService.Difficulty.Normal);
      model.gameModeService.SetGameMode(IGameModeService.GameMode.SpeedRun);      
      model.gameDataSetter.SetSelectedStage(1, 1);
      model.sceneLoader.LoadSceneAsync(SceneType.Game).Forget();
    }

    private void OnResumeSpeedrun()
    {
      model.gameDataProvider.TryGetSpeedrunResumeData(out var resumeChapter, out var resumeStage);

      subscribeHandle.Unsubscribe();

      model.indicator.PlayGoodSubmitSFX();

      model.globalManager.PlaySpeedrun();
      model.gameModeService.SetGameMode(IGameModeService.GameMode.SpeedRun);
      model.gameDataSetter.SetSelectedStage(resumeChapter, resumeStage);
      model.sceneLoader.LoadSceneAsync(SceneType.Game).Forget();
    }

    private void OnSelectedGameObject(GameObject gameObject)
    {
      model.indicator.MoveAsync(gameObject, isFirstIndiactorMove).Forget();

      if (gameObject.TryGetComponent<Selectable>(out var selectable))
        model.indicator.SetLeftInputGuide(selectable.navigation);

      view.ResumePreviewPanel.SetActive(view.ResumedirectionSet.IsEnable() && gameObject == view.ResumedirectionSet.gameObject);
    }
  }
}