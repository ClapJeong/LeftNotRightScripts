namespace LR.Stage.Player
{
  public class PlayerExhaustedState : IPlayerState
  {
    private readonly IPlayerAnimatorController animatorController;
    private readonly IPlayerMoveController playerMoveController;
    private readonly IPlayerEffectController playerEffectController;
    private readonly WallHitAnimationTimer wallHitAnimationTimer;

    public PlayerExhaustedState(
      IPlayerAnimatorController animatorController,
      IPlayerMoveController playerMoveController, 
      IPlayerEffectController playerEffectController,
      WallHitAnimationTimer wallHitAnimationTimer)
    {
      this.animatorController = animatorController;
      this.playerMoveController = playerMoveController;
      this.playerEffectController = playerEffectController;
      this.wallHitAnimationTimer = wallHitAnimationTimer;
    }

    public void FixedUpdate()
    {
      playerMoveController.ApplyMoveDeceleration();
    }

    public void OnEnter()
    {
      playerMoveController.SetLinearVelocity(UnityEngine.Vector3.zero);
      animatorController.Play(AnimatorHash.Player.Clip.Exhausted);
      playerEffectController.StopAllEffects();
      playerEffectController.PlayEffect(Enum.PlayerEffect.Exhaust);
      wallHitAnimationTimer.UnsubscribeAll();
    }

    public void OnExit()
    {
      animatorController.Play(AnimatorHash.Player.Clip.Idle);
    }
  }
}
