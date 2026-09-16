using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using LR.UI.Enum;
using System;
using TMPro;
using UnityEngine.Localization.Components;

namespace LR.UI.GameScene.Stage
{
  public class UIStageBeginView : BaseUIView
  {
    [SerializeField] private CanvasGroup canvasGroup;

    [field: SerializeField] public TextMeshProUGUI StageInfoTMP { get; private set; }
    [field: SerializeField] public Animator GimmickIconAnimator {  get; private set; }
    [field: SerializeField] public TextMeshProUGUI GimmickDescriptionTMP { get; private set; }
    [field: SerializeField] public LocalizeStringEvent GimmickLocalizeStringEvent { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public TextMeshProUGUI LeftGuideTMP {  get; private set; }

    [field: SerializeField] private RectTransform leftContainer;
    [field: SerializeField] public Image LeftInputImage {  get; private set; }

    [field: Space(5)]
    [field: SerializeField] public TextMeshProUGUI RightGuideTMP { get; private set; }
    [field: SerializeField] private RectTransform rightContainer;
    [field: SerializeField] public Image RightInputImage { get; private set; }

    [SerializeField] private float hideLength;

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      var duration = isImmediately ? 0.0f : UISO.Stage.BeginFadeDuration;
      try
      {
        await DOTween.Sequence()
          .Join(leftContainer.DOAnchorPos(-hideLength * Vector2.right, duration))
          .Join(rightContainer.DOAnchorPos(hideLength * Vector2.right, duration))
          .Join(canvasGroup.DOFade(0.0f, duration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
        gameObject.SetActive(false);

        visibleState = VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;
      var duration = isImmediately ? 0.0f : UISO.Stage.BeginFadeDuration;
      try
      {
        gameObject.SetActive(true);
        await DOTween.Sequence()
          .Join(leftContainer.DOAnchorPos(Vector2.zero, duration))
          .Join(rightContainer.DOAnchorPos(Vector2.zero, duration))
          .Join(canvasGroup.DOFade(1.0f, duration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);

        visibleState = VisibleState.Showen;
      }
      catch (OperationCanceledException) { }
    }
  }
}