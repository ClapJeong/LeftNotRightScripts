namespace LR.Manager.GameDataManager
{
  public interface ISpeedRunDataProvider
  {
    public GameData.SpeedRunData GetFastedSpeedRunData();

    public GameData.SpeedRunData GetSafestSpeedRunData();

    public bool TryGetRunningData(out GameData.SpeedRunData data);
  }
}
