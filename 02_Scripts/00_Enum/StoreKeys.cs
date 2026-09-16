using LR.Manager.GameDataManager;

public static class StoreKeys
{
  public readonly static string StageLeaderboardKeyFormat = "{0}_{1}";
  public readonly static string SafestSpeedRun = "SafeSpeedRun";
  public readonly static string FastestSpeedRun = "FastSpeedRun";
  public readonly static string SpeedRun = "SPEEDRUN";
  public readonly static string HardModeClearCount = "HARDCLEARCOUNT";
  public readonly static string StageClearFormat = "CHAPTER_CLEAR_{0}";
  public readonly static string StoveStageCount = "CLEARSTAGEINDEX";

  public readonly static string NormalPerfect = "PERFECT_NORMAL";
  public readonly static string HardPerfect = "PERFECT_HARD";

  public static string GetPerfectKey(IDifficultyService.Difficulty difficulty)
    => difficulty switch
    {
      IDifficultyService.Difficulty.Easy => string.Empty,
      IDifficultyService.Difficulty.Normal => NormalPerfect,
      IDifficultyService.Difficulty.Hard => HardPerfect,
      _ => throw new System.NotImplementedException(),
    };
}
