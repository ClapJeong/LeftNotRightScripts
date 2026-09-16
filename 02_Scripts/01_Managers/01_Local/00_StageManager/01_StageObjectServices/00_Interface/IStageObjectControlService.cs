using LR.Stage;

namespace LR.Manager.Stage.StageObject
{
  public interface IStageObjectControlService
  {
    public void EnableAll(bool isEnable);

    public void RestartAll();
  }
}