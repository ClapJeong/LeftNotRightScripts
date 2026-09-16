using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using LR.UI.Enum;
using DG.Tweening;

namespace LR.UI.GameScene.InputProgress
{
  public class UIDefaultProgressView : BaseUIView
  {
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public RectTransform ImageRootRectTransform { get; private set; }
    [field: SerializeField] public Image FillImage { get; private set; }
    [field: SerializeField] public RectTransform InputRootRectTransform { get; private set; }
    [field: SerializeField] public Image InputIdleImage { get; private set; }
    [field: SerializeField] public Image InputPerformedImage { get; private set; }
    [field: SerializeField] public Image InputGuideImage { get; private set; }
    [field: SerializeField] public Image InputPressImage { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      await CanvasGroup.DOFade(0.0f, UISO.Progress.HideDuration);
      visibleState = VisibleState.Showen;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      await UniTask.CompletedTask;
      visibleState = VisibleState.Showen;
    }
  }
}
