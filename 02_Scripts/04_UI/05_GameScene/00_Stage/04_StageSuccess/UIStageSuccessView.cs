using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using TMPro;
using LR.Manager.GameDataManager;
using LR.UI.GameScene.LocalRecord;
using LR.UI.GameScene.GlobalRecord;

namespace LR.UI.GameScene.Stage
{
  public class UIStageSuccessView : BaseUIView
  {
    [SerializeField] private Vector2 hidePosition;
    [SerializeField] private Vector2 showPosition;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform doctorImageRectTransform;
    [SerializeField] private Vector2 doctorHidePosition;

    [field: SerializeField] public RectTransform IndicatorRoot { get; private set; }
    [field: SerializeField] public Image NormalDoctorImage {  get; private set; }
    [field: SerializeField] public Image PerfectDoctorImage { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public UISubmitDirectionSet NextSubmitDirectionSet { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public UISubmitDirectionSet RestartSubmitDirectionSet { get; private set; }    
    [field: SerializeField] public Selectable RestartSelectable { get; private set; }
    [field: SerializeField] public UIPerfectNoticeView RestartPerfectIcon { get; private set; }

    [field: Space(5)]
    [field: SerializeField] public UISubmitDirectionSet QuitSubmitDirectionSet { get; private set; }
    [field: SerializeField] public Selectable QuitSelectable { get; private set; }

    [field: Space(5)]
    [field: SerializeField] public List<RectTransform> PerfectIcons { get; private set; }

    [field: Space(5)]
    [field: SerializeField] public UIStageLocalRecordView LocalRecordView { get; private set; }
    [field: SerializeField] public UIGlobalRecordView GlobalRecordView { get; private set; }

    [System.Serializable]
    public struct DifficultyStartSet
    {      
      [field: SerializeField] public IDifficultyService.Difficulty Difficulty { get; private set; }
      [field: SerializeField] public GameObject NotYet { get; private set; }
      [field: SerializeField] public Image SuccessImage { get; private set; }
    }
    [field: Space(5)]
    [field: SerializeField] public List<DifficultyStartSet> DifficultyStartSets { get; private set; }
    [System.Serializable]
    public struct LeaderboardSet
    {
      [field: SerializeField] public Image Flag { get; private set; }
      [field: SerializeField] public TextMeshProUGUI Name { get; private set; }
      [field: SerializeField] public TextMeshProUGUI Record { get; private set; }
    }
    [field: Space(5)]
    [field: SerializeField] public GameObject LeaderboardRoot { get; private set; }
    [field: SerializeField] public List<LeaderboardSet> LeaderboardSets { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public GameObject DemoObject { get; private set; }


    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      try
      {
        var duration = isImmediately ? 0.0f : UISO.Stage.UIMoveDefaultDuration;
        await DOTween.Sequence()
          .Join(RectTransform.DOAnchorPos(hidePosition, duration))
          .Join(canvasGroup.DOFade(0.0f, duration))
          .Join(doctorImageRectTransform.DOAnchorPos(doctorHidePosition, duration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);

        visibleState = VisibleState.Hidden;
        gameObject.SetActive(false);
      }
      catch (OperationCanceledException) { }      
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      gameObject.SetActive(true);
      visibleState = VisibleState.Showing;

      try
      {
        var duration = isImmediately ? 0.0f : UISO.Stage.UIMoveDefaultDuration;
        await DOTween.Sequence()
          .Join(RectTransform.DOAnchorPos(showPosition, duration))
          .Join(canvasGroup.DOFade(1.0f, duration))
          .Join(doctorImageRectTransform.DOAnchorPos(Vector2.zero, duration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);

        visibleState = VisibleState.Showen;
      }
      catch (OperationCanceledException) { }      
    }
  }
}