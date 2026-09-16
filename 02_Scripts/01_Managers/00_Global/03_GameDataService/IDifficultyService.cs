using UnityEngine.Events;

namespace LR.Manager.GameDataManager
{
  public interface IDifficultyService
  {
    public enum Difficulty
    {
      Easy,
      Normal,
      Hard,
    }

    public Difficulty CurrentDifficulty { get; }

    public void SetDifficulty(Difficulty difficulty);

    public void SubscribeOnDifficultyChanged(UnityAction<Difficulty> unityAction);

    public void UnsubscribeOnDifficultyChanged(UnityAction<Difficulty> unityAction);
  }
}
