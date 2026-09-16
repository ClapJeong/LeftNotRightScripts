using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Device;
using LR.Manager.GameDataManager;
using LR.Manager.Stage.Practice;
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
  public class UIPracticePresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IPracticeService practiceService;
      [Inject] public IPracticeSubscriber practiceSubscriber;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IUIPresenterContainer uiPresenterContainer;
      [Inject] public UISO uiSO;
    }

    private readonly Model model;
    private readonly UIPracticeView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer iconCTS = new();

    private Dictionary<LRDeviceType, SpriteAtlas> inputAtlases = new();

    public UIPracticePresenter(Model model, UIPracticeView view)
    {
      this.model = model;
      this.view = view;

      model
      .uiPresenterContainer
     .Add(this);

      subscribeHandle = new(
        () =>
        {
          model.practiceSubscriber.SubscribeOnPractice(OnPractice);
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(UpdateInputIcons);
        },
        () =>
        {
          model.practiceSubscriber.UnsubscribeOnPractice(OnPractice);
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
      iconCTS.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnPractice(bool isPractice)
    {
      var entryName = isPractice ? "ui_PracticeComplete" : "ui_Practice";
      view.LocalizeStringEvent.SetEntry(entryName);

      var currentDeviceType = model.deviceProvider.CurrentDeviceType;
      if (inputAtlases.TryGetValue(currentDeviceType, out var spriteAtlas))
      {
        view.PracticeIconImage.sprite = spriteAtlas.GetSprite(InputIconAssetName.GetInputTypeName(Manager.Input.LRInputType.Practice, !isPractice));
      }

      if (isPractice)
      {
        iconCTS.Create();
        var token = iconCTS.token;
        PumpIconAsync(token).Forget();
      }
      else
      {
        iconCTS.Cancel();
      }
    }

    private async UniTask PumpIconAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          var scaleValue = model.uiSO.Stage.PracticeIconScaleValue;
          var duration = model.uiSO.Stage.PracticeIconScaleDuration;
          await DOTween
            .Sequence()
            .Append(view.iconRectTransform.DOScale(scaleValue, duration))
            .Append(view.iconRectTransform.DOScale(1.0f, duration))
            .ToUniTask(TweenCancelBehaviour.Complete, token);
        }
      }
      catch (OperationCanceledException)
      {
        if (this != null && view != null)
          view.iconRectTransform.localScale = Vector3.one;
      }
    }

    private async UniTask CacheAtlasesAsync()
    {
      inputAtlases = await InputAtlasProvider.GetInputAtlasesAsync(model.addressableKeySO, model.resourceManager);
    }

    private void UpdateInputIcons(LRDeviceType deviceType)
    {
      if (inputAtlases.TryGetValue(deviceType, out var spriteAtlas))
      {
        var isPractice = model.practiceService.IsPractice;
        view.PracticeIconImage.sprite = spriteAtlas.GetSprite(InputIconAssetName.GetInputTypeName(Manager.Input.LRInputType.Practice, !isPractice));
      }
    }
  }
}