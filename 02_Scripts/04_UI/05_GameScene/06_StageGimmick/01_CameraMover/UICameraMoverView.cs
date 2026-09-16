using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.StageGimmick
{
  public class UICameraMoverView : BaseUIView
  {
    [System.Serializable]
    public class RectSet
    {
      [field: SerializeField] public Vector2 AnchoredPosition { get; private set; }
      [field: SerializeField] public Vector2 Pivot { get; private set; }
      [field: SerializeField] public Vector2 Size {  get; private set; }
      [field: SerializeField] public Vector2 AnchorMin { get; private set; }
      [field: SerializeField] public Vector2 AnchorMax { get; private set; }
    }
    [field: SerializeField] public RectSet LeftRectSet { get; private set; }
    [field: SerializeField] public RectSet OriginRectSet { get; private set; }
    [field: SerializeField] public RectSet RightRectSet { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public RectTransform ContentRectTransform { get; private set; }
    [field: SerializeField] public Image OutlineImage { get; private set; }
    [field: SerializeField] public RectTransform ArrowRectTransform { get; private set; }
    [field: SerializeField] public Image ArrowImage { get; private set; }
    [SerializeField] private CanvasGroup canvasGroup;    


    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      var fadeDuration = isImmediately ? 0.0f : UISO.StageGimmick.FadeDuration;
      var moveDuration = isImmediately ? 0.0f : UISO.StageGimmick.MoveDuration;
      var targetAlpha = 0.0f;
      var targetPositon = new Vector2(0.0f, -UISO.StageGimmick.HideLength);
      try
      {
        await DOTween
          .Sequence()
          .Join(canvasGroup.DOFade(targetAlpha, fadeDuration))
          .Join(RectTransform.DOAnchorPos(targetPositon, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = Enum.VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }

    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      var fadeDuration = isImmediately ? 0.0f : UISO.StageGimmick.FadeDuration;
      var moveDuration = isImmediately ? 0.0f : UISO.StageGimmick.MoveDuration;
      var targetAlpha = 1.0f;
      var targetPositon = Vector2.zero;
      try
      {
        await DOTween
          .Sequence()
          .Join(canvasGroup.DOFade(targetAlpha, fadeDuration))
          .Join(RectTransform.DOAnchorPos(targetPositon, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = Enum.VisibleState.Showen;
      }
      catch (OperationCanceledException) { }

    }
  }
}
