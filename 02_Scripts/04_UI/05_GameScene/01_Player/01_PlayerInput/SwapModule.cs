using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Device;
using LR.Manager.Stage;
using LR.Stage.Player.Enum;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LR.UI.GameScene.Player.PlayerInput
{
  public class SwapModule : IGimmickModule
  {
    private readonly PlayerType playerType;
    private readonly ColorSO colorSO;
    private readonly UISO uiSO;
    private readonly IPlayerGetter playerGetter;
    private readonly IDeviceProvider deviceProvider;
    private readonly UIPlayerInputView view;
    private readonly SubscribeHandle myInputSubscribeHandle;
    private readonly UnityAction<Direction> onInputActionPerformed;
    private readonly UnityAction<Direction> onInputActionCanceled;
    private readonly UnityAction<Direction, bool> onUpdateInput;    

    private readonly SubscribeHandle oppositeInputSubscribeHandle;

    private readonly CTSContainer swapCTS = new();
    private bool isSwapped;

    public SwapModule(
      PlayerType playerType, 
      ColorSO colorSO, 
      UISO uiSO, 
      IPlayerGetter playerGetter,
      IDeviceProvider deviceProvider,
      UIPlayerInputView view, 
      SubscribeHandle myInputSubscribeHandle, 
      UnityAction<Direction> onInputActionPerformed, 
      UnityAction<Direction> onInputActionCanceled,
      UnityAction<Direction, bool> onUpdateInput)
    {
      this.playerType = playerType;
      this.colorSO = colorSO;
      this.uiSO = uiSO;
      this.playerGetter = playerGetter;
      this.deviceProvider = deviceProvider;
      this.view = view;
      this.myInputSubscribeHandle = myInputSubscribeHandle;
      this.onInputActionPerformed = onInputActionPerformed;
      this.onInputActionCanceled = onInputActionCanceled;
      this.onUpdateInput = onUpdateInput;

      oppositeInputSubscribeHandle = new(SubscribeOppositeInputActionController, UnsubscribeOppositeInputActionController);
    }

    public void Swap(bool isImmedieately)
    {
      isSwapped = !isSwapped;

      swapCTS.Cancel();
      swapCTS.Create();
      var token = swapCTS.token;
      var color = colorSO.GetPlayerColor(!isSwapped ? playerType : playerType.ParseOpposite());

      SwapImageAsync(color, isImmedieately, token).Forget();

      onUpdateInput?.Invoke(Direction.Up, false);
      onUpdateInput?.Invoke(Direction.Right, false);
      onUpdateInput?.Invoke(Direction.Down, false);
      onUpdateInput?.Invoke(Direction.Left, false);

      if (isSwapped)
      {
        myInputSubscribeHandle.Unsubscribe();
        oppositeInputSubscribeHandle.Subscribe();
      }
      else
      {
        oppositeInputSubscribeHandle.Unsubscribe();
        myInputSubscribeHandle.Subscribe();
      }
    }

    private async UniTask SwapImageAsync(Color targetColor, bool isImmedieately, CancellationToken token)
    {
      var duration = isImmedieately ? 0.0f : uiSO.StageGimmick.SwapDuration;
      var hideRotation = new Vector3(0.0f, 180.0f, 0.0f);
      var originRotation = Vector3.zero;
      var upScale = Vector3.one * uiSO.StageGimmick.SwapScale;
      var originScale = Vector3.one;
      try
      {
        var deviceType = deviceProvider.CurrentDeviceType;
        var sequence = DOTween.Sequence();
        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
          var rectTransform = view.InputView.GetImageRectTransform(deviceType, direction);
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
          sequence
            .Join(rectTransform.DORotate(hideRotation, duration))
            .Join(rectTransform.DOScale(upScale, duration));
        }
        sequence.AppendCallback(() =>
        {
          SetImageColor(targetColor);
        });

        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
          var rectTransform = view.InputView.GetImageRectTransform(deviceType, direction);
          sequence
            .Join(rectTransform.DORotate(originRotation, duration))
            .Join(rectTransform.DOScale(originScale, duration));
        }
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.

        await sequence.ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
      finally
      {
        if(this!= null)
        {
          var deviceType = deviceProvider.CurrentDeviceType;
          SetImageColor(targetColor);

          var targetInputStateProvider =
            playerGetter
            .GetPlayer(isSwapped ? playerType.ParseOpposite() : playerType)
            .GetInputStateProvider();

          foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
          {
            var rectTransform = view.InputView.GetImageRectTransform(deviceType, direction);
            rectTransform.eulerAngles = originRotation;
            rectTransform.localScale = originScale;

            if (targetInputStateProvider.IsPressing(direction))
              onUpdateInput?.Invoke(direction, true);
          }
        }
      }
    }

    private void SetImageColor(Color color)
    {
      var deviceType = deviceProvider.CurrentDeviceType;
      var deviceInputSet = view.InputView.GetDeviceInputSet(deviceType);
      deviceInputSet.UpInputSet.Image.color = color;
      deviceInputSet.RightInputSet.Image.color = color;
      deviceInputSet.DownInputSet.Image.color = color;
      deviceInputSet.LeftInputSet.Image.color = color;
    }

    private void SubscribeOppositeInputActionController()
    {
      var oppositeInputActionSubscriber =
        playerGetter
        .GetPlayer(playerType.ParseOpposite())
        .GetInputActionSubscriber();

      oppositeInputActionSubscriber.SubscribePerformed(onInputActionPerformed);
      oppositeInputActionSubscriber.SubscribeCanceled(onInputActionCanceled);
    }

    private void UnsubscribeOppositeInputActionController()
    {
      var oppositeInputActionSubscriber =
        playerGetter
        .GetPlayer(playerType.ParseOpposite())
        .GetInputActionSubscriber();

      oppositeInputActionSubscriber.UnsubscribePerformed(onInputActionPerformed);
      oppositeInputActionSubscriber.UnsubscribeCanceled(onInputActionCanceled);
    }


    public void Dispose()
    {
      swapCTS.Dispose();
    }
  }
}
