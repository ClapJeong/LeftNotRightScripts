using Cysharp.Threading.Tasks;
using LR.Manager.Device;
using LR.Manager.Stage;
using LR.Manager.UI;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using LR.UI.GameScene.Player.PlayerInput;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerInputPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public PlayerType playerType;
      [Inject] public LR.Stage.StageDataContainer.StageGimmick stageGimmick;
      [Inject] public UISO uiSO;
      [Inject] public ColorSO colorSO;
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public IPlayerInputActionSubscriber inputActionSubscriber;
      [Inject] public IPlayerStateSubscriber stateSubscriber;
      [Inject] public IPlayerGetter playerGetter;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public IStageEventSubscriber stageEventSubscriber;
    }

    private readonly Model model;
    private readonly UIPlayerInputView view;

    private readonly SubscribeHandle playerSubscribeHandle;
    private readonly CTSContainer exhaustCTS = new();

    public PlayerType PlayerType => model.playerType;
    //Gimmick UI에서 이놈 갖다 쓰는데 좌우 구분할 때 필요함

    public readonly InputRequireModule InputRequireModule;
    public readonly SwapModule SwapModule;
    private readonly Dictionary<Direction, CTSContainer> wiggleCTSs = new();

    public UIPlayerInputPresenter(Model model, UIPlayerInputView view)
    {
      this.model = model;
      this.view = view;

      if(model.playerType == PlayerType.Right)
          model.diContainer.Inject(view.InputView);

      view.InputView.ApplyColor(model.colorSO, model.playerType);
      playerSubscribeHandle = new(Subscribes, Unsubscribes);
      playerSubscribeHandle.Subscribe();

      foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        wiggleCTSs[direction] = new();

      switch (model.stageGimmick)
      {
        case LR.Stage.StageDataContainer.StageGimmick.InputRequire:
          {
            InputRequireModule = new(view.RectTransform, model.uiSO);
          }
          break;

        case LR.Stage.StageDataContainer.StageGimmick.Swap:
          {
            SwapModule = new(
              model.playerType,
              model.colorSO,
              model.uiSO,
              model.playerGetter,
              model.deviceProvider,
              view,
              playerSubscribeHandle,
              OnInputActionPerformed,
              OnInputActionCanceled,
              UpdateIcon);
          }
          break;
      }

      SubscribeStunState();

      model.presenterContainer.Add(this);

      model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Exhausted, OnExhaust);
      model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);      
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await CacheAtlasAsync();
      await view.ShowAsync(isImmediately, token);
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await view.HideAsync(isImmediately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void Dispose()
    {
      model.stageEventSubscriber.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.Exhausted, OnExhaust);
      model.stageEventSubscriber.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);

      foreach (var ctsContainer in wiggleCTSs.Values)
        ctsContainer.Dispose();

      exhaustCTS.Dispose();
      SwapModule?.Dispose();
      InputRequireModule?.Dispose();

      model.presenterContainer.Remove(this);      
      playerSubscribeHandle.Dispose();
      UnsubscribeStunState();

      if (view)
        view.DestroySelf();
    }

    public RectTransform GetCurrentEnableRectTransform()
      => view.InputView.GetEnableRectTransform();

    private async UniTask CacheAtlasAsync()
    {
      await InputAtlasProvider.GetInputAtlasesAsync(
        model.addressableKeySO,
        model.resourceManager,
        onForeach: (deviceType, atlas) =>
        {
          view.InputView.AddAtlas(model.playerType, deviceType, atlas);
        });

      UpdateIcon(Direction.Up, false);
      UpdateIcon(Direction.Right, false);
      UpdateIcon(Direction.Down, false);
      UpdateIcon(Direction.Left, false);
    }

    private void SubscribeStunState()
    {
      model.stateSubscriber.SubscribeOnEnter(LR.Stage.Player.Enum.PlayerState.Stun, OnStunEnter);
      model.stateSubscriber.SubscribeOnExit(LR.Stage.Player.Enum.PlayerState.Stun, OnStunExit);
    }

    private void UnsubscribeStunState()
    {
      model.stateSubscriber.UnsubscribeOnEnter(LR.Stage.Player.Enum.PlayerState.Stun, OnStunEnter);
      model.stateSubscriber.UnsubscribeOnExit(LR.Stage.Player.Enum.PlayerState.Stun, OnStunExit);
    }

    private void OnStunEnter()
    {
      view.CanvasGroup.alpha = 0.3f;
    }

    private void OnStunExit()
    {
      view.CanvasGroup.alpha = 1.0f;
    }

    private void Subscribes()
    {
      model.inputActionSubscriber.SubscribePerformed(OnInputActionPerformed);
      model.inputActionSubscriber.SubscribeCanceled(OnInputActionCanceled);
      model.deviceEvnetSubscriber.SubscribeDeviceEvent(view.InputView.ToggleDeviceInputSet);
    }
    
    private void Unsubscribes()
    {
      model.inputActionSubscriber.UnsubscribePerformed(OnInputActionPerformed);
      model.inputActionSubscriber.UnsubscribeCanceled(OnInputActionCanceled);
      model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(view.InputView.ToggleDeviceInputSet);      
    }

    private void OnExhaust()
    {
      exhaustCTS.Cancel();
      exhaustCTS.Create();
      var token = exhaustCTS.token;

      view.ExhaustAsync(token).Forget();
    }

    private void OnRestart()
    {
      exhaustCTS.Cancel();
      view.ResetInputPositions();
    }

    private void OnInputActionPerformed(Direction direction)
    {
      UpdateIcon(direction, true);
    }

    private void OnInputActionCanceled(Direction direction)
    {
      UpdateIcon(direction, false);
    }

    private void UpdateIcon(Direction direction, bool isInput)
    {
      var targetScale = Vector3.one * (isInput ? model.uiSO.Player.InputIconScale : 1.0f);
      view.InputView.GetImageRectTransform(model.deviceProvider.CurrentDeviceType, direction).localScale = targetScale;
      view.InputView.UpdateIcon(model.playerType, direction, isInput);
    }
  }
}