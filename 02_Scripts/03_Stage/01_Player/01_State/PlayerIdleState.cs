using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Stage;
using LR.Stage.Player.Enum;
using System;
using UniRx;
using UnityEngine;

namespace LR.Stage.Player
{
  public class PlayerIdleState : IPlayerState
  {
    private readonly PlayerStatus playerStatus;
    private readonly IStageStateProvider stageStateProvider;
    private readonly IPlayerMoveController moveController;
    private readonly IPlayerInputActionSubscriber inputActionSubscriber;
    private readonly IPlayerInputStateProvider inputStateProvider;
    private readonly IPlayerStateController stateController;
    private readonly IPlayerStateProvider playerStateProvider;
    private readonly IPlayerAnimatorController animatorController;
    private readonly IPlayerEffectController effectController;
    private readonly WallHitAnimationTimer wallHitAnimationTimer;

    private IDisposable electricDisposable;

    public PlayerIdleState(
      PlayerStatus playerStatus,
      IStageStateProvider stageStateProvider,
      IPlayerMoveController moveController,
      IPlayerInputActionSubscriber inputActionSubscriber,
      IPlayerInputStateProvider inputStateProvider,
      IPlayerStateController stateController,
      IPlayerStateProvider playerStateProvider,
      IPlayerAnimatorController animatorController,
      IPlayerEffectController effectController,
      WallHitAnimationTimer wallHitAnimationTimer)
    {
      this.playerStatus = playerStatus;
      this.stageStateProvider = stageStateProvider;
      this.moveController = moveController;
      this.inputActionSubscriber = inputActionSubscriber;
      this.inputStateProvider = inputStateProvider;
      this.stateController = stateController;
      this.playerStateProvider = playerStateProvider;
      this.animatorController = animatorController;
      this.effectController = effectController;
      this.wallHitAnimationTimer = wallHitAnimationTimer;
    }

    public void FixedUpdate()
    {
      moveController.ApplyMoveDeceleration();
      moveController.ResetAllDirection();
    }

    public void OnEnter()
    {
      moveController.ResetAllDirection();
      effectController.StopAllEffects(PlayerEffect.Electric);      
      inputActionSubscriber.SubscribePerformed(OnMovePerformed);

      UpdateEnterAnimationAsync().Forget();

      wallHitAnimationTimer.SubscribeOnComplete(OnWallHitTimerComplete);
    }

    public void OnExit()
    {
      electricDisposable?.Dispose();
      inputActionSubscriber.UnsubscribePerformed(OnMovePerformed);
      wallHitAnimationTimer.UnsubscribeOnComplete(OnWallHitTimerComplete);
    }

    private async UniTask UpdateEnterAnimationAsync()
    {
      await UniTask.NextFrame();

      if(moveController.GetCurrentDirection() == Vector2.zero) 

      if (playerStatus.IsElectric.Value)
      {
        animatorController.Play(AnimatorHash.Player.Clip.MoveBlend);
        electricDisposable = playerStatus.IsElectric.Subscribe(OnElectricValueChanged);
      }
      else if (wallHitAnimationTimer.IsWallHitTimerWorking)
      {
        animatorController.Play(AnimatorHash.Player.Clip.WallHit);
      }
      else
      {
        if (!IsAnyInputPerformed())
          animatorController.Play(AnimatorHash.Player.Clip.Idle);
      }
    }

    private bool IsAnyInputPerformed()
    {
      if (playerStateProvider.GetCurrentState() != PlayerState.Idle)// && inputStateProvider.IsAnyInput())
        return true;

      return false;
    }

    private void OnMovePerformed(Direction direction)
    {
      if (!stageStateProvider.IsPlayingState)
        return;

      stateController.ChangeState(PlayerState.Move);
    }

    private void OnWallHitTimerComplete()
    {
      if(playerStatus.IsElectric.Value)
        animatorController.Play(AnimatorHash.Player.Clip.MoveBlend);
      else
        animatorController.Play(AnimatorHash.Player.Clip.Idle);
    }

    private void OnElectricValueChanged(bool isElectric)
    {
      if (!isElectric)
      {
        animatorController.Play(AnimatorHash.Player.Clip.Idle);
      }        
    }
  }
}