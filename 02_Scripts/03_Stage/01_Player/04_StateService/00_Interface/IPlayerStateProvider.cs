using LR.Stage.Player.Enum;

namespace LR.Stage.Player
{
  public interface IPlayerStateProvider
  {
    public PlayerState GetCurrentState();
  }
}