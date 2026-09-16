using Cysharp.Threading.Tasks;
using LR.Table.StageGimmick;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Manager.Stage.Gimmick.SwapGimmick
{
  public class TimeScaler : IDisposable
  {
    private readonly SwapData swapData;
    private readonly IStageStateProvider stageStateProvider;

    private readonly CTSContainer cts = new();
    private float prevTimeScale = 1.0f;

    public TimeScaler(SwapData swapData, IStageStateProvider stageStateProvider)
    {
      this.swapData = swapData;
      this.stageStateProvider = stageStateProvider;
    }

    public void PlaySlow()
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;
      SlowAsync(token).Forget();
    }

    private async UniTask SlowAsync(CancellationToken token)
    {
      try
      {
        var slowDuration = swapData.TimeSlowDuration;
        var slowDelay = swapData.TimeSlowDelay;
        await TimerAsync(
          slowDuration,
          t => { Time.timeScale = Mathf.Lerp(1.0f, swapData.TimeScaleValue, t); },
          token);

        Time.timeScale = swapData.TimeScaleValue;
        await TimerAsync(slowDelay, null, token);

        await TimerAsync(
          slowDuration,
          t => { Time.timeScale = Mathf.Lerp(swapData.TimeScaleValue, 1.0f, t); },
          token);
      }
      catch (OperationCanceledException) { }
      finally
      {
        Time.timeScale = 1.0f;
      }
    }

    private async UniTask TimerAsync(float duration, UnityAction<float> onProgress, CancellationToken token)
    {
      var time = 0.0f;
      while (time < duration)
      {
        token.ThrowIfCancellationRequested();
        if (!stageStateProvider.IsPlayingState)
        {
          await UniTask.Yield();
          continue;
        }
        time += Time.unscaledDeltaTime;

        onProgress?.Invoke(time / duration);
        await UniTask.Yield();
      }
      onProgress?.Invoke(1.0f);
    }

    public void StopImmedieatley()
    {
      cts.Cancel();
      Time.timeScale = 1.0f;
    }

    public void OnPause()
    {
      prevTimeScale = Time.timeScale;
      Time.timeScale = 1.0f;
    }

    public void OnResume()
    {
      Time.timeScale = prevTimeScale;
    }

    public void Dispose()
    {
      Time.timeScale = 1.0f;
      cts.Dispose();
    }
  }
}
