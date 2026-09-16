using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.Player;
using LR.Table.TriggerTile;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.InteractiveObject
{
  public class ShooterBullet : MonoBehaviour
  {
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private BounceData bounceData;
    [SerializeField] private ParticleSystem flyingParticle;
    [SerializeField] private ParticleSystem walllDestroyEffect;
    [SerializeField] private ParticleSystem playerHitEffect;
    [SerializeField] private SpriteRenderer bulletSprite;
    [SerializeField] private TrailRenderer trailRenderer;

    private IStageStateProvider stageStateProvider;
    private IPlayerGetter playerGetter;
    private UnityAction onDestroyed;
    private int shooterHash;
    private ISFXController sfxController;

    private int shootFrame;
    private bool isEnable = false;
    private LayerMask obstacleLayer;

    public void Initialize(
      IStageStateProvider stageStateProvider, 
      IPlayerGetter playerGetter,
      LayerMask obstacleLayer,
      UnityAction onDestroyed,
      int shooterHash,
      ISFXController sfxController)
    {
      this.stageStateProvider = stageStateProvider;
      this.playerGetter = playerGetter;
      this.obstacleLayer = obstacleLayer;
      this.onDestroyed = onDestroyed;
      this.shooterHash = shooterHash;
      this.sfxController = sfxController;
      trailRenderer.emitting = false;
    }

    public void Shoot(Vector3 worldPosition, Quaternion quaternion)
    {
      transform.SetPositionAndRotation(worldPosition, quaternion);

      isEnable = true;
      bulletSprite.enabled = true;
      flyingParticle.Play();
      trailRenderer.Clear();
      trailRenderer.emitting = true;
      shootFrame = Time.frameCount;
    }

    public void DeactiveImmedieately()
    {
      isEnable = false;
      bulletSprite.enabled = false;
      walllDestroyEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      playerHitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      flyingParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      trailRenderer.emitting = false;
      onDestroyed?.Invoke();
    }

    private void Update()
    {
      if (!isEnable || !stageStateProvider.IsPlayingState)
        return;

      var moveValue = speed * Time.deltaTime * Vector3.up;
      transform.Translate(moveValue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      if (!isEnable || 
        collision.gameObject.GetHashCode() == shooterHash)
        return;

      var isTooEarly = (Time.frameCount - shootFrame) < 5;

      if (collision.CompareTag(Tag.Player))
      {
        var playerType = collision.GetComponent<IPlayerView>().GetPlayerType();
        var reactionController = playerGetter
          .GetPlayer(playerType)
          .GetReactionController();

        reactionController.DamageEnergy(damage);
        reactionController.Bounce(bounceData, transform.up);
        sfxController.PlayOnce(playerType.ParseToAudioSourceType(), playerType switch
        {
          Player.Enum.PlayerType.Left => SFX.LBulletHit,
          Player.Enum.PlayerType.Right => SFX.RBulletHit,
          _ => throw new NotImplementedException(),
        });
        DestroyBullet(true);
      }
      else
      {
        var isWall = (obstacleLayer & (1 << collision.gameObject.layer)) != 0;
        if (isWall && !collision.isTrigger && !isTooEarly)
        {
          DestroyBullet(false);
        }        
      }
    }

    private void DestroyBullet(bool isPlayerHit)
    {
      if (isPlayerHit)
        playerHitEffect.Play();
      else
        walllDestroyEffect.Play();

      bulletSprite.enabled = false;
      trailRenderer.emitting = false;
      flyingParticle.Stop();
      onDestroyed?.Invoke();
      isEnable = false;
    }
  }
}
