using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using LR.UI.Enum;
using System;
using DG.Tweening;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerRootView : BaseUIView
  {
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector2 hideAnchoredPosition;
    [field: SerializeField] public UIPlayerInputView InputView { get; private set; }
    [field: SerializeField] public UIPlayerStatePortraitView StatePortraitView { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }
  }
}