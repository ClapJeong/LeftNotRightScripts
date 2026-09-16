namespace LR.Stage.Player
{
  public interface IPlayerInputActionController 
  {
    public void EnableInputAction(Direction direction, bool enable);

    public void EnableAllInputActions(bool enable);

    public void RebindToOpposite();
  }
}