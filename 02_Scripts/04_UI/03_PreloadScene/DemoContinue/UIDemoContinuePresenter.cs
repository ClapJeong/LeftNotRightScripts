using Cysharp.Threading.Tasks;
using LR.Manager.Device;
using LR.Manager.Input;
using LR.Manager.UI;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using LR.UI.Indicator;
using LR.UI.Input;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.UI.Preloading
{
  public class UIDemoContinuePresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public UISO uiSO;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public ColorSO colorSO;
      [Inject] public IUIIndicatorService indicatorService;
      [Inject] public IUIDepthService depthService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;

      [Inject] public UnityAction onContinue;
      [Inject] public UnityAction onReset;
    };

    private readonly Model model;
    private readonly UIDemoContinueView view;

    private readonly SubscribeHandle subscribeHandle;
    private IUIIndicatorPresenter indicator;

    public UIDemoContinuePresenter(Model model, UIDemoContinueView view)
    {
      this.model = model;
      this.view = view;

      subscribeHandle = new(
        () =>
        {
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(view.LeftInputView.ToggleDeviceInputSet);
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(view.RightInputView.ToggleDeviceInputSet);
          view.LeftInputView.ApplyColor(model.colorSO, Stage.Player.Enum.PlayerType.Left);
          view.RightInputView.ApplyColor(model.colorSO, Stage.Player.Enum.PlayerType.Right);

          model.inputActionSubscriber.Subscribe(LRInputType.LeftUp, OnLeftUp);
          model.inputActionSubscriber.Subscribe(LRInputType.LeftRight, OnLeftRight);
          model.inputActionSubscriber.Subscribe(LRInputType.LeftDown, OnLeftDown);
          model.inputActionSubscriber.Subscribe(LRInputType.LeftLeft, OnLeftLeft);
          model.inputActionSubscriber.Subscribe(LRInputType.RightUp, OnRightUp);
          model.inputActionSubscriber.Subscribe(LRInputType.RightRight, OnRightRight);
          model.inputActionSubscriber.Subscribe(LRInputType.RightDown, OnRightDown);
          model.inputActionSubscriber.Subscribe(LRInputType.RightLeft, OnRightLeft);

          model.depthService.RaiseDepth(view.ContinueDirectionSet.gameObject);

          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnGameObjectSelected);
        },
        () =>
        {
          model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(view.LeftInputView.ToggleDeviceInputSet);
          model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(view.RightInputView.ToggleDeviceInputSet);

          model.inputActionSubscriber.Unsubscribe(LRInputType.LeftUp, OnLeftUp);
          model.inputActionSubscriber.Unsubscribe(LRInputType.LeftRight, OnLeftRight);
          model.inputActionSubscriber.Unsubscribe(LRInputType.LeftDown, OnLeftDown);
          model.inputActionSubscriber.Unsubscribe(LRInputType.LeftLeft, OnLeftLeft);
          model.inputActionSubscriber.Unsubscribe(LRInputType.RightUp, OnRightUp);
          model.inputActionSubscriber.Unsubscribe(LRInputType.RightRight, OnRightRight);
          model.inputActionSubscriber.Unsubscribe(LRInputType.RightDown, OnRightDown);
          model.inputActionSubscriber.Unsubscribe(LRInputType.RightLeft, OnRightLeft);

          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnGameObjectSelected);

          model.indicatorService.ReleaseTopIndicator();
          model.depthService.LowerDepth();
        });

      view.ContinueDirectionSet.Subscribe(OnContinue);
      view.ResetDirectionSet.Subscribe(OnReset);
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await InputAtlasProvider.GetInputAtlasesAsync(
  model.addressableKeySO,
  model.resourceManager,
  (deviceType, atlas) =>
  {
    view.LeftInputView.AddAtlas(PlayerType.Left, deviceType, atlas);
    view.RightInputView.AddAtlas(PlayerType.Right, deviceType, atlas);
  });
      subscribeHandle.Subscribe();

      indicator = await model.indicatorService.GetNewAsync(view.IndicatorRoot, view.ContinueDirectionSet.RectTransform);      

      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();

      await view.HideAsync(isImmedieately, token);

      view.DestroySelf();
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      subscribeHandle.Dispose();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnContinue()
    {
      subscribeHandle.Unsubscribe();
      model.onContinue?.Invoke();      
    }
    
    private void OnReset()
    {
      subscribeHandle.Unsubscribe();
      model.onReset?.Invoke();
    }

    private void OnGameObjectSelected(GameObject gameObject)
    {
      if (indicator != null)
      {
        indicator.MoveAsync(gameObject).Forget();
      }
    }

    #region InputView
    private void OnLeftUp(InputPhase inputPhase)
    {
      var direction = Direction.Up;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnLeftPerformed(direction); break;
        case InputPhase.Canceled: OnLeftCanceled(direction); break;
      }
    }

    private void OnLeftRight(InputPhase inputPhase)
    {
      var direction = Direction.Right;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnLeftPerformed(direction); break;
        case InputPhase.Canceled: OnLeftCanceled(direction); break;
      }
    }

    private void OnLeftDown(InputPhase inputPhase)
    {
      var direction = Direction.Down;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnLeftPerformed(direction); break;
        case InputPhase.Canceled: OnLeftCanceled(direction); break;
      }
    }

    private void OnLeftLeft(InputPhase inputPhase)
    {
      var direction = Direction.Left;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnLeftPerformed(direction); break;
        case InputPhase.Canceled: OnLeftCanceled(direction); break;
      }
    }

    private void OnRightUp(InputPhase inputPhase)
    {
      var direction = Direction.Up;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnRightPerformed(direction); break;
        case InputPhase.Canceled: OnRightCanceled(direction); break;
      }
    }

    private void OnRightRight(InputPhase inputPhase)
    {
      var direction = Direction.Right;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnRightPerformed(direction); break;
        case InputPhase.Canceled: OnRightCanceled(direction); break;
      }
    }

    private void OnRightDown(InputPhase inputPhase)
    {
      var direction = Direction.Down;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnRightPerformed(direction); break;
        case InputPhase.Canceled: OnRightCanceled(direction); break;
      }
    }

    private void OnRightLeft(InputPhase inputPhase)
    {
      var direction = Direction.Left;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnRightPerformed(direction); break;
        case InputPhase.Canceled: OnRightCanceled(direction); break;
      }
    }

    private void OnLeftPerformed(Direction direction)
      => UpdateIcon(view.LeftInputView, PlayerType.Left, direction, true);

    private void OnLeftCanceled(Direction direction)
      => UpdateIcon(view.LeftInputView, PlayerType.Left, direction, false);

    private void OnRightPerformed(Direction direction)
      => UpdateIcon(view.RightInputView, PlayerType.Right, direction, true);

    private void OnRightCanceled(Direction direction)
      => UpdateIcon(view.RightInputView, PlayerType.Right, direction, false);

    private void UpdateIcon(UIInputView inputView, PlayerType playerType, Direction direction, bool isInput)
    {
      var targetScale = Vector3.one * (isInput ? model.uiSO.Player.InputIconScale : 1.0f);
      inputView.GetImageRectTransform(model.deviceProvider.CurrentDeviceType, direction).localScale = targetScale;
      inputView.UpdateIcon(playerType, direction, isInput);
    }
    #endregion
  }
}
