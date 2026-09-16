namespace LR.Stage.Player
{
  public interface IPlayerState
  {
    public void OnEnter();

    public void OnExit();

    public void FixedUpdate();
  }
}