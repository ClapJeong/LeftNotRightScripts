using Cysharp.Threading.Tasks;
using LR.Table.Player;
using System;
using System.Threading;
using UnityEngine;

namespace LR.Stage.Player.ReactionController
{
  public class WallHitsizeController : IDisposable
  {
    private readonly PlayerWallHitSizeData data;
    private readonly Transform transform;

    private readonly CTSContainer cts = new();

    public WallHitsizeController(PlayerWallHitSizeData data, Transform transform)
    {
      this.data = data;
      this.transform = transform;
    }

    public void PlayHorizontal()
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;
      var targetScale = new Vector3(1.0f + data.ScaleValue, 1.0f - data.ScaleValue, 1.0f);
      PlayAsync(targetScale, token).Forget();
    }

    public void PlayVertical()
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;
      var targetScale = new Vector3(1.0f - data.ScaleValue , 1.0f + data.ScaleValue, 1.0f);
      PlayAsync(targetScale, token).Forget();
    }

    private async UniTask PlayAsync(Vector3 targetScale, CancellationToken token)
    {
      try
      {
        var time = 0.0f;
        while(time < data.Duration)
        {
          token.ThrowIfCancellationRequested();

          transform.localScale = Vector3.Lerp(Vector3.one, targetScale, time / data.Duration);

          time += Time.deltaTime;
          await UniTask.Yield();
        }

        while (time > 0.0f)
        {
          token.ThrowIfCancellationRequested();

          transform.localScale = Vector3.Lerp(Vector3.one, targetScale, time / data.Duration);

          time -= Time.deltaTime;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        if (this != null && transform != null)
          transform.localScale = Vector3.one;
      }
    }

    public void StopImmedieately()
    {
      cts.Cancel();
    }

    public void Dispose()
    {
      cts.Dispose();
    }
  }
}
