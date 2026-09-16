using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.Player.Enum;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.Complete
{
  public class StageCompleteDoctor : MonoBehaviour
  {
    public enum DoctorBeginState
    {
      Idle,
      Shaking,
      None,
      Final,
    }

    public DoctorBeginState State => doctorBeginState;
    [SerializeField] private DoctorBeginState doctorBeginState;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer shadowSpriteRenderer;
    [Space(10)]
    [SerializeField] private Transform platformTransform;
    [SerializeField] private Transform doorTransform;
    [SerializeField] private float runChargeLength = 0.0f;
    [SerializeField] private ParticleSystem walkParticle;
    [SerializeField] private ParticleSystem runParticle;

    private readonly CTSContainer sfxCTS = new();
    private Vector2 initializedPosition;
    private StageCompleteDataSO data;
    private ISFXController sfxController;
    private AudioLoopHandle audioLoopHandle;

    public void Initialize(
      IGameDataProvider gameDataProvider,
      StageManager stageManager,
      StageCompleteDataSO data,
      ISFXController sfxController,
      bool isSpeedRun)
    {
      if(isSpeedRun)
      {
        gameObject.SetActive(false);
        return;
      }

      this.data = data;
      this.sfxController = sfxController;
      initializedPosition = transform.position;

      var stage = gameDataProvider.GetSelectedStage();
      spriteRenderer.flipX = transform.position.x < platformTransform.position.x;

      stageManager.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Exhausted, OnExhaust);
      stageManager.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, ResetBeginState);

      if (doctorBeginState == DoctorBeginState.Idle)
        stageManager.GetPlayer(Player.Enum.PlayerType.Left).GetEnergySubscriber().SubscribeOnHit(OnPlayerHit);

      ResetBeginState();
    }

    private void OnPlayerHit(PlayerType _, DamageType __)
    {
      animator.Play(AnimatorHash.StageCompleteDoctor.PlayerHit, 0, 0.0f);
    }

    private void OnExhaust()
    {
      if (doctorBeginState == DoctorBeginState.Idle)
        animator.Play(AnimatorHash.StageCompleteDoctor.Fail);
    }

    private void ResetBeginState()
    {
      transform.position = initializedPosition;
      animator.Play(doctorBeginState switch
      {
        DoctorBeginState.Idle => AnimatorHash.StageCompleteDoctor.Idle,
        DoctorBeginState.Shaking => AnimatorHash.StageCompleteDoctor.Shaking,
        DoctorBeginState.None => AnimatorHash.StageCompleteDoctor.None,
        DoctorBeginState.Final => AnimatorHash.StageCompleteDoctor.None,
        _ => throw new NotImplementedException(),
      });
      runParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      spriteRenderer.color = Color.white;
      shadowSpriteRenderer.enabled = doctorBeginState == DoctorBeginState.Idle || doctorBeginState == DoctorBeginState.Shaking;
    }

    public async UniTask PlayAsync(UnityAction onComplete, CancellationToken token = default)
    {      
      try
      {
        switch (doctorBeginState)
        {
          case DoctorBeginState.Idle:
            {
              audioLoopHandle = sfxController.CreateLoopSource(CharacterPositionType.Center, SFX.DoctorRun);
              animator.Play(AnimatorHash.StageCompleteDoctor.Running);

              UpdateEffectDirect();
              walkParticle.Play();
              var platformPosition = platformTransform.position;
              var platformMoveDuration = Vector3.Distance(transform.position, platformPosition) / data.DoctorWalkSpeed;
              var doorPosition = doorTransform.position;
              var doorMoveDuration = Vector3.Distance(platformPosition, doorPosition) / data.DoctorWalkSpeed;
              await DOTween
                .Sequence()
                .Append(transform.DOMove(platformPosition, platformMoveDuration).SetEase(Ease.Linear))
                .Append(transform.DOMove(doorPosition, doorMoveDuration).SetEase(Ease.Linear))
                .AppendCallback(() =>
                {
                  StopDoctorRunSFXAsync(data.DoctorWalkHideDuration).Forget();
                })
                .Append(spriteRenderer.DOFade(0.0f, data.DoctorWalkHideDuration))
                .Join(shadowSpriteRenderer.DOFade(0.0f, data.DoctorWalkHideDuration))
                .ToUniTask(TweenCancelBehaviour.Complete, token);
              walkParticle.Stop();
              await UniTask.WaitForSeconds(data.DelayAfterDoctorWork, false, PlayerLoopTiming.Update, token);
            }
            break;

          case DoctorBeginState.Shaking:
            {
              audioLoopHandle = sfxController.CreateLoopSource(CharacterPositionType.Center, SFX.DoctorRun);
              animator.Play(AnimatorHash.StageCompleteDoctor.Running);

              var normalized = (platformTransform.position - transform.position).normalized;
              var chargePosition = transform.TransformPoint(-1.0f * runChargeLength * normalized);
              var platformPosition = platformTransform.position;
              var platformMoveDuration = Vector3.Distance(transform.position, platformPosition) / data.DoctorRunSpeed;
              var doorPosition = doorTransform.position;
              var doorMoveDuration = Vector3.Distance(platformPosition, doorPosition) / data.DoctorRunSpeed;
              await DOTween
                .Sequence()
                .Append(transform.DOMove(chargePosition, data.DoctorRunChargingDuration))
                .AppendCallback(() =>
                {
                  UpdateEffectDirect();
                  runParticle.Play();
                })
                .Append(transform.DOMove(platformPosition, platformMoveDuration, false).SetEase(Ease.Linear))
                .Append(transform.DOMove(doorPosition, doorMoveDuration, false).SetEase(Ease.Linear))
                .AppendCallback(() =>
                {
                  StopDoctorRunSFXAsync(data.DoctorRunHideDuration).Forget();
                })
                .Append(spriteRenderer.DOFade(0.0f, data.DoctorRunHideDuration))
                .Join(shadowSpriteRenderer.DOFade(0.0f, data.DoctorWalkHideDuration))
                .ToUniTask(TweenCancelBehaviour.Complete, token);
              runParticle.Stop();              
              await UniTask.WaitForSeconds(data.DelayAfterDoctorWork, false, PlayerLoopTiming.Update, token);
            }
            break;

          default:
            await UniTask.CompletedTask;
            break;
        }
        onComplete?.Invoke();
      }
      catch (OperationCanceledException) { }
    }

    public float GetTotalMoveDuration()
    {
      var platformPosition = platformTransform.position;
      var speed = State == DoctorBeginState.Idle ? data.DoctorWalkSpeed : data.DoctorRunSpeed;
      var platformDuration = Vector3.Distance(transform.position, platformPosition) / speed;

      var doorPosition = doorTransform.position;
      var doorMoveDuration = Vector3.Distance(platformPosition, doorPosition) / speed;

      return platformDuration + doorMoveDuration;
    }

    private void UpdateEffectDirect()
    {
      var normalized = (platformTransform.position - transform.position).normalized;
      var atan2 = Mathf.Atan2(normalized.y, normalized.x);
      walkParticle.transform.eulerAngles = new Vector3(0.0f, 0.0f, atan2 * Mathf.Rad2Deg);
    }

    private async UniTask StopDoctorRunSFXAsync(float duration)
    {
      sfxCTS.Cancel();
      sfxCTS.Create();
      var token = sfxCTS.token;
      var time = 0.0f;
      try
      {
        while (time < duration)
        {
          token.ThrowIfCancellationRequested();
          if(audioLoopHandle != null)
          audioLoopHandle.Volume = 1.0f - (time / duration);

          time += Time.deltaTime;
          await UniTask.Yield();
        }

        audioLoopHandle?.Dispose();
        audioLoopHandle = null;
      }
      catch (OperationCanceledException) { }
    }

    private void OnDrawGizmos()
    {
      if (doorTransform == null || platformTransform == null)
        return;

      Gizmos.color = Color.yellow;
      var normalized = (platformTransform.position - transform.position).normalized;
      Gizmos.DrawLine(transform.position, transform.TransformPoint(-1.0f * runChargeLength * normalized));
      Gizmos.color = Color.red;
      Gizmos.DrawLine(transform.position, platformTransform.position);
    }

    private void OnDestroy()
    {
      sfxCTS.Dispose();
      audioLoopHandle?.Dispose();
    }
  }
}