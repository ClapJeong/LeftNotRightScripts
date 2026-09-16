using System;
using UnityEngine.Events;

public class SubscribeHandle : IDisposable
{
  private readonly UnityEvent onSubscribe = new();
  private readonly UnityEvent onUnsubscribe = new();
  private bool isSubscribing;

  public SubscribeHandle(UnityAction onSubscribe, UnityAction onUnsubscribe)
  {
    this.onSubscribe.AddListener(onSubscribe);
    this.onUnsubscribe.AddListener(onUnsubscribe);
  }

  public void Subscribe()
  {
    if (isSubscribing)
      return;

    onSubscribe?.Invoke();
    isSubscribing = true;
  }

  public void Unsubscribe()
  {
    if (!isSubscribing)
      return;

    onUnsubscribe?.Invoke();
    isSubscribing = false;
  }

  public void Dispose()
    => Unsubscribe();
}