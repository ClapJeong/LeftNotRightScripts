using Cysharp.Threading.Tasks;
using LR.UI.DifficultySettting;
using LR.UI.Enum;
using LR.UI.VolumeControl;
using System.Threading;
using UnityEngine;

namespace LR.UI.GameScene.Stage
{
  public class UIStagePauseView : BaseUIView
  {
    [SerializeField] private Vector2 hidePosition;
    [SerializeField] private Vector2 showPosition;
    [SerializeField] private CanvasGroup canvasGroup;
    [field: SerializeField] public RectTransform ContentRectTransform { get; private set; }
    [field: SerializeField] public RectTransform IndicatorRoot { get; private set; }
    [field: SerializeField] public UIVolumeControlView VolumeControlView { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet ResumeSubmitDirectionSet {  get; private set; }
    [field: SerializeField] public UISubmitDirectionSet QuitSubmitDirectionSet { get; private set; }
    [field: SerializeField] public UIRedPillButtonView RedPillButtonView { get; private set; }
    [field: SerializeField] public UIDifficultyView DifficultyView { get; private set; }
    [field: SerializeField] public CanvasGroup SpeedRunQuitGuideCanvasGroup { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {      
      visibleState = VisibleState.Hiding;

      ContentRectTransform.anchoredPosition = hidePosition;
      canvasGroup.alpha = 0.0f;      

      visibleState = VisibleState.Hidden;
      gameObject.SetActive(false);
      await UniTask.CompletedTask;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      ContentRectTransform.anchoredPosition = showPosition;
      canvasGroup.alpha = 1.0f;
      gameObject.SetActive(true);
      visibleState = VisibleState.Showing;
      await UniTask.CompletedTask;
    }
  }
}