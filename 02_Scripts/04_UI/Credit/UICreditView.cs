using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace LR.UI.Credit
{
  public class UICreditView : BaseUIView
  {
    [field: SerializeField] public CanvasGroup CanavsGroup {  get; private set; }
    [field: SerializeField] public LocalizeStringEvent ThanksTMP { get; private set; }
    [field: SerializeField] public RectTransform RightyRectTransform { get; private set; }
    [field: SerializeField] public RectTransform LeftyRectTransform { get; private set; }
    [field: SerializeField] public RectTransform JobOutlineRectTransform { get; private set; }
    [field: SerializeField] public RectTransform NameOutlineRectTransform { get; private set; }
    [field: SerializeField] public RectTransform DoctorRectTransform { get; private set; }
    [field: Range(0.0f, 1.0f)] [field: SerializeField] public float CornerRange { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      var duration = isImmediately ? 0.0f : UISO.Credit.ShowHideDuration;
      try
      {
        await CanavsGroup.DOFade(0.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      var duration = isImmediately ? 0.0f : UISO.Credit.ShowHideDuration;
      try
      {
        await CanavsGroup.DOFade(1.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }
  }
}