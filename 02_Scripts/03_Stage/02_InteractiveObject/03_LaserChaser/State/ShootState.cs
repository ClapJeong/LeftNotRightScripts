using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.Player;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.InteractiveObject.LaserChaserState
{
  public class ShootState : IState
  {
    private readonly Transform transform;
    private readonly Transform laserTransform;
    private readonly IPlayerGetter playerGetter;
    private readonly ICameraEffectService cameraEffectService;
    private readonly LaserChaser.Model model;
    private readonly ISFXController sfxController;
    private readonly UnityAction<float> onUpdateIntensity;

    private float duration;

    public ShootState(
      Transform transform, 
      Transform laserTransform,
      IPlayerGetter playerGetter,
      ICameraEffectService cameraEffectService,
      LaserChaser.Model model,
      ISFXController sfxController,
      UnityAction<float> onUpdateIntensity)
    {
      this.transform = transform;
      this.laserTransform = laserTransform;
      this.playerGetter = playerGetter;
      this.cameraEffectService = cameraEffectService;
      this.model = model;
      this.sfxController = sfxController;
      this.onUpdateIntensity = onUpdateIntensity;

      duration = model.ShootDuration;
    }

    public void OnEnter()
    {
      cameraEffectService.LaserZoom();
      ShootLaser();
      onUpdateIntensity?.Invoke(model.ShootGlowIntensity);
      sfxController.PlayOnce(AudioSourceType.Center, SFX.LaserShoot, true);
      laserTransform.localScale = new Vector3(model.ShootWidth, 1.0f, 1.0f);
      laserTransform.gameObject.SetActive(true);
      duration = model.ShootDuration;
    }

    public void OnExit()
    {
      onUpdateIntensity?.Invoke(model.ChaseGlowIntensityMin);
      laserTransform.gameObject.SetActive(false);
    }

    public void OnUpdate(UnityAction onComplete)
    {
      duration = Mathf.Max(0.0f, duration -= Time.deltaTime);

      laserTransform.localScale = new Vector3(model.ShootWidth * (duration / model.ShootDuration), 1.0f, 1.0f);

      if (duration <= 0.0f)
        onComplete?.Invoke();
    }

    public void OnPuase()
    {

    }

    private void ShootLaser()
    {
      var origin = transform.position;
      var size = new Vector2(model.ShootWidth, 100.0f);
      var angle = transform.eulerAngles.z;
      var direction = transform.up;
      var layerMask = 1 << LayerMask.NameToLayer("Player");

      var hits = Physics2D.BoxCastAll(
          origin,
          size,
          angle,
          direction,
          distance: 0.0f,
          layerMask);
      if (hits.Length > 0)
      {
        foreach (var hit in hits)
        {
          if (hit.transform.TryGetComponent<IPlayerView>(out var playerView))
            playerGetter
              .GetPlayer(playerView.GetPlayerType())
              .GetReactionController()
              .DamageEnergy(model.ShootDamage);
        }
      }
    }

    public void UpdateTarget(Transform target)
    {
      
    }
  }
}
