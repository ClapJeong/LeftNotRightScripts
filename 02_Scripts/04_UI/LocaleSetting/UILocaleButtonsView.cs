using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace LR.UI.LocaleSet
{
  public class UILocaleButtonsView : BaseUIView
  {
    [System.Serializable]
    public class ButtonSet
    {
      [field: SerializeField] public Locale Locale { get; private set; }
      [field: SerializeField] public UISubmitDirectionSet SubmitDirectionSet { get; private set; }
      [field: SerializeField] public Selectable Selectable { get; private set; }
    }

    [field: SerializeField] public List<ButtonSet> ButtonSets { get; private set; }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }
  }
}
