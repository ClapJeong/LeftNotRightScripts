using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using LR.UI.Enum;

namespace LR.UI.Lobby
{
  public abstract class UIBaseLobbyPanelView : BaseUIView
  {
    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] private Vector2 hideAnchoredPosition;
    [SerializeField] private Vector2 showAnchoredPosition;
    [SerializeField] private CanvasGroup canvasGroup;

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;

      var moveDuration = isImmediately ? 0.0f : UISO.Lobby.PanelShowDuration;
      var hideDuration = isImmediately ? 0.0f : UISO.Lobby.PanelHideDuration;
      await DOTween
        .Sequence()
        .Join(contentRectTransform.DOAnchorPos(hideAnchoredPosition, moveDuration))
        .Join(canvasGroup.DOFade(0.0f, hideDuration))
        .ToUniTask(TweenCancelBehaviour.Kill, token);

      visibleState = VisibleState.Hidden;
      gameObject.SetActive(false);
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.WaitForSeconds(UISO.Lobby.PanelShowDelay, false, PlayerLoopTiming.Update, token);

      visibleState = VisibleState.Showing;
      gameObject.SetActive(true);

      var moveDuration = isImmediately ? 0.0f : UISO.Lobby.PanelShowDuration;
      var hideDuration = isImmediately ? 0.0f : UISO.Lobby.PanelHideDuration;
      await DOTween
        .Sequence()
        .Join(contentRectTransform.DOAnchorPos(showAnchoredPosition, moveDuration))
        .Join(canvasGroup.DOFade(1.0f, hideDuration))
        .ToUniTask(TweenCancelBehaviour.Kill, token);

      visibleState = VisibleState.Showen;
    }
  }
}
