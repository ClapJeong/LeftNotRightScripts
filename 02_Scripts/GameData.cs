using LR.Manager.GameDataManager;
using System.Collections.Generic;

public class GameData
{
  public bool IsDemoPlayed = false;
  public int DataVersion = 0;

  public int LastSpeedrunChapter = 0;
  public int LastSpeedrunStage = 0;

  public GameData(int dataVersion)
  {
    this.DataVersion = dataVersion;
  }

  [System.Serializable]
  public class SpeedRunData
  {
    public float Second;
    public int RestartCount;

    public SpeedRunData(float second, int restartCount)
    {
      Second = second;
      RestartCount = restartCount;
    }
  }

  [System.Serializable] 
  public class ClearData
  {
    public int Chapter;
    public int Stage;
    public bool IsNormalClear;  //old

    public int ClearDifficulty;

    public bool IsAnyPerfect = false;
    public int PerfectDifficulty;

    public ClearData(int chapter, int stage, int clearDifficulty)
    {
      Chapter = chapter;
      Stage = stage;
      this.ClearDifficulty = clearDifficulty;      
    }

    public IDifficultyService.Difficulty GetClearDifficulty()
      => (IDifficultyService.Difficulty)ClearDifficulty;

    public void UpdateDifficulty(IDifficultyService.Difficulty difficulty)
    {
      var newDifficultyIndex = (int)difficulty;
      ClearDifficulty = UnityEngine.Mathf.Max(newDifficultyIndex, ClearDifficulty);
    }

    public bool TryGetTopPerfectDifficulty(out IDifficultyService.Difficulty difficulty)
    {
      difficulty = (IDifficultyService.Difficulty)ClearDifficulty;
      return IsAnyPerfect;
    }

    public bool IsPerfect(IDifficultyService.Difficulty difficulty)
      => IsAnyPerfect && (int)difficulty <= ClearDifficulty;

    public void UpdatePerfect(IDifficultyService.Difficulty difficulty)
    {
      IsAnyPerfect = true;
      PerfectDifficulty = UnityEngine.Mathf.Max(PerfectDifficulty, (int)difficulty);
    }
  }

  public List<ClearData> ClearDatas = new();
  public List<SpeedRunData> speedRunDatas = new();
  public SpeedRunData RunningSpeedrunData = null;

  public void UpdateVersion(int version)
  {
    if (this.DataVersion >= version)
      return;

    switch (version)
    {
      case 1:
        {
          foreach (var clearData in ClearDatas)
            clearData.UpdateDifficulty(clearData.IsNormalClear ? IDifficultyService.Difficulty.Normal : IDifficultyService.Difficulty.Easy);
        }
        break;
    }    

    this.DataVersion = version;
  }
}
