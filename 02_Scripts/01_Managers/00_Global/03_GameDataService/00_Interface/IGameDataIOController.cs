using Cysharp.Threading.Tasks;
using System.Threading;

namespace LR.Manager.GameDataManager
{
  public interface IGameDataIOController
  {
    public UniTask SaveDataAsync(CancellationToken token = default);

    public UniTask LoadDataAsync(bool resetData, CancellationToken token = default);

    public void AddClearData(int chapter, int stage, IDifficultyService.Difficulty difficulty);

    public void UpdatePerfectData(int chapter, int stage, IDifficultyService.Difficulty difficulty);

    public void AddSpeedRunData(float time, int restartCount);

    public void UpdateDemoPlayed(bool isDemoPlaying);

    public void UpdateSpeedrunResumeData(int chater, int stage);
    public void UpdateSpeedrunResumeTimeData(float time, int count);
    public void ResetSpeedrunResumeData();

    public void ResetData();
  }
}
