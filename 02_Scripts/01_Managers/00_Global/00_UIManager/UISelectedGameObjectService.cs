using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LR.Manager.UI
{
  public class UISelectedGameObjectService : IUISelectedGameObjectService
  {
    private readonly UnityEvent<GameObject> onSelectEnter = new();
    private readonly UnityEvent<GameObject> onSelectExit = new();
    private GameObject previousSelectedObject;

    public void UpdateDetectingSelectedObject()
    {
      var currentSelectedObject = EventSystem.current.currentSelectedGameObject;
      var isPreviousGameObjectExist = previousSelectedObject != null;
      var isCurrentGameObjectExist = currentSelectedObject != null;
      var isGameObjectChanged = previousSelectedObject != currentSelectedObject;

      if (isPreviousGameObjectExist &&
          isCurrentGameObjectExist &&
          isGameObjectChanged)
      {
        onSelectExit?.Invoke(previousSelectedObject);
        onSelectEnter?.Invoke(currentSelectedObject);
      }
      else if (isPreviousGameObjectExist &&
              isCurrentGameObjectExist == false)
      {
        onSelectExit?.Invoke(previousSelectedObject);
      }
      else if (isPreviousGameObjectExist == false &&
              isCurrentGameObjectExist)
      {
        onSelectEnter?.Invoke(currentSelectedObject);
      }

      previousSelectedObject = currentSelectedObject;
    }


    public void SetSelectedObject(GameObject gameObject)
      => EventSystem.current.SetSelectedGameObject(gameObject);

    public void SubscribeEvent(IUISelectedGameObjectService.EventType type, UnityAction<GameObject> action)
    {
      switch (type)
      {
        case IUISelectedGameObjectService.EventType.OnEnter:
          onSelectEnter.AddListener(action);
          break;

        case IUISelectedGameObjectService.EventType.OnExit:
          onSelectExit.AddListener(action);
          break;
      }
    }

    public void UnsubscribeEvent(IUISelectedGameObjectService.EventType type, UnityAction<GameObject> action)
    {
      switch (type)
      {
        case IUISelectedGameObjectService.EventType.OnEnter:
          onSelectEnter.RemoveListener(action);
          break;

        case IUISelectedGameObjectService.EventType.OnExit:
          onSelectExit.RemoveListener(action);
          break;
      }
    }
  }
}