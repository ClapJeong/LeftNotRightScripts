using UnityEngine.Events;

namespace LR.Manager.Input
{
  public interface IRightInputStateService
  {
    public RightInputState CurrentRightInputState { get; }

    public void SubscribeRightInputStateChanged(UnityAction<RightInputState> unityAction, bool invokeInitialize = true);

    public void UnsubscribeRightInputStateChanged(UnityAction<RightInputState> unityAction);
  }
}
