using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LR.Manager.Stage
{
  public class RestartShaderController : IDisposable
  {
    [Inject] private readonly IGameDataProvider gameDataProvider = null;
    [Inject] private readonly UISO uiSO = null;

    private readonly Material cctvMaterial;

    private readonly CTSContainer cts = new();

    public RestartShaderController([Inject(Id ="ShaderImage")] RawImage shaderRawImge)
    {
      this.cctvMaterial = shaderRawImge.material;
    }

    public async UniTask PlayRestartMatAsync()
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;
      var stage = gameDataProvider.GetSelectedStage();
      var isVertical = (stage - 1) % 2 == 0;
      cctvMaterial.SetFloat(ShaderHash.CCTV._ScanAxis, isVertical ? 0.0f : 1.0f);
      var targetHash = isVertical ? ShaderHash.CCTV._ScanProgressY 
                                  : ShaderHash.CCTV._ScanProgressX;
      var isLower = (stage - 1) / 2 == 0;
      var beginValue =  isLower ? 1.0f
                                : 0.0f;
      var endValue = isLower ? 0.0f
                             : 1.0f;
      var duration = uiSO.Stage.RestartMatDuration;
      cctvMaterial.SetFloat(ShaderHash.CCTV._ScanWidth, uiSO.Stage.RestartMatSize);
      try
      {
        while(duration > 0.0f)
        {
          token.ThrowIfCancellationRequested();
          var t = 1.0f - (duration / uiSO.Stage.RestartMatDuration);
          cctvMaterial.SetFloat(targetHash, Mathf.Lerp(beginValue, endValue, t));
          duration -= Time.deltaTime;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        cctvMaterial.SetFloat(ShaderHash.CCTV._ScanWidth, 0.0f);
        cctvMaterial.SetFloat(targetHash, 0.0f);
      }
    }

    public void Dispose()
    {
      cts.Dispose();
    }
  }
}
