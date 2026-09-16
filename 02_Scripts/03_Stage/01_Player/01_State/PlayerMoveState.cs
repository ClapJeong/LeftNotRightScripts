using UnityEngine;
using LR.Stage.Player.Enum;
using LR.Manager.Sound;
using LR.Table.Player;

namespace LR.Stage.Player
{
  public class PlayerMoveState : IPlayerState
  {
    private readonly PlayerStatus playerStatus;
    private readonly IPlayerStateController stateController;
    private readonly IPlayerInputStateProvider inputStateProvider;
    private readonly IPlayerInputActionSubscriber inputActionSubscriber;
    private readonly IPlayerMoveController moveController;
    private readonly IPlayerReactionController reactionController;
    private readonly IPlayerAnimatorController animatorController;
    private readonly IPlayerEffectController effectController;
    private readonly PlayerType playerType;
    private readonly ISFXController sfxController;
    private readonly PlayerCollisionData collisionData;
    private readonly IPlayerEnergySubscriber playerEnergySubscriber;
    private readonly WallHitAnimationTimer wallHitAnimationTimer;
    private readonly PlayerMovementData movementData;

    private AudioLoopHandle moveClipLoopHandle;
    private PlayerEffect currentEffect;
    private float moveDuration;    
    private Vector2 prevDirection;
    private Vector2 prevPosition;

    public PlayerMoveState(
      PlayerStatus playerStatus,
      IPlayerMoveController moveController,
      IPlayerInputStateProvider inputStateProvider,
      IPlayerInputActionSubscriber inputActionSubscriber,
      IPlayerStateController stateController,
      IPlayerReactionController reactionController,
      IPlayerAnimatorController animatorController,
      IPlayerEffectController effectController,
      PlayerType playerType,
      ISFXController sfxController,
      PlayerCollisionData collisionData,
      IPlayerEnergySubscriber playerEnergySubscriber,
      WallHitAnimationTimer wallHitAnimationTimer,
      PlayerMovementData movementData)
    {
      this.playerStatus = playerStatus;
      this.stateController = stateController;
      this.inputStateProvider = inputStateProvider;
      this.inputActionSubscriber = inputActionSubscriber;
      this.moveController = moveController;
      this.reactionController = reactionController;
      this.animatorController = animatorController;
      this.effectController = effectController;
      this.playerType = playerType;
      this.sfxController = sfxController;
      this.collisionData = collisionData;
      this.playerEnergySubscriber = playerEnergySubscriber;
      this.wallHitAnimationTimer = wallHitAnimationTimer;
      this.movementData = movementData;
    }

    public void FixedUpdate()
    {
      moveController.ApplyMoveAcceleration();

      var currentPosition = moveController.GetCurrentPosition();
      playerStatus.DeltaMoveLength = (currentPosition - prevPosition).magnitude;
      prevPosition = currentPosition;

      UpdateEffect();
      effectController.UpdateMoveDirection(moveController.GetCurrentDirection());

      var currentDirection = moveController.GetCurrentDirection();
      UpdateParameter(currentDirection);      

      if (currentDirection == prevDirection)
        moveDuration = Mathf.Min(moveDuration + Time.fixedDeltaTime, collisionData.WallBumpMaxDuration);
      else
      {
        var isReset =
          (prevDirection.x > 0.0f && currentDirection.x <= 0.0f) ||
          (prevDirection.x < 0.0f && currentDirection.x >= 0.0f) ||
          (prevDirection.y > 0.0f && currentDirection.y <= 0.0f) ||
          (prevDirection.y < 0.0f && currentDirection.y >= 0.0f);
        if (isReset)
        {
          moveDuration = 0.0f;
        }
        else
        {
          var isCurve =
            (Mathf.Abs(prevDirection.x) > 0.0f && Mathf.Abs(currentDirection.x) < Mathf.Abs(prevDirection.x)) ||
            (Mathf.Abs(prevDirection.y) > 0.0f && Mathf.Abs(currentDirection.y) < Mathf.Abs(prevDirection.y));
          if (isCurve)
            moveDuration *= 0.5f;          
        }
      }

      prevDirection = currentDirection;
      reactionController.UpdateMoveDuration(moveDuration);

      if(moveClipLoopHandle != null)
      {
        moveClipLoopHandle.Pitch = playerStatus.IsElectric.Value ? movementData.ElectricModifier : 1.0f;
      }
    }

