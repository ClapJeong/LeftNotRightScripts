using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.LocalRecord
{
  public class UIStageLocalRecordView : BaseUIView
  {
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public UINumberFontObject LeftHitFont { get; private set; }
    [field: SerializeField] public Animator LeftAnimator { get; private set; }
    [field: SerializeField] public UINumberFontObject RunningFont { get; private set; }
    [field: SerializeField] public UINumberFontObject RightHitFont { get; private set; }
    [field: SerializeField] public Animator RightAnimator { get; private set; }

    [field: SerializeField] public UINumberFontObject ResultFont { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      CanvasGroup.alpha = 0.0f;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      CanvasGroup.alpha = 1.0f;
    }
  }
}