using System;
using LR.Stage.Player.Enum;

namespace LR.Stage.Player
{
  public interface IPlayerStateController : IDisposable
  {
    public void ChangeState(PlayerState playerState);
  }
}