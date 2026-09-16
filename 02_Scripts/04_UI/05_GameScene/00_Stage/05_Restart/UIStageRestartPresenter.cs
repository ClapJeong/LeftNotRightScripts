using Cysharp.Threading.Tasks;
using LR.Manager.Device;
using LR.Manager.Stage;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;

namespace LR.UI.GameScene.Stage
{
  public class UIStageRestartPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IStageEventSubscriber stageEventSubscriber;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IUIPresenterContainer uiPresenterContainer;
    }

    private readonly Model model;
    private readonly UIStageRestartView view;

    private readonly SubscribeHandle subscribeHandle;

    private Dictionary<LRDeviceType, SpriteAtlas> inputAtlases = new();

    public UIStageRestartPresenter(Model model, UIStageRestartView view)
    {
      this.model = model;
      this.view = view;

      model
        .uiPresenterContainer
        .Add(this);

      subscribeHandle = new(
        () =>
        {
          model.stageEventSubscriber.SubscribeRestartDelay(view.RestartDelayFillImage.SetFillAmount);
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(UpdateInputIcons);
        },
        () =>
        {
          model.stageEventSubscriber.UnsubscribeRestartDelay(view.RestartDelayFillImage.SetFillAmount);
          model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(UpdateInputIcons);
        });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      if (inputAtlases.Count == 0)
      {
        await CacheAtlasesAsync();
        UpdateInputIcons(model.deviceProvider.CurrentDeviceType);
      }

      subscribeHandle.Subscribe();
      await view.ShowAsync(isImmedieately, token);
    }    

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      model
        .uiPresenterContainer
        .Remove(this);
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private async UniTask CacheAtlasesAsync()
    {
      inputAtlases = await InputAtlasProvider.GetInputAtlasesAsync(model.addressableKeySO, model.resourceManager);      
    }

    private void UpdateInputIcons(LRDeviceType deviceType)
    {
      if(inputAtlases.TryGetValue(deviceType, out var spriteAtlas))
      {
        view.RestartInputImage.sprite = spriteAtlas.GetSprite(InputIconAssetName.GetInputTypeName(Manager.Input.LRInputType.StageRestart, true));
        view.RestartDelayFillImage.sprite = spriteAtlas.GetSprite(InputIconAssetName.GetInputTypeName(Manager.Input.LRInputType.StageRestart, false));
      }      
    }
  }
}