using Cysharp.Threading.Tasks;
using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using LR.UI.Enum;
using System;
using System.Collections.Generic;

namespace LR.UI.GameScene.Dialogue
{
  public class UITalkingCharacterView : BaseUIView
  {
    [System.Serializable]
    public class ImageSet
    {
      [field: SerializeField] public Image Image { get; private set; }
      [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
      [field: SerializeField] public UIEasyHatView HatView { get; private set; }
    }
    [field: Header("[ Portrait ]")]
    [field: SerializeField] public Animator PortraitAnimator { get; private set; }
    [field: SerializeField] public ImageSet SetA { get; private set; }
    [field: SerializeField] public ImageSet SetB { get; private set; }
    [field: SerializeField] public Animator EmotionAnimator {  get; private set; }
    [field: Header("[ Dialogue ]")]
    [field: SerializeField] public RectTransform BoxRectTransform { get; private set; }
    [field: SerializeField] public CanvasGroup DialogueBackground {  get; private set; }
    [field: SerializeField] public CanvasGroup NameCanvasgGroup { get; private set; }
    [field: SerializeField] public TextMeshProUGUI DialogueTMP { get; private set; }
    [field: SerializeField] public LocalizeStringEvent DialogueLocalize { get; private set; }
    [field: SerializeField] public TextAnimator_TMP AnimatorTMP { get; private set;  }
    [field: SerializeField] public TypewriterComponent Typewriter { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      try
      {
        gameObject.SetActive(false);
        visibleState = VisibleState.Hidden;
        await UniTask.CompletedTask;
      }
      catch(OperationCanceledException) { }
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      gameObject.SetActive(true);
      visibleState = VisibleState.Showing;
      try
      {
        gameObject.SetActive(true);
        visibleState = VisibleState.Showen;
        await UniTask.CompletedTask;
      }
      catch (OperationCanceledException) { }
    }
  }
}
