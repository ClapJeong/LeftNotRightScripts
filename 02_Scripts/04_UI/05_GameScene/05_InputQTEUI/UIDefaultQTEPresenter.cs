using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Device;
using LR.Manager.Input;
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

namespace LR.UI.GameScene.InputQTE
{
  public class UIDefaultQTEPresenter : IUIInputQTEPresenter
  {
    public class Model
    {
      [Inject] public UISO uiSO;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public ICameraValueService cameraValueService;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public ICanvasProvider canvasProvider;
      [Inject] public ColorSO colorSO;
    }

    private readonly Model model;
    private readonly UIDefaultQTEView view;

    private readonly CTSContainer iconMoveCTS = new();
    private IDisposable followUpdateDisposable;
    private Dictionary<LRDeviceType, SpriteAtlas> spriteAtlases = new();
    private SpriteAtlas selectedAtlas;
    private int count = 0;
    private bool isSequenceSuccess;
    private LRInputType selectedInputType;
    private bool isSwapped = false;

    public UIDefaultQTEPresenter(Model model, UIDefaultQTEView view)
    {
      this.model = model;
      this.view = view;

      view.IdleImage.enabled = false;
      view.InputDurationFillImage.rectTransform.localScale = Vector3.zero;

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
      if (isSequenceSuccess)
      {
        view.SequenceDurationImage.enabled = false;
        view.IdleImage.enabled = false;
        view.InputDurationFillImage.enabled = false;

        await UniTask.WaitForSeconds(model.uiSO.QTE.HideDelay);
      }

      await view.HideAsync(isImmedieately, token);
      Dispose();
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      iconMoveCTS.Dispose();
      model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(UpdateAtlas);
      followUpdateDisposable?.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void OnSwapped(bool isSwap)
    {
      this.isSwapped = isSwap;

      view.InputDurationFillImage.color = model.colorSO.GetPlayerColor(isSwap ? LR.Stage.Player.Enum.PlayerType.Right : LR.Stage.Player.Enum.PlayerType.Left);

      if (selectedInputType.IsQTEAble())
      {
        selectedInputType = selectedInputType.ParseToOpposite();
        UpdateIcon();
      }      
    }

    public void OnQTEBegin(Direction direction)
    {
      selectedInputType = isSwapped ? direction.ParseToRightInputActionType() : direction.ParseToLeftInputActionType();
      UpdateIcon();

      iconMoveCTS.Cancel();
      iconMoveCTS.Create();
      var token = iconMoveCTS.token;
      MoveIconAsync(token).Forget();
    }

    private void UpdateIcon()
    {
      if (selectedAtlas != null)
      {
        var idleSpriteName = InputIconAssetName.GetInputTypeName(selectedInputType, true);
        view.IdleImage.sprite = selectedAtlas.GetSprite(idleSpriteName);
        view.IdleImage.enabled = true;

        var inputSpriteName = InputIconAssetName.GetInputTypeName(selectedInputType, false);
        view.InputDurationFillImage.sprite = selectedAtlas.GetSprite(inputSpriteName);
        view.InputDurationFillImage.rectTransform.localScale = Vector3.one;
      }
    }

    private async UniTask MoveIconAsync(CancellationToken token)
    {
      var direction = selectedInputType.ParseToDirection().ParseVector2();
      var duration = model.uiSO.QTE.IconMoveDuration;
      var beginPosition = view.SequenceDurationImage.rectTransform.anchoredPosition + direction * model.uiSO.QTE.MoveBeginLength;
      var endPosition = view.SequenceDurationImage.rectTransform.anchoredPosition + direction * model.uiSO.QTE.MoveEndLength;
      try
      {
        view.InputRoot.anchoredPosition = beginPosition;
        await view
          .InputRoot
          .DOAnchorPos(endPosition, duration)
          .SetEase(Ease.OutQuint)
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    public void OnQTECountChanged(int count)
    {
      this.count = count;
    }

    public void OnQTEProgress(float value)
    {
      view.InputDurationFillImage.fillAmount = value;
    }

    public void OnQTEResult(bool isSuccess)
    {
      if (this != null && view != null)
      {
        var animator = view.CaptchaAnimators[count];
        var trans = animator.GetAnimatorTransitionInfo(0);
        var duration = animator.IsInTransition(0) ? model.uiSO.QTE.CaptchaCrossFadeDuration * trans.normalizedTime
                                                  : model.uiSO.QTE.CaptchaCrossFadeDuration;
        animator.CrossFade(isSuccess ? AnimatorHash.QTECaptcha.Activate : AnimatorHash.QTECaptcha.Deactivate, duration);
      }
    }

    public void OnSequenceBegin()
    {
      
    }

    public void OnSequenceProgress(float value)
    {
      view.SequenceDurationImage.rectTransform.localScale = Vector3.one * value;
    }

    public void OnSequenceResult(bool isSuccess)
    {
      isSequenceSuccess = isSuccess;
    }

    public void SetFollowTransform(Transform transform)
    {
      followUpdateDisposable = view
        .gameObject
        .UpdateAsObservable()
        .Subscribe(_ => UpdatePosition(transform));
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
        selectedAtlas = atlas;
        if(selectedInputType != LRInputType.LeftAny)
        {
          var idleSpriteName = InputIconAssetName.GetInputTypeName(selectedInputType, true);
          view.IdleImage.sprite = selectedAtlas.GetSprite(idleSpriteName);
          view.IdleImage.enabled = true;

          var InputSpriteName = InputIconAssetName.GetInputTypeName(selectedInputType, false);
          view.InputDurationFillImage.sprite = selectedAtlas.GetSprite(InputSpriteName);
        }        
      }      
    }
  }
}
