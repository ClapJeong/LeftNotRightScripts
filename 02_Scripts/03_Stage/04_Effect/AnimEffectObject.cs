using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.Effect
{
  public class AnimEffectObject : BaseEffectObject
  {
    [SerializeField] private List<Animator> animators;
    [SerializeField] private float beforeDelay;
    [SerializeField] private float afterDelay;

    private readonly CTSContainer cts = new();

    public override async UniTask PlayAsync(UnityAction onComplete = null)
    {
      cts.Create();
      var token = cts.token;
      await UniTask.WaitForSeconds(beforeDelay, false, PlayerLoopTiming.Update, token);

      foreach (var animator in animators)
        animator.Play(AnimatorHash.Effect.Play);

      await UniTask.NextFrame(token);

      await UniTask.WaitUntil(() =>
      {
        foreach (var animator in animators)
        {
          if (animator == null)
            continue;

          var state = animator.GetCurrentAnimatorStateInfo(0);

          if (!state.shortNameHash.Equals(AnimatorHash.Effect.Idle) ||
              state.normalizedTime < 1.0f)
          {
            return false;
          }
        }
        return true;
      }, cancellationToken: token);

      await UniTask.WaitForSeconds(afterDelay, false, PlayerLoopTiming.Update, token);
      onComplete?.Invoke();
    }

    public override void StopImmediately()
    {
      cts.Cancel();
    }
  }
}
