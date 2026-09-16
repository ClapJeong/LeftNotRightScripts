using LR.Manager.Stage;
using LR.Stage.Player.Enum;
using UnityEngine;

namespace LR.Stage.Player
{

  public class PlayerClearState : IPlayerState
  {
    private readonly IPlayerMoveController moveController;
    private readonly IPlayerAnimatorController animatorController;
    private readonly IStageRecorderService stageRecorderService;
    private readonly PlayerType playerType;

    public PlayerClearState(
      IPlayerMoveController moveController, 
      IPlayerAnimatorController animatorController,
      IStageRecorderService stageRecorderService,
      PlayerType playerType)
    {
      this.moveController = moveController;
      this.animatorController = animatorController;
      this.stageRecorderService = stageRecorderService;
      this.playerType = playerType;
    }

    public void FixedUpdate()
    {

    }

    public void OnEnter()
    {
      moveController.SetLinearVelocity(Vector3.zero);

      var isAnyDamaged = stageRecorderService.IsAnyDamaged(playerType);
      var animatorHash = isAnyDamaged ? AnimatorHash.Player.Clip.Clear
                                      : AnimatorHash.Player.Clip.Perfect;
      animatorController.Play(animatorHash);
    }

    public void OnExit()
    {

    }
  }
}