using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Manager.Stage;

namespace LR.Stage.InteractiveObject
{
  public interface IInteractiveObject : IStageObjectController
  {
    public void Initialize(StageManager stageManager, ISFXController sfxController);

    public bool IsEnableDifficulty(IDifficultyService.Difficulty difficulty);
  }
}
