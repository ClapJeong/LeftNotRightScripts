using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LR.UI.Enum;
using System;
using DG.Tweening;

namespace LR.UI.Lobby
{
  public class UIStageButtonView : BaseUIView
  {    
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public CanvasGroup SubCanvasGroup { get; private set; }
    [field: SerializeField] public RectTransform ButtonRectTransform { get; private set; }
    [field: SerializeField] public TextMeshProUGUI TMP {  get; private set; }
    [field: SerializeField] public Image BackgroundImage { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet DirectionSet { get; private set; }
    [field: SerializeField] public Vector2 HideDirection { get; private set; }
    [field: SerializeField] public Animator PerfectIconAnimator { get; private set; }
    [field: Header("[ Clear ]")]
    [field: SerializeField] public Image Outline { get; private set; }
    [field: SerializeField] public Image Sweep { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      try
      {
        var duration = isImmediately ? 0.0f : UISO.Lobby.StageButtonShowDuration;
        var pos = HideDirection.normalized * UISO.Lobby.StageButtonShowHideLength;
        var alpha = 0.0f;
        var beginAlpha = 0.5f;
        CanvasGroup.alpha = beginAlpha;
        await DOTween
          .Sequence()
          .Join(RectTransform.DOAnchorPos(pos, duration))
          .Join(CanvasGroup.DOFade(alpha, duration))          
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException)
      {

      }
      visibleState = VisibleState.Hidden;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;
      try
      {
        var duration = isImmediately ? 0.0f : UISO.Lobby.StageButtonShowDuration;
        var pos = Vector2.zero;
        var alpha = 1.0f;
        await DOTween
          .Sequence()
          .Join(RectTransform.DOAnchorPos(pos, duration))
          .Join(CanvasGroup.DOFade(alpha, duration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException)
      {

      }
      visibleState = VisibleState.Showen;
    }
  }
}