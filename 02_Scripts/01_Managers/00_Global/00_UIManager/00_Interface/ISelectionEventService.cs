using UnityEngine;
using UnityEngine.Events;

namespace LR.Manager.UI
{
  public interface IUISelectedGameObjectService
  {
    public enum EventType
    {
      OnEnter,
      OnExit,
    }

    public void SetSelectedObject(GameObject gameObject);

    public void SubscribeEvent(EventType type, UnityAction<GameObject> action);

    public void UnsubscribeEvent(EventType type, UnityAction<GameObject> action);
  }
}