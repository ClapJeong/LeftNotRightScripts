using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Manager.Stage
{
  public class WallHitFallObject : MonoBehaviour
  {
    [SerializeField] private ParticleSystem particle;

    private CTSContainer fallingCTS = new();

    public async UniTask PlayAsync(
      Vector3 beginPosition, 
      float emmisionMin, 
      float emmisionMax,
      float sizeMin,
      float sizeMax,
      UnityAction onComplete)
    {
      var emmisionModule = particle.emission;
      var bust = emmisionModule.GetBurst(0);
      bust.minCount = (short)emmisionMin;
      bust.maxCount = (short)emmisionMax;
      emmisionModule.SetBurst(0, bust);

      var mainModule = particle.main;
      var startSize = mainModule.startSize;
      startSize.constantMin = sizeMin;
      startSize.constantMax = sizeMax;
      mainModule.startSize = startSize;

      fallingCTS.Cancel();
      fallingCTS.Create();
      var token = fallingCTS.token;
      transform.position = beginPosition;

      try
      {
        particle.Play();

        await UniTask.WaitUntil(() =>
        {
          if (particle != null && particle.IsAlive())
            return false;

          return true;
        }, PlayerLoopTiming.Update, token);
        
        onComplete?.Invoke();
      }
      catch (OperationCanceledException) { }
    }
    private void OnDestroy()
    {
      fallingCTS.Dispose();
    }
  }
}
