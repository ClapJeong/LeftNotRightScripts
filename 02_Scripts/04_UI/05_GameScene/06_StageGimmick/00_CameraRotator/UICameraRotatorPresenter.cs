using Cysharp.Threading.Tasks;
using LR.Manager.Device;
using LR.Manager.Input;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;

namespace LR.UI.GameScene.StageGimmick
{
  public class UICameraRotatorPresenter : IUIStageGimmickPresenter
  {
    public class Model
    {
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public IRightInputStateService rightInputState;
    }

    private readonly Model model;
    private readonly UICameraRotatorView view;

    private readonly SubscribeHandle subscribeHandle;
    private Dictionary<LRDeviceType, SpriteAtlas> spriteAtlases = new();
    private SpriteAtlas currentAtlas;

    public UICameraRotatorPresenter(Model model, UICameraRotatorView view)
    {
      this.model = model;
      this.view = view;

      model.presenterContainer.Add(this);
      
      subscribeHandle = new(
        () =>
        {
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(OnDeviceChanged);
          model.rightInputState.SubscribeRightInputStateChanged(OnRightInputStateChanged);
        },
        () =>
        {
          model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(OnDeviceChanged);
          model.rightInputState.UnsubscribeRightInputStateChanged(OnRightInputStateChanged);
        });      
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      if (spriteAtlases.Count == 0)
        spriteAtlases = await InputAtlasProvider.GetInputAtlasesAsync(model.addressableKeySO, model.resourceManager);

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
      subscribeHandle.Dispose();
      model.presenterContainer.Remove(this);
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnRightInputStateChanged(RightInputState rightInputState)
    {
      var deviceType = model.deviceProvider.CurrentDeviceType;
      if (deviceType == LRDeviceType.Keyboard && 
        rightInputState == RightInputState.JIKL &&
        spriteAtlases.TryGetValue(deviceType, out var atlas))
      {
        currentAtlas = atlas;
        foreach (var rightSet in view.RightInputImageSets)
        {
          if (!rightSet.Image.enabled)
            continue;

          var direction = rightSet.direction;
          rightSet.Image.sprite = currentAtlas.GetSprite(InputIconAssetName.GetInputTypeName(direction.ParseToRightInputActionType(), false, rightInputState));
        }
      }
    }

    private void OnDeviceChanged(LRDeviceType deviceType)
    {
      if(spriteAtlases.TryGetValue(deviceType, out var atlas))
      {
        currentAtlas = atlas;

        foreach(var leftImage in view.LeftInputImageSets)
        {
          if (!leftImage.Image.enabled)
            continue;

          var direction = leftImage.direction;
          leftImage.Image.sprite = currentAtlas.GetSprite(InputIconAssetName.GetInputTypeName(direction.ParseToLeftInputActionType(), false));
        }

        foreach (var rightImage in view.RightInputImageSets)
        {
          if (!rightImage.Image.enabled)
            continue;

          var direction = rightImage.direction;
          rightImage.Image.sprite = currentAtlas.GetSprite(InputIconAssetName.GetInputTypeName(direction.ParseToRightInputActionType(), false));
        }
      }
    }

    public void ResetLeft()
    {
      foreach (var leftImage in view.LeftInputImageSets)
        leftImage.Image.enabled = false;
    }

    public void ResetRight()
    {
      foreach (var rightImage in view.RightInputImageSets)
        rightImage.Image.enabled = false;
    }

    public void UpdateLeftCount(List<Direction> directions)
    {
      for(int i = 0; i < view.LeftInputImageSets.Count; i++)
      {
        var imageSet = view.LeftInputImageSets[i];
        var isEnable = i < directions.Count;
        if (isEnable)
        {
          var direction = directions[i];
          imageSet.direction = direction;
          imageSet.Image.sprite = currentAtlas.GetSprite(InputIconAssetName.GetInputTypeName(direction.ParseToLeftInputActionType(), false));
        }          
        imageSet.Image.enabled = isEnable;
      }        
    }

    public void UpdateRightcount(List<Direction> directions)
    {
      var rightInputState = model.rightInputState.CurrentRightInputState;
      for (int i = 0; i < view.RightInputImageSets.Count; i++)
      {
        var imageSet = view.RightInputImageSets[i];
        var isEnable = i < directions.Count;
        if (isEnable)
        {
          var direction = directions[i];
          imageSet.direction = direction;
          imageSet.Image.sprite = currentAtlas.GetSprite(InputIconAssetName.GetInputTypeName(direction.ParseToRightInputActionType(), false, rightInputState));
        }
        imageSet.Image.enabled = isEnable;
      }
    }

    public void UpdateEuler(float euler)
    {
      view.PreviewRectTransform.eulerAngles = new Vector3(0.0f, 0.0f, -euler);
    }
  }
}
