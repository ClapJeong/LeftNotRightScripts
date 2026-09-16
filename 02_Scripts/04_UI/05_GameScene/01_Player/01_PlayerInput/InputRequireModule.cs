using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace LR.UI.GameScene.Player.PlayerInput
{
  public class InputRequireModule : IGimmickModule
  {
    private readonly RectTransform rootRectTransform;
    private readonly UISO uiSO;

    private readonly CTSContainer shakeCTS = new();
    private readonly Vector3 originAnchordPosition;
    private float normalized;
    private bool isExhausted;

    public InputRequireModule(RectTransform rootRectTransform, UISO uiSO)
    {
      this.rootRectTransform = rootRectTransform;
      this.uiSO = uiSO;
      originAnchordPosition = rootRectTransform.anchoredPosition;

      var token = shakeCTS.token;
      ShakeAsync(token).Forget();
    }

    public void UpdateExhausted(bool isExhausted)
      => this.isExhausted = isExhausted;

    public void UpdateNormalized(float mormalized)
      => this.normalized = mormalized;

    private async UniTask ShakeAsync(CancellationToken token)
    {
      try
      {
        while (!token.IsCancellationRequested)
        {
          token.ThrowIfCancellationRequested();

          rootRectTransform.anchoredPosition = originAnchordPosition;

          var strength = isExhausted ? uiSO.StageGimmick.InputRequireShakeStrenghMin : Mathf.Lerp(
            uiSO.StageGimmick.InputRequireShakeStrenghMin,
            uiSO.StageGimmick.InputRequireShakeStrenghMax,
            1.0f - normalized);
          var vibrato = Mathf.FloorToInt(isExhausted ? uiSO.StageGimmick.InputRequireShakeVibratoMin : Mathf.Lerp(
            uiSO.StageGimmick.InputRequireShakeVibratoMin,
            uiSO.StageGimmick.InputRequireShakeVibratoMax,
            1.0f - normalized));
          await
            rootRectTransform
            .DOShakeAnchorPos(uiSO.StageGimmick.InputRequireShakeDuration, strength, vibrato)
            .ToUniTask(TweenCancelBehaviour.Kill, token);

          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException)
      {

      }
    }

    public void Dispose()
    {
      shakeCTS.Dispose();
    }
  }
}
