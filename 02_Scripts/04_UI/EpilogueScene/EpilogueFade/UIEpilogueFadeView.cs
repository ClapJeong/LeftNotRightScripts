using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace LR.UI.EpilogueScene
{
  public class UIEpilogueFadeView : BaseUIView
  {
    [SerializeField] private CanvasGroup canvasGroup;

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      try
      {
        visibleState = Enum.VisibleState.Hiding;
        var duration = isImmediately ? 0.0f : UISO.Epilogue.FirstHideDuration;
        await canvasGroup.DOFade(0.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = Enum.VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      try
      {
        visibleState = Enum.VisibleState.Showing;
        var duration = isImmediately ? 0.0f : UISO.Epilogue.FirstShowDuration;
        await canvasGroup.DOFade(1.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = Enum.VisibleState.Showen;
      }
      catch (OperationCanceledException) { }
    }
  }
}
