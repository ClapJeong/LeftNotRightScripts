using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;
using LR.UI.Enum;

namespace LR.UI.Lobby
{
  public class UIChapterPanelExitButtonView : BaseUIView
  {
    [field: SerializeField] public Selectable Selectable { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet SubmitDirectionSet { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      await UniTask.CompletedTask;
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
