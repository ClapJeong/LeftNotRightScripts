using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace LR.UI
{
  public class UIRedPillButtonView : BaseUIView
  {
    [field: SerializeField] public UISubmitDirectionSet Submit { get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public CanvasGroup DescriptionCanvasGroup { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }
  }
}
