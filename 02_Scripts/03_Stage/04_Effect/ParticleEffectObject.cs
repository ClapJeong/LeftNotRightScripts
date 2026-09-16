using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.Effect
{
  public class ParticleEffectObject : BaseEffectObject
  {
    [SerializeField] private List<ParticleSystem> particles;
    [SerializeField] private float beforeDelay;
    [SerializeField] private float afterDelay;
    private readonly CTSContainer cts = new();

    public override async UniTask PlayAsync(UnityAction onComplete = null)
    {
      cts.Create();
      var token = cts.token;

      await UniTask.WaitForSeconds(beforeDelay, false, PlayerLoopTiming.Update, token);

      foreach (var particle in particles)
        particle.Play();

      await UniTask.WaitUntil(() =>
      {
        foreach (var particle in particles)
          if (particle != null && particle.IsAlive())
            return false;

        return true;
      }, PlayerLoopTiming.Update, token);

      await UniTask.WaitForSeconds(afterDelay, false, PlayerLoopTiming.Update, token);

      onComplete?.Invoke();
    }

    public override void StopImmediately()
    {
      cts.Cancel();
    }
  }
}
