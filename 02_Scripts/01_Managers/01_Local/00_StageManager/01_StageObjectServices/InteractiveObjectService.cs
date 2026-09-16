using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Stage.InteractiveObject;
using LR.Stage.StageDataContainer;
using System.Collections.Generic;
using Zenject;


namespace LR.Manager.Stage.StageObject
{
  public class InteractiveObjectService : IStageObjectSetupService, IStageObjectControlService
  {
    [Inject] private readonly DiContainer diContainer = null;
    [Inject] private readonly IDifficultyService difficultyService = null;

    private List<IInteractiveObject> interactiveObjects;
    private bool isSetupComplete = false;

    public async UniTask SetupAsync(StageDataContainer stageDataContainer, bool isEnableImmediately = false)
    {
      interactiveObjects = stageDataContainer.InteractiveObject;

      var stageManager = diContainer.Resolve<StageManager>();
      var sfxController = diContainer.Resolve<ISFXController>();
      var currentDifficulty = difficultyService.CurrentDifficulty;

      foreach (var baseInteractiveObject in interactiveObjects)
      {
        if (!baseInteractiveObject.IsEnableDifficulty(currentDifficulty))
          continue;

        baseInteractiveObject.Initialize(stageManager, sfxController);
      }        

      isSetupComplete = true;
      await UniTask.CompletedTask;
    }

    public async UniTask AwaitUntilSetupCompleteAsync()
    {
      await UniTask.WaitUntil(() => isSetupComplete);
    }

    public void EnableAll(bool isEnable)
    {
      foreach (var baseInteractiveObject in interactiveObjects)
        baseInteractiveObject.Enable(isEnable);
    }

    public void Release()
    {

    }

    public void RestartAll()
    {
      foreach (var baseInteractiveObject in interactiveObjects)
        baseInteractiveObject.Restart();
    }
  }
}