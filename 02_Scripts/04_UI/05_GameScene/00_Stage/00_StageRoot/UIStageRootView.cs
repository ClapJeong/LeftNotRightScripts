using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using LR.UI.Enum;
using TMPro;
using LR.UI.GameScene.Speedrun;
using UnityEngine.Localization.Components;

namespace LR.UI.GameScene.Stage
{
  public class UIStageRootView : BaseUIView
  {
    [SerializeField] private CanvasGroup canvasGroup;
    [field: SerializeField] public UIStageBeginView BeginView { get; private set; }
    [field: SerializeField] public UIStageFailView FailView { get; private set; }
    [field: SerializeField] public UIStageSuccessView SuccessView { get; private set; }
    [field: SerializeField] public UIStagePauseView PauseView { get; private set; }
    [field: SerializeField] public UIStageRestartView RestartView { get; private set; }
    [field: SerializeField] public UIPracticeView PracticeView { get; private set; }
    [field: SerializeField] public UISpeedrunCompleteView SpeedrunCompleteView { get; private set; }
    [field: SerializeField] public UIPerfectNoticeView PerfectNoticeView { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public TextMeshProUGUI StageInfoTMP {  get; private set; }
    [field: SerializeField] public TextMeshProUGUI DifficultyTMP { get; private set; }
    [field: SerializeField] public LocalizeStringEvent RestartText { get; private set; }
    [field: SerializeField] public LocalizeStringEvent FailCountText { get; private set; }
    [field: SerializeField] public LocalizeStringEvent BonusTimeText { get; private set; }
    [field: SerializeField] public GameObject DemoObject { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hidden;
      canvasGroup.alpha = 0.0f;
      await UniTask.CompletedTask;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showen;
      canvasGroup.alpha = 1.0f;
      await UniTask.CompletedTask;
    }
  }
}