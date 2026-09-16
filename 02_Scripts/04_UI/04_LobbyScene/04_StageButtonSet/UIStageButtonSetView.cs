using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using DG.Tweening;
using LR.UI.Enum;
using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

namespace LR.UI.Lobby
{
  public class UIStageButtonSetView : BaseUIView
  {
    [field: SerializeField] public Image CenterImage { get; private set; }
    [field: SerializeField] public Selectable Selectable { get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public RectTransform IconRectTransform { get; private set; }
    [field: SerializeField] public TextMeshProUGUI ChapterTMP { get; private set; } 
    [field: Header("[ Stage Buttons ]")]
    [field: SerializeField] public List<UIStageButtonView> StageButtonViews { get; private set; }


    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;

      var duration = isImmediately ? 0.0f : UISO.Lobby.StageButton.MoveDuration;
      try
      {
        CanvasGroup.alpha = 1.0f;
        await DOTween.Sequence()
        .Join(IconRectTransform.DOScale(UISO.Lobby.StageButton.HideScale, duration))
        .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
      
      visibleState = VisibleState.Hiding;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;

      var duration = isImmediately ? 0.0f : UISO.Lobby.StageButton.MoveDuration;
      try
      {
        CanvasGroup.alpha = UISO.Lobby.StageButton.ButtonSetUnselectAlpha;

        await DOTween.Sequence()
                .Join(IconRectTransform.DOScale(UISO.Lobby.StageButton.ShowScale, duration))
                .ToUniTask(TweenCancelBehaviour.Kill, token);
      }      
      catch (OperationCanceledException) { }
      visibleState = VisibleState.Hiding;
    }
  }
}
