using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LR.UI.GameScene.Player.PlayerPortrait
{
  public class ShaderController : IDisposable
  {
    private readonly Image image;
    private readonly UISO uiSO;

    private readonly Dictionary<int, float> initializedValues = new();
    private readonly CTSContainer alphaCTS = new();
    private readonly CTSContainer changeCTS = new();

    public ShaderController(Image image, UISO uiSO)
    {
      this.image = image;
      this.uiSO = uiSO;

      initializedValues[ShaderHash.PlayerPortrait._ScanStrength] = image.material.GetFloat(ShaderHash.PlayerPortrait._ScanStrength);
      initializedValues[ShaderHash.PlayerPortrait._RGBSplit] = image.material.GetFloat(ShaderHash.PlayerPortrait._RGBSplit);
      initializedValues[ShaderHash.PlayerPortrait._Flicker] = image.material.GetFloat(ShaderHash.PlayerPortrait._Flicker);
      initializedValues[ShaderHash.PlayerPortrait._GlitchStrength] = image.material.GetFloat(ShaderHash.PlayerPortrait._GlitchStrength);
      initializedValues[ShaderHash.PlayerPortrait._GlitchSpeed] = image.material.GetFloat(ShaderHash.PlayerPortrait._GlitchSpeed);
      initializedValues[ShaderHash.PlayerPortrait._EdgeNoiseStrength] = image.material.GetFloat(ShaderHash.PlayerPortrait._EdgeNoiseStrength);
      initializedValues[ShaderHash.PlayerPortrait._EdgeNoiseWidth] = image.material.GetFloat(ShaderHash.PlayerPortrait._EdgeNoiseWidth);

      UpdateAlphaAsync().Forget();
    }

    public void OnIdle()
    {
      changeCTS.Cancel();
      changeCTS.Create();
      var token = changeCTS.token;

      ChangeValueAsync(ShaderHash.PlayerPortrait._ScanStrength, uiSO.Player.Shader.IdleScanStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._RGBSplit, uiSO.Player.Shader.IdleRGB, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._GlitchStrength, uiSO.Player.Shader.IdleGlitchStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._EdgeNoiseStrength, uiSO.Player.Shader.IdleEdgeNoiseStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._EdgeNoiseWidth, uiSO.Player.Shader.IdleEdgeNoiseWidth, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._Flicker, uiSO.Player.Shader.IdleFlicker, token).Forget();
    }

    public void OnWallHit()
    {
      changeCTS.Cancel();
      changeCTS.Create();
      var token = changeCTS.token;

      ChangeValueAsync(ShaderHash.PlayerPortrait._ScanStrength, uiSO.Player.Shader.SoftHitScanStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._GlitchStrength, uiSO.Player.Shader.SoftHitGlitchStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._EdgeNoiseStrength, uiSO.Player.Shader.SoftHitEdgeNoiseStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._EdgeNoiseWidth, uiSO.Player.Shader.SoftHitEdgeNoiseWidth, token).Forget();
      LoopRGBAsync(uiSO.Player.Shader.SoftHitRGBRange, uiSO.Player.Shader.SoftHitRGBSpeed, token).Forget();
    }

    public void OnStrongHit()
    {
      changeCTS.Cancel();
      changeCTS.Create();
      var token = changeCTS.token;

      ChangeValueAsync(ShaderHash.PlayerPortrait._ScanStrength, uiSO.Player.Shader.StrongHitScanStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._GlitchStrength, uiSO.Player.Shader.StrongHitGlitchStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._EdgeNoiseStrength, uiSO.Player.Shader.StrongHitEdgeNoiseStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._EdgeNoiseWidth, uiSO.Player.Shader.StrongHitEdgeNoiseWidth, token).Forget();
      LoopRGBAsync(uiSO.Player.Shader.StrongHitRGBRange, uiSO.Player.Shader.StrongHitRGBSpeed, token).Forget();
    }

    public void OnElectric()
    {
      changeCTS.Cancel();
      changeCTS.Create();
      var token = changeCTS.token;

      ChangeValueAsync(ShaderHash.PlayerPortrait._GlitchStrength, uiSO.Player.Shader.ElectricGlitchStrength, token).Forget();
    }

    public void OnComplete()
    {
      changeCTS.Cancel();
      changeCTS.Create();
      var token = changeCTS.token;

      ChangeValueAsync(ShaderHash.PlayerPortrait._ScanStrength, uiSO.Player.Shader.CompleteScanStrength, token).Forget();
    }

    public void OnStun()
    {
      changeCTS.Cancel();
      changeCTS.Create();
      var token = changeCTS.token;

      ChangeValueAsync(ShaderHash.PlayerPortrait._Flicker, uiSO.Player.Shader.StunFlicker, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._EdgeNoiseStrength, uiSO.Player.Shader.StunEdgeNoiseStrength, token).Forget();
      ChangeValueAsync(ShaderHash.PlayerPortrait._EdgeNoiseWidth, uiSO.Player.Shader.StunEdgeNoiseWidth, token).Forget();
      SpikeRGBAsync(
        uiSO.Player.Shader.StunRGBRange,
        uiSO.Player.Shader.StunRGBIntervalSpikeDuration,
        uiSO.Player.Shader.StunRGBIntervalMin,
        uiSO.Player.Shader.StunRGBIntervalMax,
        token).Forget();
    }

    private async UniTask ChangeValueAsync(
      int id,
      float targetValue, 
      CancellationToken token)
    {
      var beginValue = image.material.GetFloat(id);
      await PlayTimerAsync(
        t =>
        {
          image.material.SetFloat(id, Mathf.Lerp(beginValue, targetValue, t));
        }, token);

      await UniTask.WaitUntil(() => token.IsCancellationRequested);

      if (image != null) 
      {
        image.material.SetFloat(id, initializedValues[id]);
      }        
    }

    private async UniTask LoopRGBAsync(
      float range, 
      float speed, 
      CancellationToken token)
    {
      var time = range;
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var t = Mathf.PingPong(time, range * 2.0f) - range;
          image.material.SetFloat(ShaderHash.PlayerPortrait._RGBSplit, t);
          time += Time.deltaTime * speed;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException)
      {

      }
    }

    private async UniTask SpikeRGBAsync(
      float range,
      float waitDuration,
      float intervalMin,
      float intervalMax,
      CancellationToken token)
    {
      try
      {
        var id = ShaderHash.PlayerPortrait._RGBSplit;
        var isDelaying = true;
        var duration = waitDuration;
        var targetInterval = 0.0f;
        image.material.SetFloat(id, UnityEngine.Random.Range(-range, range));
        while (true)
        {
          token.ThrowIfCancellationRequested();

          duration -= Time.deltaTime;
          if (duration <= 0.0f)
          {
            if (isDelaying)
            {
              image.material.SetFloat(id, 0.0f);
              targetInterval = UnityEngine.Random.Range(intervalMin, intervalMax);
              duration = targetInterval;
              isDelaying = false;
            }
            else
            {
              image.material.SetFloat(id, UnityEngine.Random.Range(-range, range));
              duration = waitDuration;
              isDelaying = true;
            }
          }
          
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask UpdateAlphaAsync()
    {
      var token = alphaCTS.token;
      try
      {
        var id = ShaderHash.PlayerPortrait._Alpha;
        var targetDuration = UnityEngine.Random.Range(uiSO.Player.Shader.FadeDurationMin, uiSO.Player.Shader.FadeDurationMin);
        var duration = targetDuration;
        var currentAlpha = image.material.GetFloat(id);
        var targetAlpha = UnityEngine.Random.Range(uiSO.Player.Shader.AlphaMin, uiSO.Player.Shader.AlphaMax);
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var t = 1.0f - (duration / targetDuration);
          image.material.SetFloat(id, Mathf.Lerp(currentAlpha, targetAlpha, t));

          duration -= Time.deltaTime;

          if(duration <= 0.0f)
          {
            targetDuration = UnityEngine.Random.Range(uiSO.Player.Shader.FadeDurationMin, uiSO.Player.Shader.FadeDurationMin);
            duration = targetDuration;
          }
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
      alphaCTS.Dispose();
      changeCTS.Dispose();
    }

    private async UniTask PlayTimerAsync(
      UnityAction<float> onProgress, 
      CancellationToken token)
    {
      var duration = 0.0f;
      var targetDuration = uiSO.Player.Shader.DefaultValueChangeDuration;
      try
      {
        while (duration < targetDuration)
        {
          token.ThrowIfCancellationRequested();

          var t = duration / targetDuration;
          onProgress?.Invoke(t);
          duration += Time.deltaTime;
          await UniTask.Yield();
        }

        onProgress?.Invoke(1.0f);
      }
      catch (OperationCanceledException) { }
    }
  }
}
