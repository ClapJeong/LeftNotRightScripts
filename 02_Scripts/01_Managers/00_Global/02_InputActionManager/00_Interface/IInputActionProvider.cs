namespace LR.Manager.Input
{
  public interface IInputActionProvider
  {
    public bool IsPressed(LRInputType inputActionType);
  }
}
