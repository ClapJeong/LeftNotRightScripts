using UnityEngine;
using LR.Stage.Player.Enum;

namespace LR.Stage.Player
{
  public class PlayerInputState: IPlayerState
  {
    private readonly PlayerStatus playerStatus;
    private readonly IPlayerMoveController moveController;
    private readonly IPlayerStateController stateController;
    private readonly IInputSequenceStopController inputSequenceStopController;
    private readonly IPlayerAnimatorController animatorController;
    private readonly IPlayerEffectController effectController;

    public PlayerInputState(
      PlayerStatus playerStatus,
      IPlayerMoveController moveController, 
      IPlayerStateController stateController, 
      IPlayerAnimatorController animatorController,
      IInputSequenceStopController inputSequenceStopController,
      IPlayerEffectController effectController)
    {
      this.playerStatus = playerStatus;
      this.moveController = moveController;
      this.stateController = stateController;
      this.animatorController = animatorController;
      this.inputSequenceStopController = inputSequenceStopController;
      this.effectController = effectController;
    }

    public void FixedUpdate()
    {      
      if (playerStatus.IsInputting == false)
      {
        stateController.ChangeState(PlayerState.Idle);
      }        
    }

    public void OnEnter()
    {
      animatorController.Play(AnimatorHash.Player.Clip.InputBegin);
      moveController.SetLinearVelocity(Vector3.zero);
      effectController.PlayEffect(PlayerEffect.Inputing);
    }

    public void OnExit()
    {
      inputSequenceStopController.Stop();
      effectController.StopEffect(PlayerEffect.Inputing);
    }
  }
}