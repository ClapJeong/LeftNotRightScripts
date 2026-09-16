using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.Input;
using System;
using System.Threading;
using UnityEngine;

namespace LR.UI.Preloading
{
  public class UIDemoContinueView : BaseUIView
  {
    [field: SerializeField] public UIInputView LeftInputView { get; private set; }
    [field: SerializeField] public UIInputView RightInputView { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public UISubmitDirectionSet ContinueDirectionSet { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet ResetDirectionSet { get; private set; }
    [field: SerializeField] public Transform IndicatorRoot { get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

    private readonly float fadeDuration = 1.0f;

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      var alpha = 0.0f;
      var duration = fadeDuration;
      try
      {
        await CanvasGroup.DOFade(alpha, duration).ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      var alpha = 1.0f;
      var duration = fadeDuration;
      try
      {
        await CanvasGroup.DOFade(alpha, duration).ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }
  }
}
