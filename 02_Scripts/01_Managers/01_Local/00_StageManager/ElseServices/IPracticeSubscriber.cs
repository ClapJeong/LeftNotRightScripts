using UnityEngine.Events;

namespace LR.Manager.Stage.Practice
{
  public interface IPracticeSubscriber
  {
    public void SubscribeOnPractice(UnityAction<bool> unityAction);

    public void UnsubscribeOnPractice(UnityAction<bool> unityAction);
  }
}
