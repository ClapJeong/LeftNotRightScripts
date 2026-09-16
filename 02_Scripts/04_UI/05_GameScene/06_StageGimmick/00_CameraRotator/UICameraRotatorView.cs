using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.StageGimmick
{
  public class UICameraRotatorView : BaseUIView
  {
    [System.Serializable]
    public class ImageSet
    {
      [field: SerializeField] public Image Image { get; private set; }
      [HideInInspector] public Direction direction;
    }
    [SerializeField] private CanvasGroup canvasGroup;
    [field: SerializeField] public RectTransform PreviewRectTransform { get; private set; }
    [field: SerializeField] public List<ImageSet> LeftInputImageSets { get; private set; }
    [field: SerializeField] public List<ImageSet> RightInputImageSets { get; private set; }

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
