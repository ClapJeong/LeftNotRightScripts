using Cysharp.Threading.Tasks;
using LR.UI.Indicator;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using LR.UI.Enum;
using LR.Manager.UI;
using LR.UI.Loading;
using Zenject;
using DG.Tweening;
using LR.Manager.GameDataManager;

namespace LR.UI.Lobby
{
  public class UIMainPanelPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public IGameModeService gameModeService;
      [Inject] public ICanvasProvider canvasProvider;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public UISO uiSO;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IUIDepthService depthService;
      [Inject] public IUIIndicatorPresenter indicator;
      [Inject] public UnityAction<int> onSubmit;
    }

    private readonly Model model;
    private readonly UIMainPanelView view;

    private readonly CTSContainer showHideCTS = new();
    private readonly SubscribeHandle subscribeHandle;
    private BaseSubmitView selectedSubmitView;

    public UIMainPanelPresenter(Model model, UIMainPanelView view)
    {
      this.model = model;
      this.view = view;

      var isDemo = model.gameModeService.GetCurrentGameMode() == IGameModeService.GameMode.Demo;
      view.CreditTMP.SetActive(!isDemo);
      view.StoreIcon.SetActive(isDemo);

      var buttons = view.PanelButtons;
      for (int i = 0; i < buttons.Count; i++)
      {
        var submitView = buttons[i];
        var index = i + 1;
        submitView.Subscribe(() => model.onSubmit?.Invoke(index));
        submitView.RectTransform.localScale = Vector3.one * model.uiSO.Lobby.MainPanelButtonHideScale;
      }

      view.QuitButton.Subscribe(OnOptionQuitSubmit);

      if(model.gameModeService.GetCurrentGameMode() == IGameModeService.GameMode.Demo)
      {
        view.SpeedRunButton.Enable(false);
        view.SpeedRunButton.CanvasGroup.alpha = 0.4f;
      }

      subscribeHandle = new(
        () =>
        {
          SubscribeSelectedGameObjectService();
          SubscribeCurrentDepth();
        },
        () =>
        {
          UnsubscribeSelectedGameObjectService();
          UnsubscribeCurrentDepth();
        });

      view.ShowAsync(true).Forget();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      showHideCTS.Cancel();
      showHideCTS.Create();

      subscribeHandle.Subscribe();
      model.depthService.SelectTopObject();
      var firstButtonIndex = PlayerPrefs.GetInt(PlayerPrefsName.LobbyState);
      firstButtonIndex = Mathf.Max(firstButtonIndex, 1);
      model.selectedGameObjectService.SetSelectedObject(view.GetSubmitView(firstButtonIndex).RectTransform.gameObject);

      var myToken = showHideCTS.token;
      await view.ShowAsync(isImmedieately, myToken);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      showHideCTS.Cancel();
      showHideCTS.Create();

      subscribeHandle.Unsubscribe();
      var myToken = showHideCTS.token;
      await view.HideAsync(isImmedieately, myToken);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      showHideCTS.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();


    private void OnOptionQuitSubmit()
    {
      showHideCTS.Dispose();
      subscribeHandle.Dispose();

      QuitAsync().Forget();
    }

    private async UniTask QuitAsync()
    {
      var loadingPresenter = await UILoadingPresenter.CreateAsync(
        model.diContainer,
        model.addressableKeySO,
        model.canvasProvider,
        model.resourceManager);

      await loadingPresenter.ActivateAsync();

#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#endif
      Application.Quit();
    }

    private void SubscribeSelectedGameObjectService()
    {
      model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, UpdateIndicatorGuide);
      model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, SetIndicatorTarget);
    }

    private void UnsubscribeSelectedGameObjectService()
    {
      model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, UpdateIndicatorGuide);
      model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, SetIndicatorTarget);
    }

    private void SubscribeCurrentDepth()
    {
      model.depthService.RaiseDepth(view.StageButton.RectTransform.gameObject);
    }

    private void UnsubscribeCurrentDepth() 
    {
      model.depthService.LowerDepth();
    }

    private void SetIndicatorTarget(GameObject gameObject)
    {
      if (selectedSubmitView != null)
        selectedSubmitView.RectTransform.DOScale(model.uiSO.Lobby.MainPanelButtonHideScale, model.uiSO.Lobby.MainPanelButtonScaleDuration);
      selectedSubmitView = null;

      if (gameObject != null)
      {
        model.indicator.MoveAsync(gameObject);
        if (gameObject.TryGetComponent<BaseSubmitView>(out var submitView))
        {
          selectedSubmitView = submitView;
          selectedSubmitView.RectTransform.DOScale(1.0f, model.uiSO.Lobby.MainPanelButtonScaleDuration);
        }
      }        
    }

    private void UpdateIndicatorGuide(GameObject gameObject)
    {
      if(gameObject.TryGetComponent<Selectable>(out var selectable))
      {
        model.indicator.SetLeftInputGuide(selectable.navigation);
        if(gameObject != view.QuitButton.gameObject)
          view.QuitButtonSelectable.AddNavigation(Direction.Up, selectable);
      }      

      var selectedRectTransform = gameObject.GetComponent<RectTransform>();
    }
  }
}