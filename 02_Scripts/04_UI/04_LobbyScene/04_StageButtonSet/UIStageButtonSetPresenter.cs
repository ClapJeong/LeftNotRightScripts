using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.UI;
using LR.UI.Enum;
using LR.UI.Indicator;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.UI.Lobby
{
  public class UIStageButtonSetPresenter : IUIPresenter
  {
    public class Model
    {      
      [Inject] public DiContainer diContainer;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public ColorSO colorSO;
      [Inject] public UISO uiSO;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IUIPresenterContainer presenterContainer;

      [Inject] public int chapter;
      [Inject] public IUIIndicatorPresenter indicator;
      [Inject] public UnityAction<int> onPreviewStage;
      [Inject] public UnityAction onPreviewStageExit;
      [Inject] public UnityAction onSelectStage;
    }

    private readonly Model model;
    private readonly UIStageButtonSetView view;

    private readonly CTSContainer selectCTS = new();
    private readonly SubscribeHandle subscribeHandle;
    
    private readonly List<UIStageButtonPresenter> stageButtonPresenters = new();
    private readonly bool isEnable;

    private bool isChangeSceneCalled = false;

    public UIStageButtonSetPresenter(Model model, UIStageButtonSetView view)
    {
      this.model = model;
      this.view = view;

      var clearIndex = model.gameDataProvider.GetMaxClearIndex();
      var clearChapter = clearIndex / StageConst.StageUnit;

      UpdateCenterColor();

      view.ChapterTMP.text = model.chapter.ToString();
      isEnable = model.chapter <= clearChapter + 1;
      view.CanvasGroup.alpha = isEnable ? 1.0f : 0.4f;

      if (isEnable)
      {
        subscribeHandle = new(
          () =>
          {
            model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectGameObject);
          },
          () =>
          {
            model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectGameObject);
          });

        for (int i = 0; i < StageConst.StageUnit; i++)
        {
          var stage = i + 1;
          var buttonModel = model.diContainer.Instantiate<UIStageButtonPresenter.Model>(new object[]
          {
          model.chapter,
          stage,
          (UnityAction)OnSelectGame,
          });
          var presenter = new UIStageButtonPresenter(buttonModel, view.StageButtonViews[i]);

          stageButtonPresenters.Add(presenter);
        }
      }
      else
      {
        foreach (var submitView in view.StageButtonViews)
          submitView.gameObject.SetActive(false);
      }
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      selectCTS.Cancel();
      selectCTS.Create();
      var localToken = selectCTS.token;

      if (isEnable)
      {        
        subscribeHandle?.Subscribe();

        model.presenterContainer.GetFirst<UILobbyLogoPresenter>().OnStageSelect();

        var lastSelectedChapter = PlayerPrefs.GetInt(PlayerPrefsName.SelectedChapter);
        if (lastSelectedChapter != model.chapter)
        {
          var targetIndex = lastSelectedChapter < model.chapter ? 6 : 2;
          model.selectedGameObjectService.SetSelectedObject(view.StageButtonViews[targetIndex].gameObject);
        }          
        else
        {
          var lastSelectedStage = PlayerPrefs.GetInt(PlayerPrefsName.SelectedStage, 1);
          model.selectedGameObjectService.SetSelectedObject(view.StageButtonViews[lastSelectedStage - 1].gameObject);
        }

        foreach (var presenter in stageButtonPresenters)
          presenter.ActivateAsync(false, localToken).Forget();        
      }

      PlayerPrefs.SetInt(PlayerPrefsName.SelectedChapter, model.chapter);
      await view.ShowAsync(isImmedieately, localToken);
    }


    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      selectCTS.Cancel();
      selectCTS.Create();
      var localToken = selectCTS.token;

      if (isEnable)
      {
        subscribeHandle?.Unsubscribe();
        model.onPreviewStageExit?.Invoke();

        model.presenterContainer.GetFirst<UILobbyLogoPresenter>().OnStageUnselect();

        foreach (var presenter in stageButtonPresenters)
          presenter.DeactivateAsync(false, localToken).Forget();
      }

      await view.HideAsync(isImmedieately, localToken);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      subscribeHandle?.Dispose();
      selectCTS.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public GameObject GetButtonGameObject()
      => view.Selectable.gameObject;

    private void UpdateCenterColor()
    {
      var minDifficultyIndex = 100;
      for(int i = 0; i < StageConst.StageUnit; i++)
      {
        var stage = i + 1;
        if (!model.gameDataProvider.IsClearStage(model.chapter, stage, out var difficulty))
          return;
        else
        {
          minDifficultyIndex = Mathf.Min(minDifficultyIndex, (int)difficulty);
        }
      }
      view.CenterImage.color = model.colorSO.GetDifficultyClearColor((IDifficultyService.Difficulty)minDifficultyIndex);
    }

    private void OnSelectGameObject(GameObject gameObject)
    {
      var isStageButton = false;
      for(int i = 0; i < view.StageButtonViews.Count; i++)
      {
        if (view.StageButtonViews[i].gameObject == gameObject)
        {
          if (view.StageButtonViews[i].DirectionSet.enabled)
          {
            var stageIndex = (model.chapter - 1) * StageConst.StageUnit + i + 1;
            OnPreview(stageIndex);
            isStageButton = true;
          }          
          break;
        }
      }

      if(!isStageButton)
        model.onPreviewStageExit?.Invoke();
    }

    private void OnPreview(int stageIndex)
    {
      model.onPreviewStage?.Invoke(stageIndex);
    }

    private void OnSelectGame()
    {
      if (isChangeSceneCalled)
        return;

      model.onSelectStage?.Invoke();
      model.indicator.PlayGoodSubmitSFX();
      isChangeSceneCalled = true;
    }
  }
}
