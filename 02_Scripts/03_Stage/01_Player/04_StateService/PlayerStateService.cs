using System.Collections.Generic;
using UnityEngine.Events;
using LR.Stage.Player.Enum;

namespace LR.Stage.Player
{
  public class PlayerStateService : IPlayerStateController, IPlayerStateSubscriber, IPlayerStateProvider
  {
    private readonly WallHitAnimationTimer wallHitAnimationTimer;

    private readonly Dictionary<PlayerState, IPlayerState> states = new();
    private readonly Dictionary<PlayerState, UnityEvent> onEnterEvents = new();
    private readonly Dictionary<PlayerState, UnityEvent> onExitEvents = new();

    private PlayerState currentKey = PlayerState.None;

    public PlayerStateService(WallHitAnimationTimer wallHitAnimationTimer)
    {
      this.wallHitAnimationTimer = wallHitAnimationTimer;
    }

    public void AddState(PlayerState type, IPlayerState state)
      => states[type] = state;

    public void RemoveState(PlayerState type, IPlayerState state)
    {
      if (states.ContainsKey(type))
        states.Remove(type);
    }

    #region IPlayerStateController
    public void ChangeState(PlayerState type)
    {
      if (type == currentKey)
        return;

      if(states.TryGetValue(currentKey, out var previousState)) 
        previousState.OnExit();
      onExitEvents.TryInvoke(currentKey);

      currentKey = type;

      if(states.TryGetValue(currentKey, out var currentState))
        currentState.OnEnter();
      onEnterEvents.TryInvoke(currentKey);
    }

    public void FixedUpdate()
    {
      if(states.TryGetValue(currentKey, out var currentState))
        currentState.FixedUpdate();

      wallHitAnimationTimer.OnFixedUpdate();
    }
    #endregion

    #region IPlayerStateProvider
    public PlayerState GetCurrentState()
      => currentKey;
    #endregion

    #region IPlayerStateSubscriber
    public void SubscribeOnEnter(PlayerState playerState, UnityAction onEnter)
      => onEnterEvents.AddEvent(playerState, onEnter);

    public void UnsubscribeOnEnter(PlayerState playerState, UnityAction onEnter)
      => onEnterEvents.RemoveEvent(playerState, onEnter);

    public void SubscribeOnExit(PlayerState playerState, UnityAction onExit)
      => onExitEvents.AddEvent(playerState, onExit);

    public void UnsubscribeOnExit(PlayerState playerState, UnityAction onExit)
      => onExitEvents.RemoveEvent(playerState, onExit);
    #endregion

    public void Dispose()
    {
      if (states.TryGetValue(currentKey, out var currentState))
        currentState.OnExit();
    }
  }
}