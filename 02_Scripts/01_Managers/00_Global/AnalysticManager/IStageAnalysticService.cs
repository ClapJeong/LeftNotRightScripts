namespace LR.Manager.Analystic
{
  public interface IStageAnalysticService
  {
    public void SendStageClearEvent(int index, int deathCount, int easyRestartCount, int hardRestartCount);
  }

  public class StageEvent : Unity.Services.Analytics.Event
  {
    public StageEvent() : base("stageEvent")
    {

    }

    public int StageIndex { set { SetParameter("stageIndex", value); } }
    public int DeathCount { set { SetParameter("deathCount", value); } }
    public int EasyRestartCount { set { SetParameter("easyRestartCount", value); } }
    public int HardRestartCount { set { SetParameter("hardRestartCount", value); } }
  }
}
