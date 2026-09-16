using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.Dialogue
{
  public class UITalkingInputsView : BaseUIView
  {
    [System.Serializable]
    public class InputImageSet
    {
      [field: SerializeField] public RectTransform Root { get; private set; }
      [field: SerializeField] public Image Idle { get; private set; }
      [field: SerializeField] public Image Input { get; private set; }
    }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public InputImageSet Left {  get; private set; }
    [field: SerializeField] public InputImageSet Right { get; private set; }
    [field: SerializeField] public Image SkipIcon { get; private set; }
    [field: SerializeField] public Image SkipProgressImage { get; private set; }
    [field: SerializeField] public RectTransform SkipProgressRectTransform {  get; private set; }
    [field: SerializeField] public RectTransform SkipInput { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      var duration = isImmediately ? 0.0f : UISO.Dialogue.OverlayFadeDuration;
      try
      {
        await CanvasGroup.DOFade(0.0f, duration);
        visibleState = VisibleState.Hidden;
        Left.Root.gameObject.SetActive(false);
        Right.Root.gameObject.SetActive(false);
        SkipIcon.enabled = false;
        SkipProgressImage.enabled = false;
        gameObject.SetActive(false);
      }
      catch (OperationCanceledException) { }
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      gameObject.SetActive(true);
      Left.Root.gameObject.SetActive(true);
      Right.Root.gameObject.SetActive(true);
      SkipIcon.enabled = true;
      SkipProgressImage.enabled = true;
      visibleState = VisibleState.Showing;
      var duration = isImmediately ? 0.0f : UISO.Dialogue.OverlayFadeDuration;
      try
      {
        await CanvasGroup.DOFade(1.0f, duration);
        visibleState = VisibleState.Showen;
      }
      catch (OperationCanceledException) { }
    }
  }
}
