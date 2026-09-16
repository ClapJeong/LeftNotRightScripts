using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public static class GameObjectExtension
{
  public static IDisposable AttachDisposable(this GameObject gameObject, IDisposable disposable)
    => gameObject.OnDestroyAsObservable().Subscribe(_ => disposable.Dispose());
}