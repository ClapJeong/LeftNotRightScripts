using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using LR.UI.Enum;

namespace LR.UI
{
  public interface IUIView: IUIVisibleStateContainer
  {
    public void SubscribeEvent(ViewEvent eventType, UnityAction action);

    public void UnsubscribeEvent(ViewEvent eventType, UnityAction action);

    public UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default);

    public UniTask HideAsync(bool isImmediately = false, CancellationToken token = default);

    public void DestroySelf();
  }
}