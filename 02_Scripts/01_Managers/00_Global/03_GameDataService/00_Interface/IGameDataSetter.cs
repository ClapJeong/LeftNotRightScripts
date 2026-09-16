namespace LR.Manager.GameDataManager
{
  public interface IGameDataSetter
  {
    public void SetSelectedStage(int chapter, int stage);

    public void ResetSelectedStage();
  }
}
