using LR.Manager.Input;
using LR.UI;
using System;
using UnityEngine;

namespace LR.Manager.UI
{
  public class UISubmitController : IUISubmitController, IDisposable
  {
    private readonly IUISelectedGameObjectService selectedGameObjectService;
    private readonly IInputActionSubscriber inputActionSubscriber;

    private BaseSubmitView lastSelectedView;
    private BaseSubmitView selectedView;

    public UISubmitController(
      IUISelectedGameObjectService selectedGameObjectService, 
      IInputActionSubscriber inputActionSubscriber)
    {
      this.selectedGameObjectService = selectedGameObjectService;
      this.inputActionSubscriber = inputActionSubscriber;

      inputActionSubscriber.SubscribePhase(LRInputType.RightUp, OnPerformedUp, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.RightRight, OnPerformedRight, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.RightDown, OnPerformedDown, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.RightLeft, OnPerformedLeft, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.UISubmit, OnPerformedSubmit, InputPhase.Performed);

      selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
      selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnExit, OnSelectedGameObjectExit);
    }

    public void Dispose()
    {
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightUp, OnPerformedUp, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightRight, OnPerformedRight, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightDown, OnPerformedDown, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightLeft, OnPerformedLeft, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.UISubmit, OnPerformedSubmit, InputPhase.Performed);

      selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
      selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnExit, OnSelectedGameObjectExit);
    }

    public void Release(BaseSubmitView view)
    {
      if (selectedView == view)
        selectedView = null;
      if(lastSelectedView == view)
        lastSelectedView = view;
    }

    private void OnPerformedUp()
    {
      if ((selectedView as UnityEngine.Object) == null)
      {
        selectedView = null;
        return;
      }

      if (selectedView.IsEnable() == false)
        return;

      var direction = Direction.Up;
      selectedView?.Perform(direction);
    }

    private void OnPerformedRight()
    {
      if ((selectedView as UnityEngine.Object) == null)
      {
        selectedView = null;
        return;
      }

      if (selectedView.IsEnable() == false)
        return;

      var direction = Direction.Right;
      selectedView?.Perform(direction);
    }

    private void OnPerformedDown()
    {
      if ((selectedView as UnityEngine.Object) == null)
      {
        selectedView = null;
        return;
      }

      if (selectedView.IsEnable() == false)
        return;

      var direction = Direction.Down;
      selectedView?.Perform(direction);
    }

    private void OnPerformedLeft()
    {
      if ((selectedView as UnityEngine.Object) == null)
      {
        selectedView = null;
        return;
      }

      if (selectedView.IsEnable() == false)
        return;

      var direction = Direction.Left;
      selectedView?.Perform(direction);
    }

    private void OnPerformedSubmit()
    {
      if ((selectedView as UnityEngine.Object) == null)
      {
        selectedView = null;
        return;
      }

      if (selectedView.IsEnable() == false)
        return;

      selectedView?.Perform();
    }

    private void OnSelectedGameObjectEnter(GameObject gameObject)
    {
      if (gameObject.TryGetComponent<BaseSubmitView>(out var view))
      {
        if(lastSelectedView != null)
        {
          var direction = lastSelectedView.RectTransform.position.y != view.RectTransform.position.y ? Direction.Up
                         : lastSelectedView.RectTransform.position.x == view.RectTransform.position.x ? Direction.Up
                         : lastSelectedView.RectTransform.position.x < view.RectTransform.position.x ? Direction.Left 
                                                                                                     : Direction.Right;
          view.OnSelect(direction);
        }
        else
        {
          view.OnSelect();
        }
        selectedView = view;
        lastSelectedView = selectedView;
      }        
    }

    private void OnSelectedGameObjectExit(GameObject gameObject)
    {
      selectedView?.OnExit();
      selectedView = null;
    }
  }
}