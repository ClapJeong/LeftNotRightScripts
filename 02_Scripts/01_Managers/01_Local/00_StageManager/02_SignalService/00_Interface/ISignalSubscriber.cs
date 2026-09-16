using UnityEngine.Events;

namespace LR.Manager.Stage.Signal
{
  public interface ISignalSubscriber
  {
    public void SubscribeSignalActivate(int key, UnityAction<bool> activate);

    public void UnsubscribeSignalActivate(int key, UnityAction<bool> activate);

    public void SubscribeSignalDeactivate(int key, UnityAction<bool> deactivate);

    public void UnsubscribeSignalDeactivate(int key, UnityAction<bool> deactivate);

    public void SubscribeIDActivate(int key, int triggerID, UnityAction<int> activate);

    public void UnsubscribeIDActivate(int key, int triggerID, UnityAction<int> activate);

    public void SubscribeIDDeactivate(int key, int triggerID, UnityAction<int> deactivate);

    public void UnsubscribeIDDeactivate(int key, int triggerID, UnityAction<int> deactivate);
  }
}