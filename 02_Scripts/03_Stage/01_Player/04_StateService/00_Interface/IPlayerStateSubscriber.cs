using UnityEngine.Events;
using LR.Stage.Player.Enum;

namespace LR.Stage.Player
{
  public interface IPlayerStateSubscriber
  {
    public void SubscribeOnEnter(PlayerState playerState, UnityAction onEnter);

    public void UnsubscribeOnEnter(PlayerState playerState, UnityAction onEnter);

    public void SubscribeOnExit(PlayerState playerState, UnityAction onExit);

    public void UnsubscribeOnExit(PlayerState playerState, UnityAction onExit);
  }
}