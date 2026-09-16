using Cysharp.Threading.Tasks;
using LR.Table.TriggerTile;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace LR.Stage.TriggerTile
{
  public class ACDCGlowPlayer : IDisposable
  {
    private readonly SpriteRenderer spriteRenderer;
    private readonly SignalTriggerData data;

    private readonly MaterialPropertyBlock glowMatBlock = new();
    private readonly CTSContainer glowCTS = new();

    public ACDCGlowPlayer(SpriteRenderer spriteRenderer, SignalTriggerData data)
    {
      this.spriteRenderer = spriteRenderer;
      this.data = data;
      spriteRenderer.GetPropertyBlock(glowMatBlock);

      UpdateGlowIntensity(data.IdleIntensity);
    }

    public void Play()
    {
      glowCTS.Create();
      var token = glowCTS.token;
      GlowAsync(token).Forget();
    }

    public void Stop()
    {
      glowCTS.Cancel();
    }

    private async UniTask GlowAsync(CancellationToken token)
    {
      try
      {
        var time = data.GlowInterval;
        UpdateGlowIntensity(data.ActivateIntensity);
        while (true)
        {
          token.ThrowIfCancellationRequested();
          time -= Time.deltaTime;
          UpdateGlowIntensity(Mathf.Lerp(data.ActivateIntensity, data.IdleIntensity, time / data.GlowInterval));

          if (time <= 0.0f)
            time = data.GlowInterval;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        if (spriteRenderer != null)
          UpdateGlowIntensity(data.IdleIntensity);
      }
    }

    private void UpdateGlowIntensity(float intensity)
    {
      glowMatBlock.SetFloat(ShaderHash.Glow._Intensity, intensity);
      spriteRenderer.SetPropertyBlock(glowMatBlock);
    }

    public void Dispose()
    {
      glowCTS.Dispose();
    }
  }
}
