namespace LR.Stage.Player
{
  public interface IPlayerInputStateProvider
  {
    public bool IsAnyInput();

    public bool IsPressing(Direction direction);
  }
}
