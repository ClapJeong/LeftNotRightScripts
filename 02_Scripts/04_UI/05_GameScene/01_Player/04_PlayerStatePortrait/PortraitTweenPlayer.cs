using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Table.Player;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LR.UI.GameScene.Player.PlayerPortrait
{
  public class PortraitTweenPlayer
  {
    private readonly PlayerCollisionData playerCollisionData;
    private readonly UISO uiSO;
    private readonly Vector2 originPortriatAnchoredPos;
    private readonly RectTransform contentRectTransform;
    private readonly RectTransform portraigeRectTransform;
    private readonly Image vignetteImage;
    private readonly Image portraitImage;

    public PortraitTweenPlayer(
      PlayerCollisionData playerCollisionData,
      UISO uiSO, 
      Vector3 originPortriatAnchoredPos,
      RectTransform contentRectTransform,
      RectTransform portraigeRectTransform, 
      Image vignetteImage,
      Image portraitImage)
    {
      this.playerCollisionData = playerCollisionData;
      this.uiSO = uiSO;
      this.originPortriatAnchoredPos = originPortriatAnchoredPos;
      this.contentRectTransform = contentRectTransform;
      this.portraigeRectTransform = portraigeRectTransform;
      this.vignetteImage = vignetteImage;
      this.portraitImage = portraitImage;

      vignetteImage.SetAlpha(0.0f);
    }

    public async UniTask ExhaustedAsync(CancellationToken token)
    {
      try
      {
        var duration = 0.0f;
        var targetDuration = uiSO.Player.ExhaustedMoveDuration;
        while (duration < targetDuration)
        {
          token.ThrowIfCancellationRequested();
          var t = uiSO.Player.ExhaustedMoveCurve.Evaluate(duration / targetDuration);
          var targetAnchoredPosition = Vector2.LerpUnclamped(Vector2.zero, uiSO.Player.ExhaustedHidePosition, t);
          portraigeRectTransform.anchoredPosition = originPortriatAnchoredPos + targetAnchoredPosition;

          duration += Time.deltaTime;
          await UniTask.Yield();
        }
        portraitImage.SetAlpha(0.0f);
        portraigeRectTransform.anchoredPosition = uiSO.Player.ExhaustedHidePosition;
      }
      catch (OperationCanceledException) { }
    }

    public void ResetExhaust()
    {
      portraigeRectTransform.anchoredPosition = originPortriatAnchoredPos;
      portraitImage.SetAlpha(1.0f);
    }

    public async UniTask WallDamageShakeAsync(UnityAction onComplete, CancellationToken token)
    {
      var duration = uiSO.Player.DamagedPortraitShakeDuration;
      var length = uiSO.Player.DamagedPortraitShakeStrengh;

      try
      {
      await DOTween
        .Sequence()
        .Join(contentRectTransform.DOPunchScale(Vector3.one * uiSO.Player.DamagedScaleValue, uiSO.Player.DamagedScaleDuration))
        .Join(portraigeRectTransform.DOPunchAnchorPos(GetRandomPointOnCircle(length), duration, 1))
        .AppendInterval(playerCollisionData.WallBumpAnimationDuration)
        .OnComplete(() =>
        {
          onComplete?.Invoke();
          portraigeRectTransform.anchoredPosition = originPortriatAnchoredPos;
        })
        .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }

    public async UniTask WallDamageVignetteAsync(CancellationToken token)
    {
      vignetteImage.SetAlpha(uiSO.Player.WallBumpVignetteAlpha);
      var duration = uiSO.Player.DamagedPortraitShakeDuration; 
      try
      {
        await DOTween
        .Sequence()
        .Join(vignetteImage.DOFade(0.0f, duration))
        .OnComplete(() =>
        {
          vignetteImage.SetAlpha(0.0f);
        })
        .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }      
    }

    public async UniTask StrongDamageShakeAsync(UnityAction onCompelte, CancellationToken token)
    {
      var duration = uiSO.Player.DamagedPortraitShakeDuration;
      try
      {
        await DOTween
          .Sequence()
          .Join(contentRectTransform.DOPunchScale(Vector3.one * uiSO.Player.DamagedScaleValue, uiSO.Player.DamagedScaleDuration))
          .Join(portraigeRectTransform.DOShakeAnchorPos(duration, uiSO.Player.DamagedPortraitShakeStrengh, uiSO.Player.DamagedPortraitShakeVibrato, 360.0f))
          .OnComplete(() =>
          {
            onCompelte?.Invoke();
            portraigeRectTransform.anchoredPosition = originPortriatAnchoredPos;
          })
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }

    public async UniTask StrongDamageVignetteAsync(CancellationToken token)
    {
      vignetteImage.SetAlpha(uiSO.Player.DamagedVignetteAlpha);
      var duration = uiSO.Player.DamagedPortraitShakeDuration;
      try
      {
        await DOTween
        .Sequence()
        .Join(vignetteImage.DOFade(0.0f, duration))
        .OnComplete(() =>
        {
          vignetteImage.SetAlpha(0.0f);
        })
        .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }

    public async UniTask ElectricShakeAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          portraigeRectTransform.anchoredPosition = originPortriatAnchoredPos + GetRandomPointOnCircle(uiSO.Player.ElectricShakeRange);
          await UniTask.WaitForSeconds(uiSO.Player.ElectricShakeInterval, false, PlayerLoopTiming.Update, token);
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        if(portraigeRectTransform != null)
          portraigeRectTransform.anchoredPosition = originPortriatAnchoredPos;
      }
    }

    private static Vector2 GetRandomPointOnCircle(float length)
    {
      float radius = length * 0.5f;
      float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

      float x = Mathf.Cos(angle) * radius;
      float y = Mathf.Sin(angle) * radius;

      return new Vector2(x, y);
    }

    public async UniTask ClearEnterAsync(CancellationToken token)
    {
      try
      {
        var upDuration = uiSO.Player.ClearEnterUpduration;
        var downDuration = uiSO.Player.ClearEnterDownDuration;
        var length = uiSO.Player.ClearEnterLength;

        await DOTween
          .Sequence()
          .Join(contentRectTransform.DOAnchorPosY(length, upDuration))
          .Append(contentRectTransform.DOAnchorPosY(0.0f, downDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException)
      {
      }
    }
  }
}
