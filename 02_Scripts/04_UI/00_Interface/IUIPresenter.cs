using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace LR.UI
{
  public interface IUIPresenter : IUIVisibleStateContainer, IDisposable
  {
    public UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default);

    public UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default);

    public IDisposable AttachOnDestroy(GameObject target);
  }
}