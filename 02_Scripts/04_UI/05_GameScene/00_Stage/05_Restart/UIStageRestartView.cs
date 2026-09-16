using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.Stage
{
  public class UIStageRestartView : BaseUIView
  {
    [SerializeField] private RectTransform textRectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform iconRectTransform;
    [field: SerializeField] public Image RestartInputImage {  get; set; }
    [field: SerializeField] public Image RestartDelayFillImage {  get; set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      var fadeDuration = isImmediately ? 0.0f : UISO.Stage.RestartUIFadeDuration;
      var moveDuration = isImmediately ? 0.0f : UISO.Stage.RestartUIMoveDuration;
      var textTargetPosition = new Vector2(0.0f, UISO.Stage.RestartTextMoveLength);
      var iconTargetPosition = new Vector2(0.0f, UISO.Stage.RestartIconMoveLength);
      var targetAlpha = 0.0f;
      try
      {
        await DOTween
          .Sequence()
          .Join(textRectTransform.DOAnchorPos(textTargetPosition, moveDuration))
          .Join(iconRectTransform.DOAnchorPos(iconTargetPosition, moveDuration))
          .Join(canvasGroup.DOFade(targetAlpha, fadeDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
        visibleState = Enum.VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }      
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      var fadeDuration = isImmediately ? 0.0f : UISO.Stage.RestartUIFadeDuration;
      var moveDuration = isImmediately ? 0.0f : UISO.Stage.RestartUIMoveDuration;
      var textTargetPosition = Vector2.zero;
      var iconTargetPosition = Vector2.zero;
      var targetAlpha = 1.0f;
      try
      {
        await DOTween
          .Sequence()
          .Join(textRectTransform.DOAnchorPos(textTargetPosition, moveDuration))
          .Join(iconRectTransform.DOAnchorPos(iconTargetPosition, moveDuration))
          .Join(canvasGroup.DOFade(targetAlpha, fadeDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
        visibleState = Enum.VisibleState.Showen;
      }
      catch (OperationCanceledException) { }
    }
  }
}