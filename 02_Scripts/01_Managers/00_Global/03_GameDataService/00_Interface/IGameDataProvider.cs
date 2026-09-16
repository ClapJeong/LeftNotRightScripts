using System.Collections.Generic;

namespace LR.Manager.GameDataManager
{
  public interface IGameDataProvider
  {
    public int StageDataCount { get; }

    public int GetSelectedChapter();

    public int GetSelectedStage();

    public int GetMaxClearIndex();

    public bool IsStageExist(int chapter, int stage);

    public bool IsClearStage(int chapter, int stage, out IDifficultyService.Difficulty difficulty);
    public bool IsClearStageWithDifficulty(int chapter, int stage, IDifficultyService.Difficulty difficulty);

    public bool IsVeryFirst();

    public bool IsAllClear();

    public bool WasDemoPlayed();

    public List<int> GetClearChaptes();

    public int GetHardClearCount();

    public bool IsPerfect(int chapter, int stage, IDifficultyService.Difficulty difficulty);

    public bool TryGetSpeedrunResumeData(out int chapter, out int stage);

    public int GetPerfectClearCount(IDifficultyService.Difficulty difficulty);
  }
}
