using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.UI.DifficultySettting;
using LR.UI.Enum;
using LR.UI.Indicator;
using LR.UI.Lobby.Option;
using LR.UI.VolumeControl;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.Lobby
{
  public class UIOptionPanelPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public IUIIndicatorPresenter indicator;
      [Inject] public IUIDepthService depthService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public UnityAction onExit;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IAchievementRegister achievementRegister;
    }

    private readonly Model model;
    private readonly UIOptionPanelView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly UIVolumeControlPresenter volumeControlPresenter;
    private readonly UIDifficultyPresenter difficultyPresenter;
    private readonly UIResetWarningPresenter resetWarningPresenter;
    private bool isFirstIndiactorMove = true;

    public UIOptionPanelPresenter(Model model, UIOptionPanelView view)
    {
      this.model = model;
      this.view = view;

      var volumeControlModel = model.diContainer.Instantiate<UIVolumeControlPresenter.Model>();
      volumeControlPresenter = new(volumeControlModel, view.VolumeControlView);
      volumeControlPresenter.AttachOnDestroy(view.gameObject);
      view.ExitSubmitDirectionSet.Subscribe(model.onExit);

      var difficultyModel = model.diContainer.Instantiate<UIDifficultyPresenter.Model>(new object[]
      {
        (UnityAction<UIDifficultyView.DifficultyButtonSet>)OnDifficultySelect,
      });
      difficultyPresenter = new(difficultyModel, view.DifficultyView);
      difficultyPresenter.AttachOnDestroy(view.gameObject);

      var dataResetWarningModel = model.diContainer.Instantiate<UIResetWarningPresenter.Model>(new object[]
      {
        model.indicator
      });
      resetWarningPresenter = new(dataResetWarningModel, view.ResetWarningView);
      resetWarningPresenter.AttachOnDestroy(view.gameObject);
      resetWarningPresenter.DeactivateAsync(true).Forget();

      var currentDifficultySet = view.DifficultyView.DifficultyButtonSets.FirstOrDefault(set => set.Difficulty == model.difficultyService.CurrentDifficulty);
      OnDifficultySelect(currentDifficultySet);

      var isAllClear = model.gameDataProvider.IsAllClear();
      view.AllAchievementText.SetEntry(isAllClear ? "ui_AllAchievementDescription_enable" : "ui_AllAchievementDescription_disable");
      view.AllAchievementGuideCanvasGroup.alpha = 0.0f;
      view.AllAchievementButtonSet.CanvasGroup.alpha = isAllClear ? 1.0f : 0.4f;

      subscribeHandle = new(
        () =>
        {
          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObject);
          volumeControlPresenter.ActivateAsync().Forget();
          view.DataResetButton.Subscribe(OnDataResetButtonSelect);
          model.depthService.RaiseDepth(view.VolumeControlView.MasterVolumeSet.gameObject);

          if (isAllClear)
            view.AllAchievementButtonSet.Subscribe(OnAllAchievement);
        },
        () =>
        {
          volumeControlPresenter.DeactivateAsync().Forget();
          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObject);
          view.DataResetButton.Unsubscribe(OnDataResetButtonSelect);

          model.depthService.LowerDepth();

          view.AllAchievementButtonSet.Unsubscribe(OnAllAchievement);
        });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      isFirstIndiactorMove = true;
      await UniTask.WhenAll(
        difficultyPresenter.ActivateAsync(isImmedieately, token),
        view.ShowAsync(isImmedieately, token));
      subscribeHandle.Subscribe();      
      model.depthService.SelectTopObject();      
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await UniTask.WhenAll(
        difficultyPresenter.DeactivateAsync(isImmedieately, token),
        view.HideAsync(isImmedieately, token));
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnDifficultySelect(UIDifficultyView.DifficultyButtonSet set)
    {
      view
        .VolumeControlView
        .MasterVolumeSet
        .Selectable
        .AddNavigation(Direction.Up, set.Selectable);
    }

    private void OnDataResetButtonSelect()
    {
      resetWarningPresenter.ActivateAsync().Forget();
    }

    private void OnSelectedGameObject(GameObject gameObject)
    {
      model.indicator.MoveAsync(gameObject, isFirstIndiactorMove).Forget();

      if (gameObject.TryGetComponent<Selectable>(out var selectable))
        model.indicator.SetLeftInputGuide(selectable.navigation);

      var isAllAchievementSelected = gameObject == view.AllAchievementButtonSet.gameObject;
      view.AllAchievementGuideCanvasGroup.alpha = isAllAchievementSelected ? 1.0f : 0.0f;
    }

    private void OnAllAchievement()
    {
      for(int i = 0; i < 12; i++)
      {
        var chapter = i + 1;
        model.achievementRegister.SetAchievement(string.Format(StoreKeys.StageClearFormat, chapter));        
      }

      var allStageCount = model.gameDataProvider.StageDataCount;
      model.achievementRegister.SetData(StoreKeys.HardModeClearCount, allStageCount);

      model.achievementRegister.SetData(StoreKeys.NormalPerfect, allStageCount);
      model.achievementRegister.SetData(StoreKeys.HardPerfect, allStageCount);

      model.achievementRegister.SetAchievement(StoreKeys.SpeedRun);
    }
  }
}
