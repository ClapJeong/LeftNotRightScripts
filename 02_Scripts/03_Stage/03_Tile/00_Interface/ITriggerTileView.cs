using LR.Manager.GameDataManager;
using LR.Stage.TriggerTile.Enum;

namespace LR.Stage.TriggerTile
{
  public interface ITriggerTileView : ITriggerEventSubscriber
  {
    public TriggerTileType GetTriggerType();

    public bool IsEnableDifficulty(IDifficultyService.Difficulty difficulty);

    public bool DisableOnEasy { get; }

    public bool EnableOnHard { get; }
  }
}