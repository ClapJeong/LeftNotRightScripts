using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using LR.UI.Enum;
using System;
using System.Collections.Generic;

namespace LR.UI.Lobby
{
  public class UIMainPanelView : BaseUIView
  {
    private const int SubmitViewCount = 6;

    [field: SerializeField] public BaseSubmitView StageButton { get; private set; }
    [field: SerializeField] public BaseSubmitView DialogueButton { get; private set; }
    [field: SerializeField] public BaseSubmitView SpeedRunButton { get; private set; }    
    [field: SerializeField] public BaseSubmitView OptionButton { get; private set; }
    [field: SerializeField] public BaseSubmitView LocalizeButton { get; private set; }
    [field: SerializeField] public BaseSubmitView CreditButton { get; private set; }
    [field: SerializeField] public GameObject CreditTMP { get; private set; }
    [field: SerializeField] public GameObject StoreIcon { get; private set; }

    [field: SerializeField] public BaseSubmitView QuitButton { get; private set; }
    [field: SerializeField] public Selectable QuitButtonSelectable { get; private set; }

    [field: Header("[ Positions ]")]
    [field: SerializeField] public Vector2 IdlePosition {  get; private set; }
    [field: SerializeField] public Vector2 HidePosition { get; private set; }

    [Space(5)]
    [SerializeField] private CanvasGroup canvasGroup;

    private float submitViewOriginY;
    private float quitViewOriginY;

    public List<BaseSubmitView> PanelButtons
      => new()
      {
        StageButton,
        DialogueButton,
        SpeedRunButton,
        OptionButton,
        LocalizeButton,
        CreditButton,
      };

    private void Awake()
    {
      submitViewOriginY = StageButton.RectTransform.anchoredPosition.y;
      quitViewOriginY = QuitButton.RectTransform.anchoredPosition.y;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;

      var selectedSubmitIndex = PlayerPrefs.GetInt(PlayerPrefsName.LobbyState);
      selectedSubmitIndex = Mathf.Clamp(selectedSubmitIndex, 1, SubmitViewCount);
      var delayCount = selectedSubmitIndex > (SubmitViewCount / 2) ? selectedSubmitIndex
                                                                  : SubmitViewCount - selectedSubmitIndex + 1;
      var showDuration = isImmediately ? 0.0f : UISO.Lobby.PanelShowDuration;
      var targetY = submitViewOriginY;
      var totalDuration = 0.0f;
      for (int i = 0; i < delayCount; i++)
      {
        var index = selectedSubmitIndex;
        var delay = isImmediately ? 0.0f : UISO.Lobby.MainPanelSubmitMoveDelay * i;
        totalDuration = Mathf.Max(totalDuration, delay + showDuration);
        if (i == 0)
        {          
          HideSubmitView(index, delay);
        }
        else
        {
          var upperIndex = index + i;
          HideSubmitView(upperIndex, delay);

          var lowerIndex = index - i;
          HideSubmitView(lowerIndex, delay);
        }
      }
      DOTween
        .Sequence()
        .Join(QuitButton.RectTransform.DOAnchorPosY(quitViewOriginY, showDuration))
        .ToUniTask(TweenCancelBehaviour.Complete, token).Forget();

      try
      {
        await UniTask.WaitForSeconds(totalDuration, false, PlayerLoopTiming.Update, token);
        visibleState = VisibleState.Showen;
      }
      catch (OperationCanceledException) { }

      void HideSubmitView(int index, float delay)
      {
        if (TryGetSubmitView(index, out var upperView))
          DOTween
            .Sequence()
            .AppendInterval(delay)
            .Append(upperView.RectTransform.DOAnchorPosY(targetY, showDuration))
            .ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
      }
    }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      var selectedSubmitIndex = PlayerPrefs.GetInt(PlayerPrefsName.LobbyState);
      selectedSubmitIndex = Mathf.Clamp(selectedSubmitIndex, 1, SubmitViewCount);
      var delayCount = selectedSubmitIndex > (SubmitViewCount / 2) ? selectedSubmitIndex
                                                                  : SubmitViewCount - selectedSubmitIndex + 1;
      var hideDuration = isImmediately ? 0.0f : UISO.Lobby.PanelHideDuration;
      var targetY = submitViewOriginY + HidePosition.y;
      var totalDuration = 0.0f;
      for (int i = 0; i < delayCount; i++)
      {
        var index = selectedSubmitIndex;
        var delay = isImmediately ? 0.0f : UISO.Lobby.MainPanelSubmitMoveDelay * (delayCount - i - 1);
        
        if (i == 0)
        {
          totalDuration = delay + hideDuration;
          HideSubmitView(index, delay);
        }
        else
        {
          var upperIndex = index + i;
          HideSubmitView(upperIndex, delay);

          var lowerIndex = index - i;
          HideSubmitView(lowerIndex, delay);
        }
      }
      DOTween
        .Sequence()
        .Join(QuitButton.RectTransform.DOAnchorPosY(quitViewOriginY + HidePosition.y, hideDuration))
        .ToUniTask(TweenCancelBehaviour.Complete, token).Forget();

      try
      {
        await UniTask.WaitForSeconds(totalDuration, false, PlayerLoopTiming.Update, token);
        visibleState = VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }

        void HideSubmitView(int index, float delay)
      {
        if (TryGetSubmitView(index, out var upperView))
          DOTween
            .Sequence()
            .AppendInterval(delay)
            .Append(upperView.RectTransform.DOAnchorPosY(targetY, hideDuration))
            .ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
      }
    }

    public BaseSubmitView GetSubmitView(int index)
      => index switch
      {
        1 => StageButton,
        2 => DialogueButton,
        3 => SpeedRunButton,
        4 => OptionButton,
        5 => LocalizeButton,
        6 => CreditButton,
        _ => throw new System.NotImplementedException(),
      };

    private bool TryGetSubmitView(int index, out BaseSubmitView view)
    {
      if (index < 1 || index > SubmitViewCount)
      {
        view = null;
        return false;
      }
      else
      {
        view = GetSubmitView(index);
        return true;
      }
    }

  }
}
