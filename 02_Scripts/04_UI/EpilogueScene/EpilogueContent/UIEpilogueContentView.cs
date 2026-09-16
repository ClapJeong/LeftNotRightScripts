using Cysharp.Threading.Tasks;
using Febucci.TextAnimatorForUnity;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace LR.UI.EpilogueScene
{
  public class UIEpilogueContentView : BaseUIView
  {
    [field: SerializeField] public List<CanvasGroup> CanvasGroups { get; private set; }
    [field: SerializeField] public LocalizeStringEvent LocalizeStringEvent { get; private set; }
    [field: SerializeField] public TextMeshProUGUI TMP { get; private set; }
    [field: SerializeField] public TypewriterComponent TypeWriter { get; private set; }
    [field: SerializeField] public Image NextInputImage { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      await UniTask.CompletedTask;
      gameObject.SetActive(false);
      visibleState = Enum.VisibleState.Hidden;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      await UniTask.CompletedTask;
      gameObject.SetActive(true);
      visibleState = Enum.VisibleState.Showen;
    }
  }
}
