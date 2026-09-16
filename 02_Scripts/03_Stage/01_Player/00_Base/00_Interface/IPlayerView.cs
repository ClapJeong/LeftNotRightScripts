using UnityEngine;
using LR.Stage.Player.Enum;
using UnityEngine.Events;

namespace LR.Stage.Player
{
  public interface IPlayerView
  {
    public PlayerType GetPlayerType();

    public GameObject GameObject { get; }

    public Transform Transform { get; }

    public void SubscribeOnCollisionEnter2D(UnityAction<Collision2D> onCollisionEnter);

    public void UnsubscribeOnCollisionEnter2D(UnityAction<Collision2D> onCollisionEnter);

    public void SubscribeOnCollisionExit2D(UnityAction<Collision2D> onCollisionExit);

    public void UnsubscribeOnCollisionExit2D(UnityAction<Collision2D> onCollisionExit);
  }
}