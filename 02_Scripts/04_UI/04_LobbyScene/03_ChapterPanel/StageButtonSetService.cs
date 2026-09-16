using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.Indicator;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.Lobby.ChapterPanel
{
  public class StageButtonSetService : IDisposable
  {
    [Inject] private readonly UISO uiSO = null;
    [Inject] private readonly RectTransform rootRectTransform = null;
    [Inject] private readonly RectTransform centerPosition = null;
    [Inject] private readonly IUIIndicatorPresenter indicator = null;
    [Inject] private readonly HorizontalLayoutGroup layoutGroup = null;
    [Inject] private readonly UIChapterPanelExitButtonView exitView = null;

    private readonly CTSContainer stageButtonSetMoveCTS = new();
    private readonly List<UIStageButtonSetPresenter> stageButtonSetPresenters = new();
    private readonly List<int> stageButtonViewIDMap = new();
    private UIStageButtonSetPresenter selectedStageButtonSetPresenter;    

    private float buttonSetViewWidth;
    private bool isActivated = false;


    public void AddMap(UIStageButtonSetPresenter presenter, UIStageButtonSetView view)
    {
      if (stageButtonViewIDMap.Count == 0)
        buttonSetViewWidth = view.RectTransform.rect.width;

      stageButtonSetPresenters.Add(presenter);
      stageButtonViewIDMap.Add(view.Selectable.gameObject.GetInstanceID());      
    }

    public async UniTask DeactivateAsync(bool isImmediately, CancellationToken token)
    {
      stageButtonSetMoveCTS.Cancel();
      if (selectedStageButtonSetPresenter != null)
        await selectedStageButtonSetPresenter.DeactivateAsync(isImmediately, token);
      isActivated = false;
    }

    public void Dispose()
    {
      stageButtonSetMoveCTS.Dispose();
    }

    public void OnSelectStageButtonSet(GameObject gameObject)
    {      
      if (gameObject.TryGetComponent<Selectable>(out var selectable))
      {        
        var id = selectable.gameObject.GetInstanceID();
        if (stageButtonViewIDMap.Contains(id))
        {
          selectedStageButtonSetPresenter?.DeactivateAsync().Forget();

          var targetIndex = stageButtonViewIDMap.IndexOf(id);
          selectedStageButtonSetPresenter = stageButtonSetPresenters[targetIndex];
          selectedStageButtonSetPresenter.ActivateAsync().Forget();

          exitView.Selectable.AddNavigation(Direction.Up, selectable);

          indicator.SetLeftInputGuide(selectable.navigation);

          stageButtonSetMoveCTS.Cancel();
          stageButtonSetMoveCTS.Create();
          MoveStageButtonSetViewsAsync(targetIndex, stageButtonSetMoveCTS.token, isImmedieatly: !isActivated).Forget();

          isActivated = true;
        }
      }
    }

    public void InitializeFirstPosition()
    {
      MoveStageButtonSetViewsAsync(PlayerPrefs.GetInt(PlayerPrefsName.SelectedChapter, 1) - 1, default, true).Forget();
    }

    public GameObject GetFirstSelectButton()
    {      
      var lastSeledtedChapter = Mathf.Max(1, PlayerPrefs.GetInt(PlayerPrefsName.SelectedChapter, 1));
      lastSeledtedChapter = Mathf.Clamp(lastSeledtedChapter, 1, stageButtonSetPresenters.Count);
      return stageButtonSetPresenters[lastSeledtedChapter - 1].GetButtonGameObject();
    }

    private async UniTask MoveStageButtonSetViewsAsync(int targetIndex, CancellationToken token, bool isImmedieatly = false)
    {
      var interval = layoutGroup.spacing;
      var targetLength = buttonSetViewWidth * 0.5f + (buttonSetViewWidth + interval) * targetIndex;

      var duration = isImmedieatly ? 0.0f : uiSO.Lobby.StageButton.MoveDuration;
      await rootRectTransform
        .DOAnchorPos(centerPosition.anchoredPosition + (-targetLength) * Vector2.right, duration)
        .ToUniTask(TweenCancelBehaviour.Complete, token);
    }
  }
}
