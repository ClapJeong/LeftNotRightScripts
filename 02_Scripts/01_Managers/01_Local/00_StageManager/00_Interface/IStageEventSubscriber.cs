using UnityEngine.Events;

namespace LR.Manager.Stage
{
  public interface IStageEventSubscriber
  {
    public enum StageEventType
    {
      BeforeShowComplete,
      AfterDialogueComplete,

      Begin,
      Pause,
      Resume,
      Complete,

      Exhausted,

      LeftClearEnter,

      RightClearEnter,

      Restart,

      AllClearEnter,
    }

    public void SubscribeOnEvent(StageEventType type, UnityAction action);

    public void UnsubscribeOnEvent(StageEventType type, UnityAction action);

    public void SubscribeRestartDelay(UnityAction<float> action);

    public void UnsubscribeRestartDelay(UnityAction<float> action);
  }
}