using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

namespace LR.UI.Speedrun
{
  public class UISpeedRunView : BaseUIView
  {
    [field: SerializeField] public UITimerFontObject Timer { get; private set; }
    [field: SerializeField] public UINumberFontObject RestartCount { get; private set; }

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
