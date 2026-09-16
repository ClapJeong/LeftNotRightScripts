using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.TriggerTile
{
  public interface ITriggerEventSubscriber
  {
    public void SubscribeOnEnter(UnityAction<Collider2D> onEnter);

    public void UnsubscribeOnEnter(UnityAction<Collider2D> onEnter);

    public void SubscribeOnExit(UnityAction<Collider2D> onExit);

    public void UnsubscribeOnExit(UnityAction<Collider2D> onExit);
  }
}