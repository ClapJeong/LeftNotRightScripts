using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.Stage
{
  public class UIPerfectNoticeView : BaseUIView
  {
    [field: SerializeField] public Image OutlineImage { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public Animator IconAnimator { get; private set; }
    [field: SerializeField] public CanvasGroup IconCanvasGroup { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public RectTransform DisableTextRect { get; private set; }
    [field: SerializeField] public CanvasGroup DisableTextCanvasGroup { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      DisableTextCanvasGroup.alpha = 0.0f;
      IconCanvasGroup.alpha = 0.0f;
      gameObject.SetActive(false);
      await UniTask.CompletedTask;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      IconAnimator.Play(AnimatorHash.PerfectNotice.Idle, 0, 0.0f);
      DisableTextCanvasGroup.alpha = 0.0f;
      IconCanvasGroup.alpha = 1.0f;
      DisableTextRect.anchoredPosition = Vector2.zero;
      visibleState = Enum.VisibleState.Showen;
      gameObject.SetActive(true);
      await UniTask.CompletedTask;
    }
  }
}