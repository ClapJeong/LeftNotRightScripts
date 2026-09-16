using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LR.Manager.UI
{
  public class UIDepthService : IUIDepthService
  {
    private readonly Stack<GameObject> previousDepthSelectedGameObjects = new();
    private readonly Stack<GameObject> newDepthFirstSelectedGameObjects = new();

    public void UpdateFocusingSelectedGameObject()
    {
      var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;

      if (currentSelectedGameObject == null &&
        newDepthFirstSelectedGameObjects.TryPeek(out var currentDepthSelectedGameObject))
      {
        EventSystem.current.SetSelectedGameObject(currentDepthSelectedGameObject);
      }
    }

    public void LowerDepth()
    {
      newDepthFirstSelectedGameObjects.Pop();
      if (previousDepthSelectedGameObjects.TryPop(out var previousSelectedGameObject))
        EventSystem.current?.SetSelectedGameObject(previousSelectedGameObject);
    }

    public void RaiseDepth(GameObject newDepthFirstSelectingGameObject)
    {
      var lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
      previousDepthSelectedGameObjects.Push(lastSelectedGameObject);
      newDepthFirstSelectedGameObjects.Push(newDepthFirstSelectingGameObject);
      EventSystem.current.SetSelectedGameObject(newDepthFirstSelectingGameObject);
    }

    public void SelectTopObject()
    {
      var currentSelectedGameObject = newDepthFirstSelectedGameObjects.Peek();
      EventSystem.current.SetSelectedGameObject(currentSelectedGameObject);
    }
  }
}