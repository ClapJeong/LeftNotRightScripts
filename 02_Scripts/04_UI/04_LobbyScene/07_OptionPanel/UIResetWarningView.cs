using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace LR.UI.Lobby.Option
{
  public class UIResetWarningView : BaseUIView
  {
    [System.Serializable]
    public class PanelSet
    {
      [field: SerializeField] public RectTransform RectTransform { get; private set; }
      [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
      [field: SerializeField] public UISubmitDirectionSet YesButton { get; private set; }
      [field: SerializeField] public UISubmitDirectionSet NoButton { get; private set; }
    }

    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public PanelSet FirstAskSet { get; private set; }
    [field: SerializeField] public PanelSet LastAskSet { get; private set; }


    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      CanvasGroup.alpha = 0.0f;
      gameObject.SetActive(true);

      await UniTask.CompletedTask;

      visibleState = Enum.VisibleState.Hidden;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;

      var screenHeight = Screen.height;
      FirstAskSet.RectTransform.anchoredPosition = Vector2.zero;
      LastAskSet.RectTransform.anchoredPosition = Vector2.down * screenHeight;

      FirstAskSet.CanvasGroup.alpha = 1.0f;
      LastAskSet.CanvasGroup.alpha = 0.0f;

      CanvasGroup.alpha = 1.0f;

      gameObject.SetActive(true);
      await UniTask.CompletedTask;

      visibleState = Enum.VisibleState.Showen;
    }
  }
}
