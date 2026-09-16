namespace LR.Manager.Stage
{
  public interface IStageResultHandler
  {
    public void Exhausted();

    public void LeftClearEnter();

    public void RightClearEnter();
  }
}