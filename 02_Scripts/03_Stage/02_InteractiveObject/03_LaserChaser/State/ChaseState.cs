using LR.Manager.Sound;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.InteractiveObject.LaserChaserState
{
  public class ChaseState : IState
  {
    private readonly Transform transform;
    private Transform targetTransform;
    private readonly GameObject chaseLine;
    private readonly LaserChaser.Model model;
    private readonly ISFXController sfxController;
    private readonly SpriteRenderer pointingSpriteRenderer;
    private readonly MaterialPropertyBlock pointingMatBlock = new();
    private readonly UnityAction<float> onFillAmount;
    private readonly UnityAction<float> onUpdateIntensity;

    private float duration;
    private float angularVelocity;
    private float sfxInterval = 0.0f;
    private float sfxMaxInterval = 0.0f;
    private float pointingUVOffset = 0.0f;

    public ChaseState(
      Transform transform,
      Transform targetTransform,
      GameObject chaseLine,
      LaserChaser.Model model,
      ISFXController sfxController,
      SpriteRenderer pointingSpriteRenderer,
      UnityAction<float> onFillAmount,
      UnityAction<float> onUpdateIntensity)
    {
      this.transform = transform;
      this.targetTransform = targetTransform;
      this.chaseLine = chaseLine;
      this.model = model;
      this.sfxController = sfxController;
      this.pointingSpriteRenderer = pointingSpriteRenderer;
      this.onFillAmount = onFillAmount;
      this.onUpdateIntensity = onUpdateIntensity;
      pointingSpriteRenderer.GetPropertyBlock(pointingMatBlock);

      duration = model.ChaseDuration;
    }

    public void OnEnter()
    {
      chaseLine.SetActive(true);
      duration = model.ChaseDuration;
      onFillAmount?.Invoke(0.0f);
      sfxMaxInterval = model.SFXIntervalMax;
      sfxInterval = sfxMaxInterval;
      pointingMatBlock.SetFloat(ShaderHash.Laser._Offset, model.LineSpeedMin);
      pointingSpriteRenderer.SetPropertyBlock(pointingMatBlock);
    }

    public void OnExit()
    {
      onUpdateIntensity?.Invoke(model.ChaseGlowIntensityMax);
      onFillAmount?.Invoke(1.0f);
      chaseLine.SetActive(false);
    }

    public void OnPuase()
    {
      
    }

    public void OnUpdate(UnityAction onComplete)
    {
      duration -= Time.deltaTime;

      var t = 1.0f - (duration / model.ChaseDuration);
      var lineSpeed = Mathf.Lerp(model.LineSpeedMin, model.LineSpeedMax,  t);
      pointingUVOffset += Time.deltaTime * lineSpeed;
      pointingMatBlock.SetFloat(ShaderHash.Laser._Offset, pointingUVOffset);
      pointingSpriteRenderer.SetPropertyBlock(pointingMatBlock);

      var dir = (targetTransform.position - transform.position).normalized;
      var targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90.0f;

      var currentAngle = transform.eulerAngles.z;

      var delta = Mathf.DeltaAngle(currentAngle, targetAngle);
      var clampedDelta = Mathf.Clamp(delta, -model.MaxRotateValue, model.MaxRotateValue);
      var limitedTarget = currentAngle + clampedDelta;

      float newAngle = Mathf.SmoothDampAngle(
          currentAngle,
          limitedTarget,
          ref angularVelocity,
          model.ChaseSmooth
      );

      transform.rotation = Quaternion.Euler(0, 0, newAngle);
      onFillAmount?.Invoke(t);

      if (duration <= 0)
      {
        onComplete?.Invoke();
      }
      else
      {
        sfxInterval -= Time.deltaTime;
        onUpdateIntensity?.Invoke(Mathf.Lerp(1.0f, model.ShootGlowIntensity, sfxInterval / sfxMaxInterval));
        if(sfxInterval <= 0.0f)
        {
          sfxController.PlayOnce(AudioSourceType.Center, SFX.LaserPointing);
          sfxMaxInterval = Mathf.Lerp(model.SFXIntervalMin, model.SFXIntervalMax, duration / model.ChaseDuration); ;
          sfxInterval = sfxMaxInterval;
        }
      }        
    }

    public void UpdateTarget(Transform target)
      => targetTransform = target;
  }
}
