using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using LR.Stage.Player.Enum;
using LR.Table.Player;
using UnityEngine;

namespace LR.Stage.Player
{
  public class PlayerStunState : IPlayerState
  {
    private readonly IPlayerMoveController moveController;
    private readonly IPlayerStateController stateController;
    private readonly IPlayerAnimatorController animatorController;
    private readonly IPlayerInputStateProvider inputStateProvider;
    private readonly IPlayerEffectController effectController;
    private readonly PlayerStunData stunData;
    private readonly SpriteRenderer spriteRenderer;
    private readonly PlayerType playerType;

    private MaterialPropertyBlock shearBlock = new();
    private float duration = 0.0f;

    public PlayerStunState(
      IPlayerMoveController moveController, 
      IPlayerStateController stateController, 
      IPlayerAnimatorController animatorController,
      IPlayerInputStateProvider inputStateProvider,
      IPlayerEffectController effectController,
      PlayerStunData stunData,
      SpriteRenderer spriteRenderer,
      PlayerType playerType)
    {
      this.moveController = moveController;
      this.stateController = stateController;
      this.animatorController = animatorController;
      this.inputStateProvider = inputStateProvider;
      this.effectController = effectController;
      this.stunData = stunData;
      this.spriteRenderer = spriteRenderer;
      this.playerType = playerType;
      spriteRenderer.GetPropertyBlock(shearBlock);      
    }

    public void FixedUpdate()
    {
      moveController.ApplyMoveDeceleration();
      duration -= UnityEngine.Time.fixedDeltaTime;

      var shearT =  Mathf.PingPong(duration, stunData.ShearInterval) / stunData.ShearInterval;
      UpdateShear(playerType switch
      {
        PlayerType.Left => Mathf.Lerp(-stunData.ShearRange, stunData.ShearRange, shearT),
        PlayerType.Right => Mathf.Lerp(stunData.ShearRange, -stunData.ShearRange, shearT),
        _ => throw new NotImplementedException(),
      });
      if (duration <= 0.0f)
        ChangeToNextState();
    }

    public void OnEnter()
    {
      duration = stunData.StunDuration;
      animatorController.Play(AnimatorHash.Player.Clip.Stun);
      effectController.PlayEffect(PlayerEffect.Stun);
      UpdateShear(0.0f);
    }

    public void OnExit()
    {
      //effectController.StopEffect(PlayerEffect.Stun);
      UpdateShear(0.0f);
    }

    private void ChangeToNextState()
    {
      var nextState = inputStateProvider.IsAnyInput() ? PlayerState.Move : PlayerState.Idle;
      stateController.ChangeState(nextState);
    }

    private void UpdateShear(float value)
    {
      shearBlock.SetFloat(ShaderHash.Player._Shear, value);
      spriteRenderer.SetPropertyBlock(shearBlock);
    }
  }
}
