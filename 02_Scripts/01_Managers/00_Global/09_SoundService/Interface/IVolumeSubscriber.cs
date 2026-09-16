using UnityEngine.Events;

namespace LR.Manager.Sound
{
  public interface IVolumeSubscriber
  {
    public void SubscribeOnVolumeChanged(UnityAction<VolumeType, float> unityAction);
    public void UnsubscribeOnVolumeChanged(UnityAction<VolumeType, float> unityAction);
  }
}
