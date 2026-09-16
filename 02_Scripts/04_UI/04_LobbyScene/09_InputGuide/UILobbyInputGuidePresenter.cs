using Cysharp.Threading.Tasks;
using LR.Manager.Device;
using LR.Manager.Input;
using LR.UI.Enum;
using LR.UI.Indicator;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.Lobby
{
  public class UILobbyInputGuidePresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public IUIIndicatorPresenter indicatorPresenter;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public ColorSO colorSO;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IDeviceProvider deviceProvider;
    }

    private readonly Model model;
    private readonly UILobbyInputGuideView view;

    private readonly SubscribeHandle subscribeHandle;

    public UILobbyInputGuidePresenter(Model model, UILobbyInputGuideView view)
    {
      this.model = model;
      this.view = view;

      view.LeftInputView.ApplyColor(model.colorSO, Stage.Player.Enum.PlayerType.Left);
      view.RightInputView.ApplyColor(model.colorSO, Stage.Player.Enum.PlayerType.Right);
      
      model.diContainer.Inject(view.RightInputView);

      model.indicatorPresenter.SubscribeLeftInputGuide(view.EnableLeftImage);

      subscribeHandle = new(
        () =>
        {
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(view.LeftInputView.ToggleDeviceInputSet);
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(view.RightInputView.ToggleDeviceInputSet);
          model.inputActionSubscriber.Subscribe(LRInputType.LeftUp, OnLeftUpInput);
          model.inputActionSubscriber.Subscribe(LRInputType.LeftRight, OnLeftRightInput);
          model.inputActionSubscriber.Subscribe(LRInputType.LeftDown, OnLeftDownInput);
          model.inputActionSubscriber.Subscribe(LRInputType.LeftLeft, OnLeftLeftInput);
          model.inputActionSubscriber.Subscribe(LRInputType.RightUp, OnRightUpInput);
          model.inputActionSubscriber.Subscribe(LRInputType.RightRight, OnRightRightInput);
          model.inputActionSubscriber.Subscribe(LRInputType.RightDown, OnRightDownInput);
          model.inputActionSubscriber.Subscribe(LRInputType.RightLeft, OnRightLeftInput);
        },
        () =>
        {
          model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(view.LeftInputView.ToggleDeviceInputSet);
          model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(view.RightInputView.ToggleDeviceInputSet);
          model.inputActionSubscriber.Unsubscribe(LRInputType.LeftUp, OnLeftUpInput);
          model.inputActionSubscriber.Unsubscribe(LRInputType.LeftRight, OnLeftRightInput);
          model.inputActionSubscriber.Unsubscribe(LRInputType.LeftDown, OnLeftDownInput);
          model.inputActionSubscriber.Unsubscribe(LRInputType.LeftLeft, OnLeftLeftInput);
          model.inputActionSubscriber.Unsubscribe(LRInputType.RightUp, OnRightUpInput);
          model.inputActionSubscriber.Unsubscribe(LRInputType.RightRight, OnRightRightInput);
          model.inputActionSubscriber.Unsubscribe(LRInputType.RightDown, OnRightDownInput);
          model.inputActionSubscriber.Unsubscribe(LRInputType.RightLeft, OnRightLeftInput);
        });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await CacheAtlasAsync();
      view.LeftInputView.ToggleDeviceInputSet(model.deviceProvider.CurrentDeviceType);
      view.RightInputView.ToggleDeviceInputSet(model.deviceProvider.CurrentDeviceType);
      subscribeHandle.Subscribe();
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
    }

    public void Dispose()
    {
      model.indicatorPresenter.UnsubscribeLeftInputGuide(view.EnableLeftImage);

      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    private async UniTask CacheAtlasAsync()
    {
      await InputAtlasProvider.GetInputAtlasesAsync(
        model.addressableKeySO,
        model.resourceManager,
        onForeach: (deviceType, atlas) =>
        {
          view.LeftInputView.AddAtlas(Stage.Player.Enum.PlayerType.Left, deviceType, atlas);
          view.RightInputView.AddAtlas(Stage.Player.Enum.PlayerType.Right, deviceType, atlas);
        });

      UpdateLeftIcon(Direction.Up, false);
      UpdateLeftIcon(Direction.Right, false);
      UpdateLeftIcon(Direction.Down, false);
      UpdateLeftIcon(Direction.Left, false);
      UpdateRightIcon(Direction.Up, false);
      UpdateRightIcon(Direction.Right, false);
      UpdateRightIcon(Direction.Down, false);
      UpdateRightIcon(Direction.Left, false);
    }

    private void UpdateLeftIcon(Direction direction, bool isInput)
      => view.LeftInputView.UpdateIcon(Stage.Player.Enum.PlayerType.Left, direction, isInput);

    private void UpdateRightIcon(Direction direction, bool isInput)
      => view.RightInputView.UpdateIcon(Stage.Player.Enum.PlayerType.Right, direction, isInput);

    #region Inputs
    private void OnLeftUpInput(InputPhase phase)
    {
      var isInput = phase switch
      {
        InputPhase.Performed => true,
        InputPhase.Canceled => false,
        _ => throw new NotImplementedException(),
      };
      UpdateLeftIcon(Direction.Up, isInput);
    }

    private void OnLeftRightInput(InputPhase phase)
    {
      var isInput = phase switch
      {
        InputPhase.Performed => true,
        InputPhase.Canceled => false,
        _ => throw new NotImplementedException(),
      };
      UpdateLeftIcon(Direction.Right, isInput);
    }

    private void OnLeftDownInput(InputPhase phase)
    {
      var isInput = phase switch
      {
        InputPhase.Performed => true,
        InputPhase.Canceled => false,
        _ => throw new NotImplementedException(),
      };
      UpdateLeftIcon(Direction.Down, isInput);
    }

    private void OnLeftLeftInput(InputPhase phase)
    {
      var isInput = phase switch
      {
        InputPhase.Performed => true,
        InputPhase.Canceled => false,
        _ => throw new NotImplementedException(),
      };
      UpdateLeftIcon(Direction.Left, isInput);
    }

    private void OnRightUpInput(InputPhase phase)
    {
      var isInput = phase switch
      {
        InputPhase.Performed => true,
        InputPhase.Canceled => false,
        _ => throw new NotImplementedException(),
      };
      UpdateRightIcon(Direction.Up, isInput);
    }

    private void OnRightRightInput(InputPhase phase)
    {
      var isInput = phase switch
      {
        InputPhase.Performed => true,
        InputPhase.Canceled => false,
        _ => throw new NotImplementedException(),
      };
      UpdateRightIcon(Direction.Right, isInput);
    }

    private void OnRightDownInput(InputPhase phase)
    {
      var isInput = phase switch
      {
        InputPhase.Performed => true,
        InputPhase.Canceled => false,
        _ => throw new NotImplementedException(),
      };
      UpdateRightIcon(Direction.Down, isInput);
    }

    private void OnRightLeftInput(InputPhase phase)
    {
      var isInput = phase switch
      {
        InputPhase.Performed => true,
        InputPhase.Canceled => false,
        _ => throw new NotImplementedException(),
      };
      UpdateRightIcon(Direction.Left, isInput);
    }
    #endregion
  }
}