using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerStatePortraitView : BaseUIView
  {
    [SerializeField] private Vector2 hideAnchoredPosition;
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public Image VignetteImage { get; private set; }
    [field: SerializeField] public RectTransform ContentRectTransform { get; private set; }
    [field: SerializeField] public Image PortraitImage { get; private set; }
    [field: SerializeField] public RectTransform PortraitImageRectTransform { get; private set; }
    [field: SerializeField] public UIEasyHatView EasyHatView { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      try
      {
        var fadeDuration = isImmediately ? 0.0f : UISO.Player.FadeDuration;
        var moveDuration = isImmediately ? 0.0f : UISO.Player.MoveDuration;
        await DOTween
          .Sequence()
          .Join(RectTransform.DOAnchorPos(hideAnchoredPosition, moveDuration))
          .Join(CanvasGroup.DOFade(0.0f, fadeDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }

    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;
      try
      {
        var fadeDuration = isImmediately ? 0.0f : UISO.Player.FadeDuration;
        var moveDuration = isImmediately ? 0.0f : UISO.Player.MoveDuration;
        await DOTween
          .Sequence()
          .Join(RectTransform.DOAnchorPos(Vector2.zero, moveDuration))
          .Join(CanvasGroup.DOFade(1.0f, fadeDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = VisibleState.Showen;
      }
      catch (OperationCanceledException) { }
    }
  }
}
