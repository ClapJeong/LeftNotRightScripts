using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using LR.UI.Enum;

namespace LR.UI.GameScene.Dialogue
{
  public class UIDialogueRootView : BaseUIView
  {
    [SerializeField] private CanvasGroup contentCanvasGroup;
    [field: SerializeField] public CanvasGroup DialogueBackgroundCanvasGroup { get; private set; }

    [Header("[ Talking ]")]
    public UIDialogueBackgroundView backgroundView;
    public UITalkingCharacterView leftTalkingCharacterView;
    public UITalkingCharacterView centerTalkingCharacterView;
    public UITalkingCharacterView rightTalkingCharacterView;
    public UITalkingInputsView talkingInputView;

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;
      await backgroundView.ShowAsync(isImmediately);
      DialogueBackgroundCanvasGroup.gameObject.SetActive(true);
      visibleState = VisibleState.Showen;      
    }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      DialogueBackgroundCanvasGroup.gameObject.SetActive(false);
      await backgroundView.HideAsync(isImmediately);      
      visibleState = VisibleState.Hidden;
    }
  }
}
