using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.StageGimmick
{
  public class UIInputRequireView : BaseUIView
  {
    [System.Serializable]
    public class ImageSet
    {
      [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
      [field: SerializeField] public Image BackgroundImage { get; private set; }
      [field: SerializeField] public Image FillImage {  get; private set; }      
    }

    [SerializeField] private CanvasGroup canvasGroup;
    [field: SerializeField] public RectTransform RootRectTransform { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public ImageSet LeftSet {  get; private set; }
    [field: SerializeField] public ImageSet RightSet { get; private set; }

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
