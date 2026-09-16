using Cysharp.Threading.Tasks;
using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Manager.Stage.Marking;
using LR.Stage.Player.Enum;
using LR.Stage.Player.ReactionController;
using LR.Table.Player;
using LR.Table.TriggerTile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.Player
{
  public class PlayerReactionController : IPlayerReactionController
  {
    private readonly IMarkPlacer markPlacer;
    private readonly PlayerStatus playerStatus;
    private readonly IEffectService effectService;
    private readonly ICameraEffectService cameraEffectService;
    private readonly PlayerType playerType;
    private readonly PlayerCollisionData collisionData;
    private readonly IPlayerMoveController moveController;
    private readonly IPlayerStateController stateController;
    private readonly IPlayerStateProvider stateProvider;
    private readonly IPlayerEnergyProvider energyProvider;
    private readonly IPlayerEnergyController energyController;
    private readonly Rigidbody2D viewRigidbody2D;
    private readonly ISFXController sfxController;
    private readonly Animator animator;
    private readonly PlayerMovementData playerMovementData;
    private readonly IPlayerEffectController effectController;

    private readonly Dictionary<int, Vector3> conveyors = new();
    private readonly List<int> electrics = new();
    private readonly List<int> wallStucks = new();
    private readonly FlashController flashController;
    private readonly WallHitsizeController wallHitsizeController;
    private readonly UnityEvent onWallContact = new();

    private bool IsReactionableState => stateProvider.GetCurrentState() == Enum.PlayerState.Clear && stateProvider.GetCurrentState() != PlayerState.Exhausted;
    
    private bool ignorReaction = false;

    private float moveDuration = 0.0f;

    public PlayerReactionController(
      IMarkPlacer markPlacer,
      PlayerStatus playerStatus,
      IEffectService effectService,
      ICameraEffectService cameraEffectService,
      PlayerType playerType,
      PlayerCollisionData collisionData,
      IPlayerMoveController moveController, 
      IPlayerStateController stateController, 
      IPlayerStateProvider stateProvider,
      IPlayerEnergyProvider energyProvider,
      IPlayerEnergyController energyController,
      Rigidbody2D viewRigidbody2D,
      Transform transform,
      ISFXController sfxController,
      SpriteRenderer spriteRenderer,
      PlayerBlinkData blinkData,
      Animator animator,
      PlayerMovementData playerMovementData,
      IPlayerEffectController effectController,
      PlayerWallHitSizeData playerWallHitSizeData)
    {
      this.markPlacer = markPlacer;
      this.playerStatus = playerStatus;
      this.effectService = effectService;
      this.cameraEffectService = cameraEffectService;
      this.playerType = playerType;
      this.collisionData = collisionData;
      this.moveController = moveController;
      this.stateController = stateController;
      this.stateProvider = stateProvider;
      this.energyProvider = energyProvider;
      this.energyController = energyController;
      this.viewRigidbody2D = viewRigidbody2D;
      this.sfxController = sfxController;
      this.animator = animator;
      this.playerMovementData = playerMovementData;
      this.effectController = effectController;

      flashController = new(spriteRenderer, blinkData);
      wallHitsizeController = new(playerWallHitSizeData, transform);
    }

    public void Bounce(BounceData data, Vector3 direction)
    {
      if (ignorReaction)
        return;

      if (IsReactionableState)
        return;

      if (stateProvider.GetCurrentState() == PlayerState.Inputting)
        return;

      moveController.SetLinearVelocity(direction * data.Force);
    }

    public void UpdateMoveDuration(float moveDuration)
      =>this.moveDuration = moveDuration;

    public void SetInputting(bool isInputting)
    {
      if (isInputting && !playerStatus.IsInputting)
        stateController.ChangeState(PlayerState.Inputting);

      playerStatus.IsInputting = isInputting;
    }

    public void Stun()
    {
      if (ignorReaction)
        return;

      if (IsReactionableState)
        return;

      stateController.ChangeState(PlayerState.Stun);
    }

    public void Clear()
    {
      wallHitsizeController.StopImmedieately();
      ClearConveyor();
      ClearElectric();
      stateController.ChangeState(PlayerState.Clear);
    }

    public void DamageEnergy(float value, bool ignoreInvincible = false)
    {
      if (ignorReaction)
        return;

      energyController.Damage(playerType, value, DamageType.Strong, ignoreInvincible);

      cameraEffectService.GenerateImpulse(playerType.ParseToDamagedImpulseType());
    }

    public void OnWallBump(Collision2D collision2D)
    {
      if (energyProvider.IsDead)
        return;

      var isWall = collision2D.gameObject.CompareTag(Tag.Wall);
      var hashCode = collision2D.GetHashCode();
      if (isWall && !wallStucks.Contains(hashCode))
      {
        wallStucks.Add(hashCode);
        playerStatus.IsWallStuck = true;
      }

      if (ignorReaction)
        return;
      
      var isStun = stateProvider.GetCurrentState() == PlayerState.Stun;
      if (isWall && !isStun)
      {
        var velocityNormalized = moveController.GetCurrentInputVelocityNormalized();
        var isVelocityLess = velocityNormalized < collisionData.WallBumpVelocityNormalized;
        var isMoveDurationLess = playerStatus.IsElectric.Value ? moveDuration < collisionData.WallBumpMinDuration / playerMovementData.ElectricModifier
                                                               : moveDuration < collisionData.WallBumpMinDuration;
        //if (isVelocityLess)
        //  Debug.Log($"{Time.frameCount} isVelocityLess {velocityNormalized} < {collisionData.WallBumpVelocityNormalized}");
        //if (isMoveDurationLess)
        //{
        //  if (playerStatus.IsElectric.Value)
        //    Debug.Log($"{Time.frameCount} isMoveDurationLess {moveDuration} < {collisionData.WallBumpMinDuration / playerMovementData.ElectricModifier}");
        //  else
        //    Debug.Log($"{Time.frameCount} isMoveDurationLess {moveDuration} < {collisionData.WallBumpMinDuration}");
        //}          

        if (isVelocityLess || isMoveDurationLess) { }
        else
        {
          //Debug.Log($"{Time.frameCount} hit {velocityNormalized} {moveDuration}");
          viewRigidbody2D.sharedMaterial = collisionData.WallMaterial;

          var t = moveDuration / collisionData.WallBumpMaxDuration;

          DecreaseWallHitDamage(t);
          cameraEffectService.GenerateImpulse(playerType.ParseToWallHitImpulseType(), t);
          flashController.WallHitBlinkAsync().Forget();
          CreateWallHitEffect(collision2D);
          PlayWallHitSize(collision2D);

          moveController.DecreaseVelocity(collisionData.WallBumpVelocityDecreaseValue);

          if (!energyProvider.IsDead)
          {
            var volume = Mathf.Lerp(0.35f, 1.0f, t);
            sfxController.PlayOnce(playerType.ParseToAudioSourceType(), playerType.ParseToWallHitSFX(), onlySingle: true, volume);
          }
        }

        onWallContact?.Invoke();
      }
      else
      {
        viewRigidbody2D.sharedMaterial = collisionData.DefaultMaterial;
      }     
    }

    private void DecreaseWallHitDamage(float moveT)
    {
      var damage = Mathf.Lerp(
          collisionData.WallBumpMinDamage,
          collisionData.WallBumpMaxDamge,
          moveT);
      energyController.Damage(playerType, damage, DamageType.WallBump, true);
    }

    private void CreateWallHitEffect(Collision2D collision2D)
    {
      var contact = collision2D.GetContact(0);
      var directionToPlayer = contact.normal;
      var rotation = Quaternion.FromToRotation(Vector2.up, directionToPlayer);
      var effectPosition = moveController.GetCurrentPosition() + Vector2.up * 0.5f - directionToPlayer * 0.5f;
      effectService.Create(playerType.ParseToWallHitEffectType(), effectPosition, rotation);

      CreateWallHitMark(directionToPlayer);
    }

    private void CreateWallHitMark(Vector2 directionToPlayer)
    {
      var markPosition = moveController.GetCurrentPosition() + Vector2.up * 0.25f - directionToPlayer * 0.5f;
      markPlacer.MarkWallHitPaint(playerType, markPosition);
    }

    private void PlayWallHitSize(Collision2D collision2D)
    {
      var contactPosition = collision2D.GetContact(0).point;
      var playerPosition = moveController.GetCurrentPosition();
      var verticalLength = Mathf.Abs(contactPosition.x - playerPosition.x);
      var horizontalLength = Mathf.Abs(contactPosition.y - playerPosition.y);
      if (horizontalLength > verticalLength)
        wallHitsizeController.PlayHorizontal();
      else
        wallHitsizeController.PlayVertical();
    }

    public void OnWallExit(Collision2D collision2D)
    {
      var isWall = collision2D.gameObject.CompareTag(Tag.Wall);
      var hashCode = collision2D.GetHashCode();
      if (isWall && wallStucks.Contains(hashCode))
      {
        wallStucks.Remove(hashCode);
        if(wallStucks.Count == 0)
          playerStatus.IsWallStuck = false;
      }
    }

    public void Teleport(Vector3 targetPosition)
    {
      if (ignorReaction)
        return;

      if (playerStatus.IsTeleported)
      {
        playerStatus.IsTeleported = false;
        return;
      }

      sfxController.PlayOnce(playerType.ParseToAudioSourceType(), SFX.Portal);
      playerStatus.IsTeleported = true;
      moveController.MovePosition(targetPosition);

      flashController.TeleportBlinkAsync().Forget();
    }

    public void EnterConveyor(int id, Vector3 direction)
    {      
      if (ignorReaction)
        return;

      if (conveyors.ContainsKey(id))
        return;
      
      conveyors[id] = direction;

      var result = UpdateConveyorDirection();
      moveController.SetModify(result);
    }

    public void ExitConveyor(int id)
    {
      if (conveyors.TryGetValue(id, out var direction))
      {
        conveyors.Remove(id);

        var result = UpdateConveyorDirection();
        moveController.SetModify(result);
      }        
    }

    public void ClearConveyor()
    {
      conveyors.Clear();
      var result = UpdateConveyorDirection();
      moveController.SetModify(result);
    }

    public void IgnoreReactionOnce()
    {
      ignorReaction = true;
      RevertIgnoreReaction().Forget();
    }

    public void ClearElectric()
    {
      electrics.Clear();

      playerStatus.IsElectric.Value = false;
      animator.speed = 1.0f;
      effectController.StopEffect(PlayerEffect.Electric);
    }

    public void EnterElectric(int id)
    {
      if (!electrics.Contains(id))
      {
        electrics.Add(id);

        playerStatus.IsElectric.Value = true;
        animator.speed = playerMovementData.ElectricModifier;
      }        
      if(electrics.Count == 1)
      {
        sfxController.PlayOnce(
          playerType.ParseToAudioSourceType(),
          playerType.ParseToElectricSFX());
        effectController.PlayEffect(PlayerEffect.Electric);
      }        
    }

    public void ExitElectric(int id)
    {
      if (electrics.Contains(id))
        electrics.Remove(id);

      if(electrics.Count == 0)
      {
        playerStatus.IsElectric.Value = false;
        animator.speed = 1.0f;
        effectController.StopEffect(PlayerEffect.Electric);
      }
    }

    private async UniTask RevertIgnoreReaction()
    {
      await UniTask.Delay(1);
      ignorReaction = false;
    }

    private Vector3 UpdateConveyorDirection()
    {
      var distinct = conveyors.Values.Distinct().ToList();

      var result = distinct.Count == 0
          ? Vector3.zero
          : distinct.Aggregate(Vector3.zero, (sum, v) => sum + v) / distinct.Count;
      return result;
    }

    public void SubscribeOnWallContact(UnityAction unityAction)
      => onWallContact.AddListener(unityAction);

    public void UnsubscribeOnWallContact(UnityAction unityAction)
      => onWallContact.RemoveListener(unityAction);

    public void Dispose()
    {
      wallHitsizeController.Dispose();
      flashController.Dispose();
    }    
  }
}