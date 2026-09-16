using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace LR.Manager.Input
{
  public class InputActionManager :  
    MonoBehaviour,
    InputSystem_Actions.IPlayerActions, 
    IInputActionSubscriber,
    IInputActionProvider,
    IRightInputStateService
  {
    private readonly Dictionary<LRInputType, UnityEvent<InputPhase>> phaseEvents = new();
    private readonly Dictionary<LRInputType, UnityEvent> performedEvents = new();
    private readonly Dictionary<LRInputType, UnityEvent> canceledEvents = new();
    private readonly Dictionary<LRInputType, bool> isPerforming = new();
    private readonly UnityEvent<RightInputState> onRightInputStateChanged = new();
    public RightInputState CurrentRightInputState
      => currentRightInputState;

    [SerializeField] private InputActionAsset inputActionMap;


    private InputSystem_Actions actions;
    private RightInputState currentRightInputState;

    void Awake()
    {
      foreach (LRInputType lrInputType in System.Enum.GetValues(typeof(LRInputType)))
        isPerforming[lrInputType] = false;

      actions = new InputSystem_Actions();
      actions.Player.SetCallbacks(this);
      actions.Enable();

      var currentRightInputStateIndex = PlayerPrefs.GetInt(PlayerPrefsName.RightInputState, 0);
      currentRightInputState = (RightInputState)currentRightInputStateIndex;
    }

    public void OnLeftMove(InputAction.CallbackContext context)
    {
      OnInputAction(context, LRInputType.LeftAny);

      if (context.phase != InputActionPhase.Performed && context.phase != InputActionPhase.Canceled)
        return;

      ParseToDirections(context, out var performedDirections, out var canceledDirections);
      //ParseToDirections(context.ReadValue<Vector2>(), out var performedDirections, out var canceledDirections);

      foreach (var performedDirection in performedDirections)
      {
        var lrInputType = performedDirection.ParseToLeftInputActionType();
        OnPerformed(lrInputType);
      }        
      foreach (var canceledDirection in canceledDirections)
      {
        var lrInputType = canceledDirection.ParseToLeftInputActionType();
        OnCanceled(lrInputType);
      }
    }

    public void OnRightMove(InputAction.CallbackContext context)
    {
      OnInputAction(context, LRInputType.RightAny);

      if (context.phase != InputActionPhase.Performed && context.phase != InputActionPhase.Canceled)
        return;

      ParseToDirections(context, out var performedDirections, out var canceledDirections);
      //ParseToDirections(context.ReadValue<Vector2>(), out var performedDirections, out var canceledDirections);

      foreach (var performedDirection in performedDirections)
      {
        var lrInputType = performedDirection.ParseToRightInputActionType();
        OnPerformed(lrInputType);
      }
      foreach (var canceledDirection in canceledDirections)
      {
        var lrInputType = canceledDirection.ParseToRightInputActionType();
        OnCanceled(lrInputType);
      }
    }

    public void OnRightDefaultMoveInput(InputAction.CallbackContext context)
    {
      if (context.phase == InputActionPhase.Performed)
      {
        ChangeRightInputState(RightInputState.Arrow);
      }
    }

    public void OnRightSubMoveInput(InputAction.CallbackContext context)
    {
      if(context.phase == InputActionPhase.Performed)
      {
        ChangeRightInputState(RightInputState.JIKL);
      }
    }

    private void ChangeRightInputState(RightInputState rightInputState)
    {
      if (currentRightInputState == rightInputState)
        return;

      currentRightInputState = rightInputState;

      PlayerPrefs.GetInt(PlayerPrefsName.RightInputState, (int)currentRightInputState);
      onRightInputStateChanged?.Invoke(currentRightInputState);
    }

    public void OnSkipDialogue(InputAction.CallbackContext context)
    {
      OnInputAction(context, LRInputType.DialogueSkip);
    }

    public void OnStageRestart(InputAction.CallbackContext context)
    {
      OnInputAction(context, LRInputType.StageRestart);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
      OnInputAction(context, LRInputType.Pause);
    }

    public void OnPractice(InputAction.CallbackContext context)
    {
      OnInputAction(context, LRInputType.Practice);
    }

    public void OnUISubmit(InputAction.CallbackContext context)
    {
      OnInputAction(context, LRInputType.UISubmit);
    }

    private void OnInputAction(InputAction.CallbackContext context, LRInputType lrInputType)
    {
      if (context.phase == InputActionPhase.Performed)
      {
        OnPerformed(lrInputType);        
      }        
      else if (context.phase == InputActionPhase.Canceled)
      {
        OnCanceled(lrInputType);        
      }      
    }

    private void ParseToDirections(InputAction.CallbackContext ctx,
    out List<Direction> performedDirections,
    out List<Direction> canceledDirections)
    {
      performedDirections = new();
      canceledDirections = new();

      Vector2 value = ctx.ReadValue<Vector2>();

      // 아날로그(스틱) 기준
      bool left = value.x < -0.5f;
      bool right = value.x > 0.5f;
      bool up = value.y > 0.5f;
      bool down = value.y < -0.5f;

      // 디지털(WASD, 방향키, DPad) 보정
      foreach (var control in ctx.action.controls)
      {
        if (control is not ButtonControl button || !button.isPressed)
          continue;

        switch (button.path)
        {
          case "/Keyboard/a":
          case "left":
          case "/Keyboard/leftArrow":
            left = true;
            break;

          case "/Keyboard/d":
          case "right":
          case "/Keyboard/rightArrow":
            right = true;
            break;

          case "/Keyboard/w":
          case "up":
          case "/Keyboard/upArrow":
            up = true;
            break;

          case "/Keyboard/s":
          case "down":
          case "/Keyboard/downArrow":
            down = true;
            break;
        }
      }

      (right ? performedDirections : canceledDirections).Add(Direction.Right);
      (left ? performedDirections : canceledDirections).Add(Direction.Left);
      (up ? performedDirections : canceledDirections).Add(Direction.Up);
      (down ? performedDirections : canceledDirections).Add(Direction.Down);
    }

    private void ParseToDirections(Vector2 value, out List<Direction> performedDirections, out List<Direction> canceledDirections)
    {
      performedDirections = new();
      canceledDirections = new();

      var rightTarget = value.x > 0.5f ? performedDirections : canceledDirections;
      rightTarget.Add(Direction.Right);
      var leftTarget = value.x < -0.5f ? performedDirections : canceledDirections;
      leftTarget.Add(Direction.Left);

      var upTarget = value.y > 0.5f ? performedDirections : canceledDirections;
      upTarget.Add(Direction.Up);
      var downTarget = value.y < -0.5f ? performedDirections : canceledDirections;
      downTarget.Add(Direction.Down);
    }

    private void OnPerformed(LRInputType inputActionType)
    {
      var prev = isPerforming[inputActionType];
      isPerforming[inputActionType] = true;
      
      if (prev == false)
      {
        performedEvents.TryInvoke(inputActionType);
        phaseEvents.TryInvoke(inputActionType, InputPhase.Performed);
      }              
    }

    private void OnCanceled(LRInputType inputActionType)
    {
      var prev = isPerforming[inputActionType];
      isPerforming[inputActionType] = false;
      
      if (prev == true)
      {
        canceledEvents.TryInvoke(inputActionType);
        phaseEvents.TryInvoke(inputActionType, InputPhase.Canceled);
      }        
    }

    #region IInputActionSubscriber
    public void Subscribe(LRInputType inputActionType, UnityAction<InputPhase> phaseUnityAction)
      => phaseEvents.AddEvent(inputActionType, phaseUnityAction);

    public void Unsubscribe(LRInputType inputActionType, UnityAction<InputPhase> phaseUnityAction) 
      => phaseEvents.RemoveEvent(inputActionType, phaseUnityAction);

    public void SubscribePhase(LRInputType inputActionType, UnityAction unityAction, InputPhase phase)
    {
      switch (phase)
      {
        case InputPhase.Performed: performedEvents.AddEvent(inputActionType, unityAction); break;
        case InputPhase.Canceled: canceledEvents.AddEvent(inputActionType, unityAction); break;
      }
    }

    public void UnsubscribePhase(LRInputType inputActionType, UnityAction unityAction, InputPhase phase)
    {
      switch (phase)
      {
        case InputPhase.Performed: performedEvents.RemoveEvent(inputActionType, unityAction); break;
        case InputPhase.Canceled: canceledEvents.RemoveEvent(inputActionType, unityAction); break;
      }
    }
    #endregion

    #region IInputActionProvider
    public bool IsPressed(LRInputType inputActionType)
      => isPerforming[inputActionType];
    #endregion

    #region IRightInputStateService
    public void SubscribeRightInputStateChanged(UnityAction<RightInputState> unityAction, bool invokeInitialize = true)
    {
      if (this == null)
        return;

      if (invokeInitialize)
        unityAction?.Invoke(CurrentRightInputState);

      onRightInputStateChanged.AddListener(unityAction);
    }

    public void UnsubscribeRightInputStateChanged(UnityAction<RightInputState> unityAction)
      => onRightInputStateChanged.RemoveListener(unityAction);    
    #endregion
  }
}