using LR.Manager.Input;
using LR.Manager.Stage;
using LR.Stage.Player.Enum;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace LR.Stage.Player
{
  public class PlayerInputActionService : 
    IPlayerInputActionController, 
    IPlayerInputActionSubscriber,
    IPlayerInputStateProvider
  {
    private class InputActionSet : IDisposable
    {
      private readonly IStageStateProvider stageStateProvider; 
      private readonly IInputActionSubscriber inputActionSubscriber;
      private readonly IInputActionProvider inputActionProvider;
      private readonly UnityAction onPerformed;
      private readonly UnityAction onCanceled;

      private LRInputType inputType;
      private bool isSubscribed = false;
      private bool enableInputAction = false;

      public InputActionSet(
        IStageStateProvider stageStateProvider,
        IInputActionSubscriber inputActionSubscriber,
        IInputActionProvider inputActionProvider,
        UnityAction onPerformed, 
        UnityAction onCanceled)
      {
        this.stageStateProvider = stageStateProvider;
        this.inputActionSubscriber = inputActionSubscriber;
        this.inputActionProvider = inputActionProvider;
        this.onPerformed = onPerformed;
        this.onCanceled = onCanceled;
      }

      public void Register(LRInputType inputType)
      {
        if(isSubscribed)
          Unregister();

        this.inputType = inputType;
        inputActionSubscriber.SubscribePhase(inputType, OnPerformed, InputPhase.Performed);
        inputActionSubscriber.SubscribePhase(inputType, OnCanceled, InputPhase.Canceled);
        isSubscribed = true;

        if (stageStateProvider.IsPlayingState && IsPressed())
          onPerformed?.Invoke();
      }

      public void Unregister()
      {
        if (stageStateProvider.IsPlayingState && IsPressed())
          onCanceled?.Invoke();

        inputActionSubscriber.UnsubscribePhase(inputType, OnPerformed, InputPhase.Performed);
        inputActionSubscriber.UnsubscribePhase(inputType, OnCanceled, InputPhase.Canceled);
        isSubscribed = false;
      }

      public void Dispose()
      {
        Unregister();
      }

      public void Enable(bool isEnable)
      {
        if (enableInputAction == isEnable)
          return;

        var wasPressed = IsPressed();
        enableInputAction = isEnable;

        if (isEnable && wasPressed)
        {
          onPerformed?.Invoke();
        }
        else if(!isEnable && wasPressed)
        {
          onCanceled?.Invoke();
        }
      }

      public bool IsPressed()
        => inputActionProvider.IsPressed(inputType);

      private void OnPerformed()
      {
        if (enableInputAction == false)
          return;

        onPerformed?.Invoke();
      }

      private void OnCanceled()
      {
        if (enableInputAction == false)
          return;

        onCanceled?.Invoke();
      }
    }

    private PlayerType currentPlayerType;

    private readonly Dictionary<Direction, InputActionSet> inputActionSets = new();
    private readonly UnityEvent<Direction> onPerformed = new();
    private readonly UnityEvent<Direction> onCanceled = new();

    public PlayerInputActionService(
      PlayerType playerType,
      IStageStateProvider stageStateProvider,
      IInputActionSubscriber inputActionSubscriber,
      IInputActionProvider inputActionProvider)
    {
      this.currentPlayerType = playerType;
      
      foreach(Direction direction in System.Enum.GetValues(typeof(Direction)))
      {
        var inputActionSet = new InputActionSet(
          stageStateProvider,
          inputActionSubscriber,
          inputActionProvider,
          () => onPerformed?.Invoke(direction),
          () => onCanceled.Invoke(direction));
        inputActionSet.Register(direction.ParseToLRInputType(playerType));
        inputActionSets[direction] = inputActionSet;
      }
    }

    public void RebindToOpposite()
    {
      currentPlayerType = currentPlayerType.ParseOpposite();
      foreach(var pair in inputActionSets)
      {
        var direction = pair.Key;
        var inputActionSet = pair.Value;
        inputActionSet.Register(direction.ParseToLRInputType(currentPlayerType));
      }
    }

    public void EnableInputAction(Direction direction, bool enable)
    {      
      if (inputActionSets.TryGetValue(direction, out var set))
        set.Enable(enable);
    }

    public void EnableAllInputActions(bool enable)
    {
      foreach (var set in inputActionSets.Values)
        set.Enable(enable);
    }

    public void SubscribePerformed(UnityAction<Direction> performed)
      => onPerformed.AddListener(performed);

    public void SubscribeCanceled(UnityAction<Direction> canceled)
      => onCanceled.AddListener(canceled);

    public void UnsubscribePerformed(UnityAction<Direction> performed)
      => onPerformed.RemoveListener(performed);

    public void UnsubscribeCanceled(UnityAction<Direction> canceled)
      => onCanceled.RemoveListener(canceled);

    public void Dispose()
    {
      foreach (var set in inputActionSets.Values)
        set.Dispose();
    }

    public bool IsAnyInput()
    {
      foreach (var set in inputActionSets.Values)
        if (set.IsPressed())
          return true;

      return false;
    }

    public bool IsPressing(Direction direction)
    {
      if(inputActionSets.TryGetValue(direction, out var set))
        return set.IsPressed();
      return false;
    }
  }
}