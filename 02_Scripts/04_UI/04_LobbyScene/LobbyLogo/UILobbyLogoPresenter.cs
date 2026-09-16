using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.UI;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using ModestTree;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.UI.Lobby
{
  public class UILobbyLogoPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public UISO uiSO;
      [Inject] public UIPresenterContainer presenterContainer;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public IGameModeService gameModeService;
    }

    private readonly Model model;
    private readonly UILobbyLogoView view;

    private readonly CTSContainer panelCTS = new();
    private readonly CTSContainer stageScaleCTS = new();

    private readonly UILobbyPortraitPresenter leftPresenter;
    private readonly UILobbyPortraitPresenter rightPresenter;
    private readonly UILobbyDoctorPresenter doctorPresenter;

    private readonly SubscribeHandle subscribeHandle;
    private readonly UnityEvent<CancellationToken> onPanelStateExit = new();
    private readonly Vector2 leftInitializedPos;
    private readonly Vector2 rightInitializedPos;
    private readonly Vector2 doctorInitializedPos;
    private readonly Vector2 logoInitializedPos;

    private int stageScaleFrameCount;

    public UILobbyLogoPresenter(Model model, UILobbyLogoView view)
    {
      this.model = model;
      this.view = view;

      var isDemo = model.gameModeService.GetCurrentGameMode() == IGameModeService.GameMode.Demo;
      view.DemoObject.SetActive(isDemo);

      var leftModel = model.diContainer.Instantiate<UILobbyPortraitPresenter.Model>(new object[] { PlayerType.Left });
      leftPresenter = new(leftModel, view.LeftView);
      leftPresenter.AttachOnDestroy(view.gameObject);
      leftPresenter.ActivateAsync(true).Forget();

      var rightModel = model.diContainer.Instantiate<UILobbyPortraitPresenter.Model>(new object[] { PlayerType.Right });
      rightPresenter = new(rightModel, view.RightView);
      rightPresenter.AttachOnDestroy(view.gameObject);
      rightPresenter.ActivateAsync(true).Forget();

      var doctorModel = model.diContainer.Instantiate<UILobbyDoctorPresenter.Model>();
      doctorPresenter = new(doctorModel, view.DoctorView);
      doctorPresenter.AttachOnDestroy(view.gameObject);

      leftInitializedPos = view.LeftView.RectTransform.anchoredPosition;
      rightInitializedPos = view.RightView.RectTransform.anchoredPosition;
      doctorInitializedPos = view.DoctorView.RectTransform.anchoredPosition;
      logoInitializedPos = view.LogoImageRectTransform.anchoredPosition;

      InitializeGlasses();
      UpdaetHats(model.difficultyService.CurrentDifficulty);

      subscribeHandle = new(
        () =>
        {
          model.difficultyService.SubscribeOnDifficultyChanged(OnDifficultyChanged);
        },
        () =>
        {
          model.difficultyService.UnsubscribeOnDifficultyChanged(OnDifficultyChanged);
        });

      subscribeHandle.Subscribe();
      model.presenterContainer.Add(this);
    }

    private void InitializeGlasses()
    {
      foreach (var glassSet in view.GlasseSets)
      {
        glassSet.initializedAnchoredPosition = glassSet.RectTransform.anchoredPosition;
        glassSet.RectTransform.anchoredPosition = glassSet.initializedAnchoredPosition + Vector2.up * model.uiSO.Lobby.GlassMoveLength;
        glassSet.CanvasGroup.alpha = 0.0f;
      }
    }

    private void UpdaetHats(IDifficultyService.Difficulty currentDifficulty)
    {
      var isEnable = currentDifficulty == IDifficultyService.Difficulty.Easy;
      foreach(var hat in view.EasyHats)
        hat.SetActive(isEnable);
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      subscribeHandle.Dispose();
      stageScaleCTS.Dispose();
      panelCTS.Dispose();
      model.presenterContainer.Remove(this);
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void OnStageSelect()
    {      
      stageScaleCTS.Cancel();
      stageScaleCTS.Create();
      var token = stageScaleCTS.token;
      ScaleAsync(model.uiSO.Lobby.StagePortraitScaleValue, token).Forget();

      stageScaleFrameCount = Time.frameCount;
    }

    public void OnStageUnselect()
    {
      if (stageScaleFrameCount == Time.frameCount)
        return;

      stageScaleCTS.Cancel();
      stageScaleCTS.Create();
      var token = stageScaleCTS.token;
      ScaleAsync(1.0f, token).Forget();
    }

    private void OnDifficultyChanged(IDifficultyService.Difficulty difficulty)
    {
      UpdaetHats(difficulty);
    }

    private async UniTask ScaleAsync(float targetScale, CancellationToken token)
    {
      try
      {
        var targetDuration = model.uiSO.Lobby.StagePortraitScaleDuration;
        await DOTween
          .Sequence()
          .Join(view.LeftView.RectTransform.DOScale(targetScale, targetDuration))
          .Join(view.RightView.RectTransform.DOScale(targetScale, targetDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    public void OnPanelState(UILobbyRootPresenter.PanelState panelState)
    {      
      panelCTS.Cancel();
      panelCTS.Create();
      var token = panelCTS.token;
      switch (panelState)
      {
        case UILobbyRootPresenter.PanelState.None: break;
        case UILobbyRootPresenter.PanelState.Main: OnMainStateAsync(token).Forget(); break;
        case UILobbyRootPresenter.PanelState.Stage: OnStageStateAsync(token).Forget(); break;
        case UILobbyRootPresenter.PanelState.Dialogue: OnDialogueStateAsync(token).Forget(); break;
        case UILobbyRootPresenter.PanelState.SpeedRun: OnSpeedRunStateAsync(token).Forget(); break;
        case UILobbyRootPresenter.PanelState.Option: OnOptionStateAsync(token).Forget(); break;
        case UILobbyRootPresenter.PanelState.Localize: break;
        case UILobbyRootPresenter.PanelState.Credit: break;
      }
    }

    private async UniTask OnMainStateAsync(CancellationToken token)
    {
      onPanelStateExit?.Invoke(token);
      onPanelStateExit.RemoveAllListeners();
      await UniTask.CompletedTask;
    }

    private async UniTask OnStageStateAsync(CancellationToken token)
    {
      try
      {
        onPanelStateExit.AddListener(exitToken => OnStageStateExitAsync(exitToken).Forget());

        var moveDuration = model.uiSO.Lobby.LogoStateChangeDuration;
        var leftTaret = leftInitializedPos + model.uiSO.Lobby.LogoLeftStagePos;
        var rightTaret = rightInitializedPos + model.uiSO.Lobby.LogoRightStagePos;
        var doctorTaret = doctorInitializedPos + model.uiSO.Lobby.LogoDoctorHidePos;
        var logoTaret = logoInitializedPos + model.uiSO.Lobby.LogoIconHidePos;
        await
          DOTween
          .Sequence()
          .Join(view.LeftView.RectTransform.DOAnchorPos(leftTaret, moveDuration))
          .Join(view.RightView.RectTransform.DOAnchorPos(rightTaret, moveDuration))
          .Join(view.DoctorView.RectTransform.DOAnchorPos(doctorTaret, moveDuration))
          .Join(view.LogoImageRectTransform.DOAnchorPos(logoTaret, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask OnStageStateExitAsync(CancellationToken token)
    {
      try
      {
        var moveDuration = model.uiSO.Lobby.LogoStateChangeDuration;
        var leftTaret = leftInitializedPos;
        var rightTaret = rightInitializedPos;
        var doctorTaret = doctorInitializedPos;
        var logoTaret = logoInitializedPos;
        await
          DOTween
          .Sequence()
          .Join(view.LeftView.RectTransform.DOAnchorPos(leftTaret, moveDuration))
          .Join(view.RightView.RectTransform.DOAnchorPos(rightTaret, moveDuration))
          .Join(view.DoctorView.RectTransform.DOAnchorPos(doctorTaret, moveDuration))
          .Join(view.LogoImageRectTransform.DOAnchorPos(logoTaret, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask OnDialogueStateAsync(CancellationToken token)
    {
      try
      {
        onPanelStateExit.AddListener(exitToken => OnDialogueStateExitAsync(exitToken).Forget());

        var duration = model.uiSO.Lobby.LogoStateChangeDuration;
        var targetAlpha = 1.0f;
        var sequence = DOTween.Sequence();
        foreach (var glassSet in view.GlasseSets)
          _ = sequence
          .Join(glassSet.CanvasGroup.DOFade(targetAlpha, duration))
          .Join(glassSet.RectTransform.DOAnchorPos(glassSet.initializedAnchoredPosition, duration));

        await sequence
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask OnDialogueStateExitAsync(CancellationToken token)
    {
      try
      {
        var duration = model.uiSO.Lobby.LogoStateChangeDuration;
        var targetAlpha = 0.0f;
        var sequence = DOTween.Sequence();
        foreach (var glassSet in view.GlasseSets)
          _ = sequence
          .Join(glassSet.CanvasGroup.DOFade(targetAlpha, duration))
          .Join(glassSet.RectTransform.DOAnchorPos(glassSet.initializedAnchoredPosition + Vector2.up * model.uiSO.Lobby.GlassMoveLength, duration));

        await sequence
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask OnSpeedRunStateAsync(CancellationToken token)
    {
      try
      {
        onPanelStateExit.AddListener(exitToken => OnSpeedRunStateExitAsync(exitToken).Forget());

        YoyoPortraitsAsync(token).Forget();
        var moveDuration = model.uiSO.Lobby.LogoStateChangeDuration;
        var doctorTarget = doctorInitializedPos + model.uiSO.Lobby.LogoDoctorHidePos;
        await
          DOTween
          .Sequence()
          .Join(view.DoctorView.RectTransform.DOAnchorPos(doctorTarget, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask YoyoPortraitsAsync(CancellationToken token)
    {
      try
      {
        var time = 0.0f;
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var t = Mathf.PingPong(time, model.uiSO.Lobby.SpeedRunPortraitScaleYoyoInterval);

          var x = 1.0f + t * model.uiSO.Lobby.SpeedRunPortraitScaleValue;
          var y = 1.0f - t * model.uiSO.Lobby.SpeedRunPortraitScaleValue;
          view.LeftView.RectTransform.localScale = new Vector3(x, y, 1.0f);
          view.RightView.RectTransform.localScale = new Vector3(x, y, 1.0f);

          time += Time.deltaTime;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException)
      {
        if(this != null && view != null)
        {
          view.LeftView.RectTransform.localScale = Vector3.one;
          view.RightView.RectTransform.localScale = Vector3.one;
        }
      }
    }

    private async UniTask OnSpeedRunStateExitAsync(CancellationToken token)
    {
      try
      {
        var moveDuration = model.uiSO.Lobby.LogoStateChangeDuration;
        var doctorTarget = doctorInitializedPos;
        await
          DOTween
          .Sequence()
          .Join(view.DoctorView.RectTransform.DOAnchorPos(doctorTarget, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask OnOptionStateAsync(CancellationToken token)
    {
      try
      {
        onPanelStateExit.AddListener(exitToken => OnOptionStateExitAsync(exitToken).Forget());
      
        var moveDuration = model.uiSO.Lobby.LogoStateChangeDuration;
        var leftTaret = leftInitializedPos + model.uiSO.Lobby.LogoLeftHidePos;
        var rightTaret = rightInitializedPos + model.uiSO.Lobby.LogoRightHidePos;
        await
          DOTween
          .Sequence()
          .AppendCallback(() =>
          {
            leftPresenter.DeactivateAsync(true).Forget();
            rightPresenter.DeactivateAsync(true).Forget();
            doctorPresenter.ActivateAsync(true).Forget();
          })
          .Join(view.LeftView.RectTransform.DOAnchorPos(leftTaret, moveDuration))
          .Join(view.RightView.RectTransform.DOAnchorPos(rightTaret, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask OnOptionStateExitAsync(CancellationToken token)
    {
      try
      {
        var moveDuration = model.uiSO.Lobby.LogoStateChangeDuration;
        var leftTaret = leftInitializedPos;
        var rightTaret = rightInitializedPos;
        await
          DOTween
          .Sequence()
          .AppendCallback(() =>
          {
            leftPresenter.ActivateAsync(true).Forget();
            rightPresenter.ActivateAsync(true).Forget();
            doctorPresenter.DeactivateAsync(true).Forget();
          })
          .Join(view.LeftView.RectTransform.DOAnchorPos(leftTaret, moveDuration))
          .Join(view.RightView.RectTransform.DOAnchorPos(rightTaret, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }
  }
}
