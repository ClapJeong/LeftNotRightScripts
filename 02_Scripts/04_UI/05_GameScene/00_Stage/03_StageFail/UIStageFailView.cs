using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using LR.UI.Enum;
using System;
using UnityEngine.UI;
using TMPro;

namespace LR.UI.GameScene.Stage
{
  public class UIStageFailView : BaseUIView
  {    
    [SerializeField] private Vector2 hidePosition;
    [SerializeField] private Vector2 showPosition;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform doctorImageRectTransform;
    [SerializeField] private Vector2 doctorHidePosition;

    [field: SerializeField] public Image DoctorImage { get; private set; }
    [field: SerializeField] public Transform IndicatorRoot { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet RestartSubmitSet { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet QuitSubmitSet { get; private set; }

    [field: SerializeField] public TextMeshProUGUI LeftFailLogText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI RightFailLogText { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;

      var duration = isImmediately ? 0.0f : UISO.Stage.UIMoveDefaultDuration;
      try
      {
        await DOTween.Sequence()
                .Join(content.DOAnchorPos(hidePosition, duration))
                .Join(doctorImageRectTransform.DOAnchorPos(doctorHidePosition, duration))
                .Join(canvasGroup.DOFade(0.0f, duration))
                .OnComplete(() =>
                {
                  visibleState = VisibleState.Hidden;
                  gameObject.SetActive(false);
                })
                .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      gameObject.SetActive(true);
      visibleState = VisibleState.Showing;

      var duration = isImmediately ? 0.0f : UISO.Stage.UIMoveDefaultDuration;
      try
      {
        await DOTween.Sequence()
                .Join(content.DOAnchorPos(showPosition, duration))
                .Join(canvasGroup.DOFade(1.0f, duration))
                .Join(doctorImageRectTransform.DOAnchorPos(Vector2.zero, duration))
                .OnComplete(() =>
                {
                  visibleState = VisibleState.Showen;
                })
                .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }
  }
}