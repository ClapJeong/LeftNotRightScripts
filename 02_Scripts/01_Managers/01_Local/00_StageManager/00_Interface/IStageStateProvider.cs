namespace LR.Manager.Stage
{
  public interface IStageStateProvider
  {
    public bool IsPlayingState { get; }

    public StageEnum.State GetState();
  }
}