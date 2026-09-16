using UnityEngine.Events;

namespace LR.Manager.Device
{
  public enum LRDeviceType
  {
    Keyboard,
    XBox,
    Switch,
    DualShock,
  }

  public interface IDeviceEvnetSubscriber
  {
    public void SubscribeDeviceEvent(UnityAction<LRDeviceType> onChanged);

    public void UnsubscribeDeviceEvent(UnityAction<LRDeviceType> onChanged);
  }
}