    public void OnEnter()
    {
      moveClipLoopHandle = sfxController.CreateLoopSource(playerType.ParseToCharacterPositionType(), playerType switch
      {
        PlayerType.Left => SFX.LMove,
        PlayerType.Right => SFX.RMove,
        _ => throw new System.NotImplementedException(),
      });
      var currentDirection = moveController.GetCurrentDirection();
      UpdateParameter(currentDirection);
      prevDirection = currentDirection;

      if (!wallHitAnimationTimer.IsWallHitTimerWorking)
      {
        animatorController.Play(AnimatorHash.Player.Clip.MoveBlend);
      }        
      currentEffect = PlayerEffect.Exhaust;

      moveDuration = 0.0f;
      reactionController.UpdateMoveDuration(moveDuration);
      reactionController.SubscribeOnWallContact(OnWallContact);
      playerEnergySubscriber.SubscribeOnHit(OnWallHit);
      wallHitAnimationTimer.SubscribeOnComplete(OnWallHitTimerComplete);
      playerStatus.DeltaMoveLength = 0.0f;
      prevPosition = moveController.GetCurrentPosition();

      inputActionSubscriber.SubscribeCanceled(OnAnyCanceled);
    }

    public void OnExit()
    {
      playerEnergySubscriber.UnsubscribeOnHit(OnWallHit);
      reactionController.UnsubscribeOnWallContact(OnWallContact);
      moveClipLoopHandle?.Dispose();
      moveClipLoopHandle = null;
      UpdateParameter(Vector2.zero);
      effectController.StopEffect(PlayerEffect.Move);
      effectController.StopEffect(PlayerEffect.Run);
      wallHitAnimationTimer.UnsubscribeOnComplete(OnWallHitTimerComplete);
      playerStatus.DeltaMoveLength = 0.0f;

      inputActionSubscriber.UnsubscribeCanceled(OnAnyCanceled);
    }

    private void OnAnyCanceled(Direction _)
    {
      if (inputStateProvider.IsAnyInput() == false)
      {
        //UnityEngine.Debug.Log($"move {Time.frameCount} {inputStateProvider.IsAnyInput()}");
        stateController.ChangeState(PlayerState.Idle);
      }
    }

    private void OnWallHitTimerComplete()
    {
      animatorController.Play(AnimatorHash.Player.Clip.MoveBlend);
    }

    private void OnWallHit(PlayerType playerType, DamageType damageType)
    {
      if (playerType != this.playerType || damageType != DamageType.WallBump)
        return;

      wallHitAnimationTimer.OnWallHit();
      animatorController.Play(AnimatorHash.Player.Clip.WallHit);
    }

    private void OnWallContact()
    {
      moveDuration = 0.0f;
      reactionController.UpdateMoveDuration(moveDuration);
    }

    private void UpdateEffect()
    {
      var inputNormalized = moveController.GetCurrentInputVelocityNormalized();

      PlayerEffect target =
          inputNormalized >= collisionData.WallBumpVelocityNormalized
          ? PlayerEffect.Run
          : PlayerEffect.Move;

      if (currentEffect == target)
        return;

      effectController.StopEffect(currentEffect);
      effectController.PlayEffect(target);

      currentEffect = target;
    }

    private void UpdateParameter(Vector2 direction)
    {
      var isVerticalMove = direction.x == 0.0f;
      var horizontal = isVerticalMove ? playerType switch
      {
        PlayerType.Left => 1.0f,
        PlayerType.Right => -1.0f,
        _ => throw new System.NotImplementedException(),
      }
      : Mathf.Sign(direction.x);
      animatorController.SetFloat(AnimatorHash.Player.Parameter.Horizontal, horizontal);
    }
  }
}