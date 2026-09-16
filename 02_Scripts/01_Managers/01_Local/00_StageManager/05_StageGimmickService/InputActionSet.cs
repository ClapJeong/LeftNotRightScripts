using LR.Manager.Input;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace LR.Manager.Stage.Gimmick
{
  public class InputActionSet : IDisposable
  {
    private readonly IInputActionSubscriber inputActionSubscriber;

    private bool inputEnable = false;

    public readonly List<Direction> leftInputs = new();
    public readonly List<Direction> rightInputs = new ();

    public int LeftCount
      => leftInputs.Count;

    public int RightCount
      => rightInputs.Count;

    private readonly UnityAction<Direction> onLeftPerformed;
    private readonly UnityAction<Direction> onRightPerformed;

    public InputActionSet(
      IInputActionSubscriber inputActionSubscriber,
      UnityAction<Direction> onLeftPerformed = null,
      UnityAction<Direction> onRightPerformed = null)
    {
      this.inputActionSubscriber = inputActionSubscriber;

      this.onLeftPerformed = onLeftPerformed;
      this.onRightPerformed = onRightPerformed;

      SubscribeInputActions();
    }

    public void Enable(bool isEnable)
    {
      inputEnable = isEnable;
    }

    private void SubscribeInputActions()
    {
      inputActionSubscriber.Subscribe(LRInputType.LeftUp, OnLeftUp);
      inputActionSubscriber.Subscribe(LRInputType.LeftLeft, OnLeftLeft);
      inputActionSubscriber.Subscribe(LRInputType.LeftDown, OnLeftDown);
      inputActionSubscriber.Subscribe(LRInputType.LeftRight, OnLeftRight);

      inputActionSubscriber.Subscribe(LRInputType.RightUp, OnRightUp);
      inputActionSubscriber.Subscribe(LRInputType.RightLeft, OnRightLeft);
      inputActionSubscriber.Subscribe(LRInputType.RightDown, OnRightDown);
      inputActionSubscriber.Subscribe(LRInputType.RightRight, OnRightRight);
    }

    private void UnsubscribeInputActions()
    {
      inputActionSubscriber.Unsubscribe(LRInputType.LeftUp, OnLeftUp);
      inputActionSubscriber.Unsubscribe(LRInputType.LeftLeft, OnLeftLeft);
      inputActionSubscriber.Unsubscribe(LRInputType.LeftDown, OnLeftDown);
      inputActionSubscriber.Unsubscribe(LRInputType.LeftRight, OnLeftRight);

      inputActionSubscriber.Unsubscribe(LRInputType.RightUp, OnRightUp);
      inputActionSubscriber.Unsubscribe(LRInputType.RightLeft, OnRightLeft);
      inputActionSubscriber.Unsubscribe(LRInputType.RightDown, OnRightDown);
      inputActionSubscriber.Unsubscribe(LRInputType.RightRight, OnRightRight);
    }

    private void UpdateLeftInputs(Direction direction, bool isPerformed)
    {
      if (isPerformed && !leftInputs.Contains(direction))
        leftInputs.Add(direction);
      else if(!isPerformed && leftInputs.Contains(direction))
        leftInputs.Remove(direction);
    }

    private void OnLeftUp(InputPhase inputPhase)
    {
      var direction = Direction.Up;
      var isPerformed = inputPhase == InputPhase.Performed;
      UpdateLeftInputs(direction, isPerformed);

      if (!inputEnable)
        return;

      if (inputPhase == InputPhase.Performed)
        onLeftPerformed?.Invoke(direction);
    }

    private void OnLeftRight(InputPhase inputPhase)
    {
      var direction = Direction.Right;
      var isPerformed = inputPhase == InputPhase.Performed;
      UpdateLeftInputs(direction, isPerformed);

      if (!inputEnable)
        return;

      if (inputPhase == InputPhase.Performed)
        onLeftPerformed?.Invoke(direction);
    }

    private void OnLeftDown(InputPhase inputPhase)
    {
      var direction = Direction.Down;
      var isPerformed = inputPhase == InputPhase.Performed;
      UpdateLeftInputs(direction, isPerformed);

      if (!inputEnable)
        return;

      if (inputPhase == InputPhase.Performed)
        onLeftPerformed?.Invoke(direction);
    }

    private void OnLeftLeft(InputPhase inputPhase)
    {
      var direction = Direction.Left;
      var isPerformed = inputPhase == InputPhase.Performed;
      UpdateLeftInputs(direction, isPerformed);

      if (!inputEnable)
        return;

      if (inputPhase == InputPhase.Performed)
        onLeftPerformed?.Invoke(direction);
    }

    private void UpdateRightInputs(Direction direction, bool isPerformed)
    {
      if (isPerformed && !rightInputs.Contains(direction))
        rightInputs.Add(direction);
      else if (!isPerformed && rightInputs.Contains(direction))
        rightInputs.Remove(direction);
    }


    private void OnRightUp(InputPhase inputPhase)
    {
      var direction = Direction.Up;
      var isPerformed = inputPhase == InputPhase.Performed;
      UpdateRightInputs(direction, isPerformed);

      if (!inputEnable)
        return;

      if (inputPhase == InputPhase.Performed)
        onRightPerformed?.Invoke(direction);
    }

    private void OnRightRight(InputPhase inputPhase)
    {
      var direction = Direction.Right;
      var isPerformed = inputPhase == InputPhase.Performed;
      UpdateRightInputs(direction, isPerformed);

      if (!inputEnable)
        return;

      if (inputPhase == InputPhase.Performed)
        onRightPerformed?.Invoke(direction);
    }

    private void OnRightDown(InputPhase inputPhase)
    {
      var direction = Direction.Down;
      var isPerformed = inputPhase == InputPhase.Performed;
      UpdateRightInputs(direction, isPerformed);

      if (!inputEnable)
        return;

      if (inputPhase == InputPhase.Performed)
        onRightPerformed?.Invoke(direction);
    }

    private void OnRightLeft(InputPhase inputPhase)
    {
      var direction = Direction.Left;
      var isPerformed = inputPhase == InputPhase.Performed;
      UpdateRightInputs(direction, isPerformed);

      if (!inputEnable)
        return;

      if (inputPhase == InputPhase.Performed)
        onRightPerformed?.Invoke(direction);
    }

    public void Dispose()
    {
      UnsubscribeInputActions();
    }
  }
}
