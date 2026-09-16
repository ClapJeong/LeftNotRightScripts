using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Device;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.GameScene.StageGimmick
{
  public class UIQTEBombPresenter : IUIStageGimmickPresenter
  {
    public class Model
    {
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public UISO uiSO;
      [Inject] public ColorSO colorSO;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public ISFXController sfxController;
      [Inject] public IStageStateProvider stageStateProvider;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public IRightInputStateService rightInputStateService;
    }

    private readonly Model model;
    private readonly UIQTEBombView view;

    private readonly CTSContainer showRandomInputCTS = new();
    private readonly CTSContainer pumpCTS = new();
    private readonly CTSContainer failCTS = new();

    private Dictionary<LRDeviceType, SpriteAtlas> inputAtlases = new();
    private SpriteAtlas currentAtlas;
    private bool isLeft = false;

    public UIQTEBombPresenter(Model model, UIQTEBombView view)
    {
      this.model = model;
      this.view = view;

      UpdateDuration(0.0f);
      view.DurationImage.color = model.colorSO.CenterColor;
      view.IconContentCanvasGroup.alpha = 0.0f;

      model.presenterContainer.Add(this);
      model.deviceEvnetSubscriber.SubscribeDeviceEvent(UpdateSelectedAtlas);
      model.rightInputStateService.SubscribeRightInputStateChanged(OnRightInputStateChanged);
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
      model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(UpdateSelectedAtlas);
      model.rightInputStateService.UnsubscribeRightInputStateChanged(OnRightInputStateChanged);
      model.presenterContainer.Remove(this);
      failCTS.Dispose();
      showRandomInputCTS.Dispose();
      pumpCTS.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void ClearIcons()
      => view.ClearIcons();

    public void UpdateTargetInput(LRInputType inputDirection, int targetCount)
    {            
      var isKeyboard = model.deviceProvider.CurrentDeviceType == LRDeviceType.Keyboard;
      var direction = inputDirection.ParseToDirection();
      var spriteName = isKeyboard ? InputIconAssetName.GetInputTypeName(inputDirection, true, model.rightInputStateService.CurrentRightInputState)
                                  : InputIconAssetName.GetInputTypeName(inputDirection, true);
      var sprite = currentAtlas.GetSprite(spriteName);
      var targetColor = Color.white;

      view.UpdateIconCount(direction, targetCount);
      view.UpdateIconSprite(direction, sprite);
      view.UpdateIconColor(direction, targetColor);

      isLeft = inputDirection.IsLeft();
    }

    public void UpdateLayout()
      => LayoutRebuilder.ForceRebuildLayoutImmediate(view.RectTransform);

    public void OnFail()
    {
      failCTS.Cancel();
      failCTS.Create();
      var token = failCTS.token;
      view
        .RootRectTransform
        .DOShakeAnchorPos(
        model.uiSO.StageGimmick.GimmickFailShakeDuration,
        model.uiSO.StageGimmick.GimmickFailShakeStrength,
        model.uiSO.StageGimmick.GimmickFailShakeVibrato)
        .ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
    }

    public void UpdateDuration(float normalized)
    {
      view.DurationImage.fillAmount = normalized;
    }

    public void OnSuccessSingleQTE(Direction direction, int index)
    {
      view.UpdateAlpha(direction, index, model.uiSO.StageGimmick.QTEBombIconDisableAlpha);
    }

    public void OnSuccessQTE()
    {
      view.IconContentCanvasGroup.alpha = 0.0f;
      UpdateDuration(0.0f);
    }

    public void BeginInput()
    {
      showRandomInputCTS.Cancel();

      view.RandomIconGameObject.SetActive(false);
      view.IconContentCanvasGroup.alpha = 1.0f;

      pumpCTS.Cancel();
      pumpCTS.Create();
      var pumpToken = pumpCTS.token;
      PumpIconAsync(pumpToken).Forget();
    }

    public void CloseImmedieately()
    {
      UpdateDuration(0.0f);
      failCTS.Cancel();
    }

    public void PlayRandomInput(bool isLeft)
    {
      pumpCTS.Cancel();

      showRandomInputCTS.Cancel();
      showRandomInputCTS.Create();
      var token = showRandomInputCTS.token;

      var targetColor = model.colorSO.GetPlayerColor(isLeft);
      view.OutlineImage.color = targetColor;
      view.RandomIconImage.color = targetColor;
      view.IconContentCanvasGroup.alpha = 0.0f;
      view.RandomIconGameObject.SetActive(true);

      view.DurationImage.color = model.colorSO.CenterColor;

      RegenShowRandomAsync(isLeft, token).Forget();
    }

    private void OnRightInputStateChanged(RightInputState rightInputState)
    {
      var deviceType = model.deviceProvider.CurrentDeviceType;

      if(deviceType == LRDeviceType.Keyboard && inputAtlases.TryGetValue(deviceType, out var atlas))
      {

        currentAtlas = inputAtlases.ContainsKey(deviceType) ? inputAtlases[deviceType]
                                                    : inputAtlases[LRDeviceType.Keyboard];

        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
          var targetInputType = isLeft ? direction.ParseToLeftInputActionType()
                                       : direction.ParseToRightInputActionType();
          var targetSpriteName = InputIconAssetName.GetInputTypeName(targetInputType, true, rightInputState);
          var sprite = currentAtlas.GetSprite(targetSpriteName);
          var color = Color.white;
          view.UpdateIconSprite(direction, sprite);
          view.UpdateIconColor(direction, color);
        }
      }
    }

    private void UpdateSelectedAtlas(LRDeviceType deviceType)
    {
      if (inputAtlases.Count == 0)
        return;

      currentAtlas = inputAtlases.ContainsKey(deviceType) ? inputAtlases[deviceType]
                                                          : inputAtlases[LRDeviceType.Keyboard];

      foreach(Direction direction in System.Enum.GetValues(typeof(Direction)))
      {
        var targetInputType = isLeft ? direction.ParseToLeftInputActionType()
                                     : direction.ParseToRightInputActionType();
        var targetSpriteName = InputIconAssetName.GetInputTypeName(targetInputType, true);
        var sprite = currentAtlas.GetSprite(targetSpriteName);
        var color = Color.white;// model.colorSO.GetPlayerColor(isLeft);
        view.UpdateIconSprite(direction, sprite);
        view.UpdateIconColor(direction, color);
      }        
    }

    private async UniTask RegenShowRandomAsync(bool isLeft, CancellationToken token)
    {
      try
      {
        if(inputAtlases.Count == 0)
        {
          inputAtlases = await InputAtlasProvider.GetInputAtlasesAsync(model.addressableKeySO, model.resourceManager);
          UpdateSelectedAtlas(model.deviceProvider.CurrentDeviceType);
        }        

        var prevInput = isLeft ? LRInputTypeUtil.GetRandomLeft() : LRInputTypeUtil.GetRandomRight();
        while (!token.IsCancellationRequested)
        {
          token.ThrowIfCancellationRequested();
          var randomInput = isLeft ? LRInputTypeUtil.GetRandomLeft() : LRInputTypeUtil.GetRandomRight();
          while (prevInput == randomInput)
          {
            token.ThrowIfCancellationRequested();

            randomInput = isLeft ? LRInputTypeUtil.GetRandomLeft() : LRInputTypeUtil.GetRandomRight();
            await UniTask.Yield();
          }

          var isKeyboard = model.deviceProvider.CurrentDeviceType == LRDeviceType.Keyboard;
          var spriteName = isKeyboard ? InputIconAssetName.GetInputTypeName(randomInput, true, model.rightInputStateService.CurrentRightInputState)
                                      : InputIconAssetName.GetInputTypeName(randomInput, true);
          var sprite = currentAtlas.GetSprite(spriteName);
          view.RandomIconImage.sprite = sprite;

          prevInput = randomInput;

          if(model.stageStateProvider.IsPlayingState)
            model.sfxController.PlayOnce(AudioSourceType.Center, SFX.QTEBombRandom);

          await UniTask.WaitForSeconds(model.uiSO.StageGimmick.QTEBombRandomShowInterval, false, PlayerLoopTiming.Update, token);
        }        
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask PumpIconAsync(CancellationToken token)
    {
      var sequence = DOTween.Sequence();

      foreach(Direction direction in System.Enum.GetValues(typeof(Direction)))
      {
        if(view.TryGetIcons(direction, out var icons))
        {
          foreach (var icon in icons)
          {
            icon.RectTransfrom.localScale = Vector3.one;
            _ = sequence.Join(
                icon.RectTransfrom.DOPunchScale(
                    Vector3.one * model.uiSO.StageGimmick.QTEBombPunchScale,
                    model.uiSO.StageGimmick.QTEBombPunchDuration));
          }
        }
      }      
      try
      {
        _ = sequence
          .SetLoops(-1)
          .SetLink(view.gameObject);

        await sequence.ToUniTask(TweenCancelBehaviour.Kill, token);        
      }
      catch (OperationCanceledException) { }
    }
  }
}
