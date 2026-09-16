using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Manager.Stage
{
  public class TimeScaleController : IDisposable
  {
    private readonly CTSContainer cts = new();

    public async UniTask PlayTimeSlowAsync(
      float timeScale,
      float beginDelay,
      float inDuration,
      UnityAction onDelayComplete = null,
      UnityAction onScaleComplete = null)
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;

      try
      {
        var beginScale = Time.timeScale;
        await UniTask.WaitForSeconds(beginDelay, true, PlayerLoopTiming.Update, token);
        onDelayComplete?.Invoke();
        await TimerAsync(inDuration,
          progress =>
          {
            Time.timeScale = Mathf.Lerp(beginScale, timeScale, progress);
          }, token);
        onScaleComplete?.Invoke();
      }
      catch (OperationCanceledException)
      {
        Time.timeScale = 1.0f;
      }
    }

    private async UniTask TimerAsync(float duration, UnityAction<float> onProgress, CancellationToken token)
    {
      var time = 0.0f;
      try
      {
        while(time < duration)
        {
          token.ThrowIfCancellationRequested();
          onProgress?.Invoke(time / duration);

          time += Time.unscaledDeltaTime;
          await UniTask.Yield();
        }
        onProgress?.Invoke(1.0f);
      }
      catch (OperationCanceledException e)
      {
        throw e;
      }
    }

    public void RevertImmediately()
    {
      cts.Cancel();
    }

    public void Dispose()
    {
      cts.Dispose();
    }
  }
}
