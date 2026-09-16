using System;
using UnityEngine.Events;

namespace LR.Stage.Player
{
  public interface IPlayerInputActionSubscriber : IDisposable
  {
    public void SubscribePerformed(UnityAction<Direction> performed);

    public void SubscribeCanceled(UnityAction<Direction> canceled);

    public void UnsubscribePerformed(UnityAction<Direction> performed);

    public void UnsubscribeCanceled(UnityAction<Direction> canceled);
  }
}
