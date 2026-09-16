using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace LR.UI.EpilogueScene
{
  public class UIEpilogueRootView : BaseUIView
  {
    [field: SerializeField] public UIEpilogueFadeView FadeView { get; private set; }
    [field: SerializeField] public UIEpilogueContentView ContentView { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      await UniTask.CompletedTask;
      visibleState = Enum.VisibleState.Hidden;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      await UniTask.CompletedTask;
      visibleState = Enum.VisibleState.Showen;
    }
  }
}