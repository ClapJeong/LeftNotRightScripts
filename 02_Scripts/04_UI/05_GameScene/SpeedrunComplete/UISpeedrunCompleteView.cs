using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.GameScene.GlobalRecord;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.Speedrun
{
  public class UISpeedrunCompleteView : BaseUIView
  {
    [field: SerializeField] public RectTransform LeftPortraitRectTransform { get; private set; }
    [field: SerializeField] public Image LeftImage {  get; private set; }
    [field: SerializeField] public RectTransform RightPortraitRectTransform { get; private set; }
    [field: SerializeField] public Image RightImage { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet ExitButton { get; private set; }
    [field: SerializeField] public Transform IndicatorRoot { get; private set; }
    [field: SerializeField] public CanvasGroup TextCanvasGroup { get; private set;  }
    [field: SerializeField] public UISpeedrunLeaderboardView SafestLeaderboardView { get; private set; }
    [field: SerializeField] public UISpeedrunLeaderboardView FastestLeaderboardView { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      LeftPortraitRectTransform.anchoredPosition = new Vector2(0.0f, UISO.Stage.SpeedrunPortraitHidePositon);
      LeftImage.SetAlpha(0.0f);
      RightPortraitRectTransform.anchoredPosition = new Vector2(0.0f, UISO.Stage.SpeedrunPortraitHidePositon);
      RightImage.SetAlpha(0.0f);
      ExitButton.RectTransform.anchoredPosition = new Vector2(0.0f, UISO.Stage.SpeedrunExitButtonHidePositon);
      TextCanvasGroup.alpha = 0.0f;
      ExitButton.gameObject.SetActive(false);
      SafestLeaderboardView.CanvasGroup.alpha = 0.0f;
      FastestLeaderboardView.CanvasGroup.alpha = 0.0f;
      visibleState = Enum.VisibleState.Hiding;
      await UniTask.CompletedTask;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      try
      {
        var portraitDuration = UISO.Stage.SpeedrunPortraitShowDuration;
        await DOTween
          .Sequence()
          .Join(LeftPortraitRectTransform.DOAnchorPosY(0.0f, portraitDuration))
          .Join(LeftImage.DOFade(1.0f, portraitDuration))
          .Join(RightPortraitRectTransform.DOAnchorPosY(0.0f, portraitDuration))
          .Join(RightImage.DOFade(1.0f, portraitDuration))
          .Join(SafestLeaderboardView.CanvasGroup.DOFade(1.0f, portraitDuration))
          .Join(FastestLeaderboardView.CanvasGroup.DOFade(1.0f, portraitDuration))
          .AppendCallback(() =>
          {
            TextCanvasGroup.alpha = 1.0f;
          })
          .AppendInterval(UISO.Stage.SpeedrunExitButtonDelay)
          .AppendCallback(() =>
          {            
            ExitButton.gameObject.SetActive(true);
          })
          .Append(ExitButton.RectTransform.DOAnchorPosY(0.0f, UISO.Stage.SpeedrunExitButtonShowDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }
  }
}