using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LR.UI.Enum;
using DG.Tweening;

namespace LR.UI.GameScene.InputQTE
{
  public class UIDefaultQTEView : BaseUIView
  {
    [field: SerializeField] public CanvasGroup ContentCanvasGroup { get; private set; }
    [field: SerializeField] public Image SequenceDurationImage { get; private set; }
    [field: SerializeField] public RectTransform CaptchaRoot { get; private set; }
    [field: SerializeField] public List<Animator> CaptchaAnimators { get; private set; }
    [field: SerializeField] public RectTransform InputRoot { get; private set; }
    [field: SerializeField] public Image IdleImage {  get; private set; }
    [field: SerializeField] public Image InputDurationFillImage {  get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      await ContentCanvasGroup.DOFade(0.0f, UISO.QTE.HideDuration);
      visibleState = VisibleState.Hidden;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;
      await UniTask.CompletedTask;
      visibleState = VisibleState.Showen;
    }
  }
}
