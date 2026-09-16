using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Device;
using LR.Table.Dialogue;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;

namespace LR.UI.GameScene.Dialogue
{
  public class UITalkingInputsPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public UISO uiSO;
      public UITextPresentationData textPresentationData;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;

      public Model(DialogueUIDataSO dialogueUIDataSO)
      {
        this.textPresentationData = dialogueUIDataSO.TextPresentationData;
      }
    }

    private readonly Model model;
    private readonly UITalkingInputsView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer skipMoveCTS = new();
    private Dictionary<LRDeviceType, SpriteAtlas> atlasese = new();

    public UITalkingInputsPresenter(Model model, UITalkingInputsView view)
    {
      this.model = model;
      this.view = view;
      
      subscribeHandle = new(
        () =>
        {
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(UpdateIcon);
        },
        () =>
        {
          model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(UpdateIcon);
        });
      DeactivateAsync(true).Forget();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      if(atlasese.Count == 0)
        atlasese = await InputAtlasProvider.GetInputAtlasesAsync(model.addressableKeySO, model.resourceManager);

      subscribeHandle.Subscribe();
      DeactivateLeftInput();
      DeactivateRightInput();
      SkipProgress(0.0f);
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
      skipMoveCTS.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void ActivateLeftInput()
    {
      view.Left.Input.enabled = true;
    }

    public void DeactivateLeftInput()
    {
      view.Left.Input.enabled = false;
    }

    public void ActivateRightInput()
    {
      view.Right.Input.enabled = true;
    }

    public void DeactivateRightInput()
    {
      view.Right.Input.enabled = false;
    }

    public void SkipProgress(float value)
      => view.SkipProgressImage.SetFillAmount(value);

    public void OnSkipPerformed()
    {
      skipMoveCTS.Cancel();
      skipMoveCTS.Create();
      var token = skipMoveCTS.token;
      var duration = 0.1f;

      view.SkipInput.DOAnchorPosY(-60.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
      view.SkipProgressRectTransform.DOAnchorPosY(0.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
    }

    public void OnSkipCanceled()
    {
      skipMoveCTS.Cancel();
      skipMoveCTS.Create();
      var token = skipMoveCTS.token;
      var duration = 0.1f;

      view.SkipInput.DOAnchorPosY(0.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
      view.SkipProgressRectTransform.DOAnchorPosY(-60.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
    }

    private void UpdateIcon(LRDeviceType deviceType)
    {
      if (atlasese.TryGetValue(deviceType, out var atlas)) 
      {
        view.Left.Idle.sprite = atlas.GetSprite(InputIconAssetName.GetInputPreviewName(true, true));
        view.Left.Input.sprite = atlas.GetSprite(InputIconAssetName.GetInputPreviewName(true, false));

        view.Right.Idle.sprite = atlas.GetSprite(InputIconAssetName.GetInputPreviewName(false, true));
        view.Right.Input.sprite = atlas.GetSprite(InputIconAssetName.GetInputPreviewName(false, false));

        view.SkipIcon.sprite = atlas.GetSprite(InputIconAssetName.GetInputTypeName(Manager.Input.LRInputType.DialogueSkip, true));
      }      
    }
  }
}
