using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Stage;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.UI.Credit;
using LR.UI.Enum;
using LR.UI.Indicator;
using LR.UI.Lobby.Background;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.UI.Lobby
{
  public class UILobbyRootPresenter : IUIPresenter
  {
    public enum PanelState
    {      
      None = -1,

      Main,
      Stage,
      Dialogue,
      SpeedRun,
      Option, 
      Localize,
      Credit,
    }

    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public IUIIndicatorService indicatorService;
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public Camera stagePreviewCamera;
      [Inject] public UISO uiSO;
      [Inject] public IUIPresenterContainer presetnerContainer;
      [Inject] public IGameModeService gameModeService; 
      [Inject] public IStoreTypeProvider storeTypeProvider;


      [Inject] public FloorSetter floorSetter;
    }

    private readonly Model model;
    private readonly UILobbyRootView view;

    private readonly Dictionary<PanelState, IUIPresenter> panelPresenters = new();
    private readonly BackgroundOffsetController backgroundOffsetController;
    private readonly UILobbyLogoPresenter logoPresenter;
    private UILobbyInputGuidePresenter inputGuidePresenter;
    private Sequence dialogueShakeSequence;

    private PanelState currentPanelState = PanelState.None;
    private IUIIndicatorPresenter currentIndicator;
    private bool isFirstSound = true;

    public UILobbyRootPresenter(Model model, UILobbyRootView view)
    {
      PlayerPrefs.SetInt(PlayerPrefsName.LobbyState, 0);

      this.model = model;
      this.view = view;

      var storeType = model.storeTypeProvider.StoreType;
      view.steamIcon.SetActive(storeType == StoreType.Steam);
      view.stoveIcon.SetActive(storeType == StoreType.Stove);

      RegisterContainer();

      var logoModel = model.diContainer.Instantiate<UILobbyLogoPresenter.Model>();
      logoPresenter = new(logoModel, view.LogoView);
      logoPresenter.AttachOnDestroy(view.gameObject);

      backgroundOffsetController = model.diContainer.Instantiate<BackgroundOffsetController>(new object[] { view.BackgroundImage });
      backgroundOffsetController.StartOffsetUpdate(Direction.Up);

      model.presetnerContainer.Add(this);
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await view.HideAsync(isImmediately, token);
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await CreateIndicatorPresenterAsync();

      CreateInputGuidePresenter();
      CreateMainPanelPresenter();
      CreateChapterPanelPresenter();
      CreateDialogueReplayPanelPresenter();
      CreateSpeedRunPanelPresenter();
      CreateOptionPanelPresenter();
      CreateLocalizePanelPresenter();

      await view.ShowAsync(isImmediately, token);
      PlayerPrefs.SetInt(PlayerPrefsName.LobbyState, (int)PanelState.Main);
      SetState(PanelState.Main);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      dialogueShakeSequence?.Kill(true);
      model.presetnerContainer.Remove(this);
      backgroundOffsetController.Dispose();
      UnregisterContainer();      

      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void ShakeForDialogue()
    {
      dialogueShakeSequence =
        DOTween
        .Sequence()
        .Join(view.RectTransform.DOShakeAnchorPos(
          model.uiSO.Lobby.DialogueShakeDuration,
          model.uiSO.Lobby.DialogueShakeStrength,
          model.uiSO.Lobby.DialogueShakeVibrato,
          model.uiSO.Lobby.DialogueShakeRandom))
        .SetLoops(-1)
        .OnKill(() =>
        {
          view.RectTransform.anchoredPosition = Vector3.zero;
        })
        .Play();
    }

    public void StopDialogueShake()
    {
      dialogueShakeSequence?.Kill(true);
    }

    private void SetStateByIndex(int index)
      => SetState((PanelState)index, false);

    private void SetState(PanelState panelState, bool isImmedieately = false)
    {
      if (panelState == currentPanelState)
        return;

      var storeType = model.storeTypeProvider.StoreType;
      var demoLink = view.GetDemoLink(storeType);

      if (panelState == PanelState.Credit)
      {
        var isDemo = model.gameModeService.GetCurrentGameMode() == IGameModeService.GameMode.Demo;
        if (isDemo && !string.IsNullOrEmpty(demoLink))
        {
          Application.OpenURL(demoLink);
          return;
        }
      }

        if (!isFirstSound)
      {
        if (panelState == PanelState.Main)
          currentIndicator.PlayBadSubmitSFX();
        else
          currentIndicator.PlayGoodSubmitSFX();
      }

      if(panelState != PanelState.None && panelState != PanelState.Main)
        PlayerPrefs.SetInt(PlayerPrefsName.LobbyState, (int)panelState);

      isFirstSound = false;

      if (panelPresenters.TryGetValue(currentPanelState, out var prevPresenter))
        prevPresenter.DeactivateAsync(isImmedieately).Forget();

      if (panelState == PanelState.Credit)
      {
        var isDemo = model.gameModeService.GetCurrentGameMode() == IGameModeService.GameMode.Demo;
        if (isDemo && !string.IsNullOrEmpty(demoLink))
        {
          Application.OpenURL(demoLink);
        }
        else
        {
          PlayCreditAsync().Forget();
        }
      }        

      logoPresenter.OnPanelState(panelState);

      currentPanelState = panelState;

      if (panelPresenters.TryGetValue(currentPanelState, out var targetPresenter))
        targetPresenter.ActivateAsync(isImmedieately).Forget();      
    }

    private async UniTask PlayCreditAsync()
    {
      IUIPresenter creditPresenter = null;
      creditPresenter = await UICreditPresenter.CreateAsync(
        model.diContainer,
        onExit: () =>
      {
        creditPresenter.DeactivateAsync().Forget();
        SetState(PanelState.Main);
      });
      creditPresenter.ActivateAsync().Forget();
    }

    private async UniTask CreateIndicatorPresenterAsync()
    {
      currentIndicator = await model.indicatorService.GetNewAsync(view.IndicatorRoot, view.MainPanelView.StageButton.RectTransform);
      model.indicatorService.ReleaseTopIndicatorOnDestroy(view.gameObject);
    }

    private void CreateInputGuidePresenter()
    {
      var model = this.model.diContainer.Instantiate<UILobbyInputGuidePresenter.Model>(new object[] { currentIndicator });
      var view = this.view.InputGuideView;
      inputGuidePresenter = new UILobbyInputGuidePresenter(model, view);
      inputGuidePresenter.AttachOnDestroy(this.view.gameObject);
      inputGuidePresenter.ActivateAsync(true).Forget();
    }

    private void CreateMainPanelPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIMainPanelPresenter.Model>(new object[] { currentIndicator, (UnityAction<int>)SetStateByIndex });
      var mainPanelPresenter = new UIMainPanelPresenter(model, view.MainPanelView);
      mainPanelPresenter.AttachOnDestroy(view.gameObject);
      panelPresenters[PanelState.Main] = mainPanelPresenter;
    }

    private void CreateChapterPanelPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIChapterPanelPresenter.Model>(new object[] {
        this.model.floorSetter,
        currentIndicator, 
        (UnityAction)(() => SetState(PanelState.Main)), 
        this.model.stagePreviewCamera });
      var chapterPanelPresenter = new UIChapterPanelPresenter(model, view.ChapterPanelView);
      chapterPanelPresenter.AttachOnDestroy(view.gameObject);
      chapterPanelPresenter.DeactivateAsync(true).Forget();
      panelPresenters[PanelState.Stage] = chapterPanelPresenter;
    }

    private void CreateDialogueReplayPanelPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIDialogueReplayPanelPresenter.Model>(new object[]
      {
        currentIndicator,
        (UnityAction)(() => SetState(PanelState.Main))
      });
      var dialogueReplayPresenter = new UIDialogueReplayPanelPresenter(model, view.DialogueReplayView);
      dialogueReplayPresenter.AttachOnDestroy(view.gameObject);
      dialogueReplayPresenter.DeactivateAsync(true).Forget();
      panelPresenters[PanelState.Dialogue] = dialogueReplayPresenter;
    }

    private void CreateSpeedRunPanelPresenter()
    {
      var model = this.model.diContainer.Instantiate<UISpeedRunPanelPresenter.Model>(new object[]
      {
        currentIndicator,
        (UnityAction)(() => SetState(PanelState.Main))
      });
      var speedRunPresenter = new UISpeedRunPanelPresenter(model, view.SpeedRunPanelView);
      speedRunPresenter.AttachOnDestroy(view.gameObject);
      speedRunPresenter.DeactivateAsync(true).Forget();
      panelPresenters[PanelState.SpeedRun] = speedRunPresenter;
    }

    private void CreateOptionPanelPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIOptionPanelPresenter.Model>(new object[]
      {
        currentIndicator,
        (UnityAction)(() => SetState(PanelState.Main)),
      });
      var optionPanelPresenter = new UIOptionPanelPresenter(model, view.OptionPanelView);
      optionPanelPresenter.AttachOnDestroy(view.gameObject);
      optionPanelPresenter.DeactivateAsync(true).Forget();
      panelPresenters[PanelState.Option] = optionPanelPresenter;
    }

    private void CreateLocalizePanelPresenter()
    {
      var model = this.model.diContainer.Instantiate<UILocalizePanelPresenter.Model>(new object[]
      {
        currentIndicator,
        (UnityAction)(() => SetState(PanelState.Main)),
      });
      var localizePresenter = new UILocalizePanelPresenter(model, view.LocalizePanelView);
      localizePresenter.AttachOnDestroy(view.gameObject);
      localizePresenter.DeactivateAsync(true).Forget();
      panelPresenters[PanelState.Localize] = localizePresenter;
    }

    private void RegisterContainer()
    {
      model
        .presenterContainer
        .Add(this);
    }

    private void UnregisterContainer()
    {
      model
        .presenterContainer
        .Remove(this);
    }
  }
}