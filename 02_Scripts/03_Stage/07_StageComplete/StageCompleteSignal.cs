using Cysharp.Threading.Tasks;
using LR.Manager.Stage;
using LR.Manager.UI;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.Complete
{
  public class StageCompleteSignal : MonoBehaviour
  {
    private enum FirstEnterPlayerType
    {
      None,
      Left,
      Right,
    };
    [System.Serializable]
    public class Model
    {
      public float blinkDuration;
      public float minIntensity;
      public float maxIntensity;
    }
    [SerializeField] public Model model;
    [Space(10)]
    [SerializeField] private SpriteRenderer leftSpriteRenderer;
    [SerializeField] private ParticleSystem leftEffect;
    [SerializeField] private SpriteRenderer rightSpriteRenderer;
    [SerializeField] private ParticleSystem rightEffect;
    [SerializeField] private Animator animator;

    private IStageStateProvider stageStateProvider;
    private MaterialPropertyBlock leftMaterialBlock;
    private MaterialPropertyBlock rightMaterialBlock;

    private FirstEnterPlayerType firstEnterPlayer = FirstEnterPlayerType.None;
    private float duration;
    private bool isLeftEnter = false;
    private bool isRightEnter = false;

    public void Initialize( StageManager stageManager)
    {
      this.stageStateProvider = stageManager;

      stageManager.SubscribeOnEvent(IStageEventSubscriber.StageEventType.LeftClearEnter, OnLeftEnter);
      stageManager.SubscribeOnEvent(IStageEventSubscriber.StageEventType.RightClearEnter, OnRightEnter);
      stageManager.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);
    }

    private void Awake()
    {
      leftMaterialBlock = new();
      leftSpriteRenderer.GetPropertyBlock(leftMaterialBlock);

      rightMaterialBlock = new();
      rightSpriteRenderer.GetPropertyBlock(rightMaterialBlock);
    }

    private void Update()
    {
      if (stageStateProvider != null && !stageStateProvider.IsPlayingState)
        return;

      duration += Time.deltaTime;
      var t = 1.0f - ((duration % model.blinkDuration) / model.blinkDuration);

      UpdateLeftIntensity(t);
      UpdateRightIntensity(t);
    }

    private void UpdateLeftIntensity(float t)
    {
      leftMaterialBlock.SetFloat(ShaderHash.Glow._Intensity, (isLeftEnter ? model.maxIntensity : model.minIntensity) * t);
      leftSpriteRenderer.SetPropertyBlock(leftMaterialBlock);
    }

    private void UpdateRightIntensity(float t)
    {
      rightMaterialBlock.SetFloat(ShaderHash.Glow._Intensity, (isRightEnter ? model.maxIntensity : model.minIntensity) * t);
      rightSpriteRenderer.SetPropertyBlock(rightMaterialBlock);
    }

    private void OnLeftEnter()
    {
      isLeftEnter = true;
      leftEffect.Play();

      if (firstEnterPlayer == FirstEnterPlayerType.None)
        firstEnterPlayer = FirstEnterPlayerType.Left;
      else
      {
        UpdateRightIntensity(1.0f);
      }
    }

    private void OnRightEnter()
    {
      isRightEnter = true;
      rightEffect.Play();

      if (firstEnterPlayer == FirstEnterPlayerType.None)
        firstEnterPlayer = FirstEnterPlayerType.Right;
      else
      {
        UpdateRightIntensity(1.0f);
      }
    }

    private void OnRestart()
    {
      isLeftEnter = false;
      isRightEnter = false;
      leftEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      rightEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      duration = 0.0f;
      animator.Play(AnimatorHash.StageCompleteSignal.Idle);
      firstEnterPlayer = FirstEnterPlayerType.None;
    }

    public async UniTask PlayAsync(UnityAction onComplete, CancellationToken token = default)
    {
      var targetHash = firstEnterPlayer switch
      {
        FirstEnterPlayerType.None => throw new NotImplementedException(),
        FirstEnterPlayerType.Left => AnimatorHash.StageCompleteSignal.LeftActivate,
        FirstEnterPlayerType.Right => AnimatorHash.StageCompleteSignal.RightActivate,
        _ => throw new NotImplementedException(),
      };
      try
      {
        animator.Play(targetHash);

        leftMaterialBlock.SetFloat(ShaderHash.Glow._Intensity, model.maxIntensity);
        leftSpriteRenderer.SetPropertyBlock(leftMaterialBlock);

        rightMaterialBlock.SetFloat(ShaderHash.Glow._Intensity, model.maxIntensity);
        rightSpriteRenderer.SetPropertyBlock(rightMaterialBlock);

        await UniTask.WaitForSeconds(0.5f, false, PlayerLoopTiming.Update, token);

        await UniTask.WaitUntil(() =>
        {
          var state = animator.GetCurrentAnimatorStateInfo(0);
          return state.shortNameHash == targetHash && state.normalizedTime >= 1.0f;
        }, PlayerLoopTiming.Update, token);

        onComplete?.Invoke();
      }
      catch (OperationCanceledException) { }
    }
  }
}