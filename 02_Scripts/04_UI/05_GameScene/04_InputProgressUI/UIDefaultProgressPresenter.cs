using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Device;
using LR.Manager.Local.CameraService;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;

namespace LR.UI.GameScene.InputProgress
{
  public class UIDefaultProgressPresenter : IUIInputProgressPresenter
  {
    public class Model
    {
      [Inject] public UISO uiSO;
      [Inject] public ColorSO colorSO;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public ICameraValueService cameraValueService;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public ICanvasProvider canvasProvider;
    }

    private readonly Model model;
    private readonly UIDefaultProgressView view;

    private readonly CTSContainer inputImageCTS = new();
    private Dictionary<LRDeviceType, SpriteAtlas> spriteAtlases = new();
    private IDisposable followUpdateDisposable;
    private bool isSuccess;
    private float prevValue = 1.0f;
    private bool isSwapped = false;

    public UIDefaultProgressPresenter(Model model, UIDefaultProgressView view)
    {
      this.model = model;
      this.view = view;

      view.ImageRootRectTransform.localScale = Vector3.one * model.uiSO.Progress.MinScale;
      view.FillImage.fillAmount = 0.0f;
      view.InputPerformedImage.SetAlpha(0.0f);      

      model.deviceEvnetSubscriber.SubscribeDeviceEvent(UpdateAtlas);
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      spriteAtlases = await InputAtlasProvider.GetInputAtlasesAsync(model.addressableKeySO, model.resourceManager);
      UpdateAtlas(model.deviceProvider.CurrentDeviceType);

      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      if (isSuccess)
      {
        view.InputGuideImage.enabled = false;
        view.InputIdleImage.enabled = false;
        view.InputPerformedImage.enabled = false;

        await UniTask.WaitForSeconds(model.uiSO.QTE.HideDelay);
      }

      await view.HideAsync(isImmedieately, token);
      Dispose();
    }

    public void OnSwapped(bool isSwap)
    {
      this.isSwapped = isSwap;
      UpdateColor(model.colorSO.GetPlayerColor(isSwap));
      var currentDeviceType = model.deviceProvider.CurrentDeviceType;
      if (spriteAtlases.TryGetValue(currentDeviceType, out var atlas))
        view.InputPerformedImage.sprite = atlas.GetSprite(InputIconAssetName.GetInputPreviewName(isSwap ? LR.Stage.Player.Enum.PlayerType.Left : LR.Stage.Player.Enum.PlayerType.Right, false));
    }

    public void SetFollowTransform(Transform transform)
    {
      followUpdateDisposable = view
        .gameObject
        .UpdateAsObservable()
        .Subscribe(_ => UpdatePosition(transform));
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(UpdateAtlas);
      followUpdateDisposable?.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void OnProgress(float normalizedValue)
    {      
      view.FillImage.fillAmount = normalizedValue;
      var eulerZ = Mathf.Lerp(model.uiSO.Progress.ProgressRotation, 0.0f, normalizedValue);
      view.ImageRootRectTransform.eulerAngles = new Vector3(0.0f, 0.0f, eulerZ);
      view.ImageRootRectTransform.localScale = Vector3.one * Mathf.Lerp(model.uiSO.Progress.MinScale, model.uiSO.Progress.MaxScale, normalizedValue);

      if(normalizedValue > prevValue)
      {
        var randomX = UnityEngine.Random.Range(-model.uiSO.Progress.InputMoveRange, model.uiSO.Progress.InputMoveRange);
        var randomY = UnityEngine.Random.Range(-model.uiSO.Progress.InputMoveRange, model.uiSO.Progress.InputMoveRange);
        view.InputRootRectTransform.anchoredPosition = new Vector2(randomX, randomY);

        inputImageCTS.Cancel();
        inputImageCTS.Create();
        var token = inputImageCTS.token;
        view.InputPerformedImage.SetAlpha(1.0f);
        view.InputPerformedImage.DOFade(0.0f, model.uiSO.Progress.InputBlinkDuration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
      }      
      
      prevValue = normalizedValue;
    }

    public void OnComplete()
    {
      isSuccess = true;
    }

    public void OnFail()
    {
      isSuccess = false;
    }

    private void UpdateColor(Color color)
    {
      view.InputIdleImage.color = color;
      view.InputPerformedImage.color = color;
      view.InputGuideImage.color = color;
    }

    private void UpdatePosition(Transform followTarget)
    {
      var viewport = model.cameraValueService
          .WorldToViewportPoint(followTarget.position);

      var canvas = model.canvasProvider.GetCanvas(RootType.Overlay);
      var canvasRect = canvas.GetComponent<RectTransform>();

      Vector2 pos = new Vector2(
          (viewport.x - 0.5f) * canvasRect.sizeDelta.x,
          (viewport.y - 0.5f) * canvasRect.sizeDelta.y
      );

      view.RectTransform.anchoredPosition = pos;
    }

    private void UpdateAtlas(LRDeviceType deviceType)
    {
      if(spriteAtlases.TryGetValue(deviceType, out var atlas))
      {
        var targetPlayerType = isSwapped ? LR.Stage.Player.Enum.PlayerType.Left : LR.Stage.Player.Enum.PlayerType.Right;
        var idleSpriteName = InputIconAssetName.GetInputPreviewName(targetPlayerType, isIdle: true);
        view.InputIdleImage.sprite = atlas.GetSprite(idleSpriteName);

        var inputSpriteName = InputIconAssetName.GetInputPreviewName(targetPlayerType, isIdle: false);
        view.InputPerformedImage.sprite = atlas.GetSprite(inputSpriteName);
      }
    }
  }
}
