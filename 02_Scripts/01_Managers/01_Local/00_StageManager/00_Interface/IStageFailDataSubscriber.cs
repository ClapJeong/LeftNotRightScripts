using UnityEngine.Events;

namespace LR.Manager.Stage
{
  public interface IStageFailDataSubscriber
  {
    public enum DataType
    {
      Restart,
      Failure,
      BonusTime,
    }

    public void SubscribeOnChanged(DataType dataType, UnityAction<float> unityAction);
    public void UnsubscribeOnChanged(DataType dataType, UnityAction<float> unityAction);
  }
}
