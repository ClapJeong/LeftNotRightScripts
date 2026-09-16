using UnityEngine.Events;

namespace LR.Manager.Input
{
  public interface IInputActionSubscriber
  {
    public void Subscribe(LRInputType inputActionType, UnityAction<InputPhase> phaseUnityAction);

    public void Unsubscribe(LRInputType inputActionType, UnityAction<InputPhase> phaseUnityAction);

    public void SubscribePhase(LRInputType inputActionType, UnityAction unityAction, InputPhase phase);

    public void UnsubscribePhase(LRInputType inputActionType, UnityAction unityAction, InputPhase phase);
  }
}
