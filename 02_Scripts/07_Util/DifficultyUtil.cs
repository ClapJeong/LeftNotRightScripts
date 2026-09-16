using LR.Manager.GameDataManager;

public static class DifficultyUtil
{
  public static bool IsLower(this IDifficultyService.Difficulty diffculty, IDifficultyService.Difficulty lowerTargetDifficulty)
  {
    var myIndex = (int)diffculty;
    var targetIndex = (int)lowerTargetDifficulty;
    return targetIndex <= myIndex;
  }
}
