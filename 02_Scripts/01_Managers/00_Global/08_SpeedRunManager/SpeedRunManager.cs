using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Stage;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.UI.Speedrun;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace LR.Manager.SpeedRun
{
  public class SpeedRunManager : IDisposable
  {
    private readonly GameObject updateTarget;
    private readonly IResourceManager resourceManager = null;
    [Inject] private readonly IGameDataIOController gameDataIOController = null;
    [Inject] private readonly IAchievementRegister achievementRegister = null;
    private readonly LeaderboardController leaderboardController = null;
    private readonly string uiKey;    

    private UISpeedRunPresenter presenter;
    private StageManager currentStageManager;
    private IDisposable updateDisposable;
    private float time = 0.0f;
    private int restartCount = 0;

    public SpeedRunManager(
      [Inject] TableContainer tableContainer,
      [Inject] ICanvasProvider canvasProvider,
      [Inject] IResourceManager resourceManager,
      [Inject] ISpeedRunDataProvider speedRunDataProvider,
      [Inject] DiContainer diContainer,
      GameObject globalManager)
    {
      this.updateTarget = globalManager;
      this.resourceManager = resourceManager;

      uiKey = tableContainer.AddressableKeySO.Path.UI + tableContainer.AddressableKeySO.UIName.SpeedRun;
      var root = canvasProvider.GetCanvas(LR.UI.Enum.RootType.SpeedRunTimer).transform;
      CreateUIPresenterAsync(diContainer, resourceManager, root, speedRunDataProvider).Forget();

      leaderboardController = diContainer.Instantiate<LeaderboardController>();
    }

    public void Play()
    {      
      updateDisposable = updateTarget
        .UpdateAsObservable()
        .Subscribe(_ => OnUpdate());
    }

    public void End(bool isComplete)
    {
      updateDisposable?.Dispose();

      if (isComplete)
      {
        var speedRunKey = StoreKeys.SpeedRun;
        achievementRegister.SetData(speedRunKey, 1);
        achievementRegister.SetAchievement(speedRunKey);

        var speedRunLeaderboardValue = Mathf.RoundToInt(time * 100.0f);
        leaderboardController.SubmitSpeedrunAsync(speedRunLeaderboardValue, restartCount).Forget();

        gameDataIOController.AddSpeedRunData(time, restartCount);
        gameDataIOController.ResetSpeedrunResumeData();        
      }
      else
      {
        gameDataIOController.UpdateSpeedrunResumeTimeData(time, restartCount);
      }
      gameDataIOController.SaveDataAsync();
    }

    public void SaveResumeData(int chapter, int stage)
    {
      gameDataIOController.UpdateSpeedrunResumeData(chapter, stage);
      gameDataIOController.UpdateSpeedrunResumeTimeData(time, restartCount);
      gameDataIOController.SaveDataAsync();
    }

    private async UniTask CreateUIPresenterAsync(DiContainer diContainer, IResourceManager resourceManager, Transform root, ISpeedRunDataProvider speedRunDataProvider)
    {
      var model = diContainer.Instantiate<UISpeedRunPresenter.Model>();
      var view = await resourceManager.CreateAssetAsync<UISpeedRunView>(uiKey, root);
      presenter = new UISpeedRunPresenter(model, view);


      if (speedRunDataProvider.TryGetRunningData(out var runningData))
      {
        time = runningData.Second;        
        restartCount = runningData.RestartCount;        
      }

      presenter.UpdateTimer(time);
      presenter.UpdateRestartCount(restartCount);
    }

    private void OnUpdate()
    {
      var localManager = LocalManager.instance;
      if (localManager == null || localManager.stageManager == null)
        return;

      UpdateStageManager(localManager.stageManager);

      if (!currentStageManager.IsPlayingState)
        return;

      UpdateTime();
    }

    private void UpdateTime()
    {
      time += Time.deltaTime;

      presenter?.UpdateTimer(time);
    }

    private void UpdateStageManager(StageManager newStageManager)
    {
      if (currentStageManager == newStageManager)
        return;

      currentStageManager?.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);
      
      currentStageManager = newStageManager;
      currentStageManager.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);
    }

    private void OnRestart()
    {
      restartCount++;
      presenter?.UpdateRestartCount(restartCount);
    }

    public void Dispose()
    {
      resourceManager.ReleaseAsset(uiKey);
      updateDisposable?.Dispose();
      presenter?.Dispose();
    }
  }
}
