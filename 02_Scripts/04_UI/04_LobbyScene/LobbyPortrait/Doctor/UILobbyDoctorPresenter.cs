using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Input;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace LR.UI.Lobby
{
  public class UILobbyDoctorPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public UISO uiSO;
    }

    private readonly Model model;
    private readonly UILobbyDoctorView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer updateCTS = new();
    private readonly Dictionary<Direction, Vector2> positionOffsets = new();
    private readonly Dictionary<Direction, float> eulerOffsets = new();
    private readonly CTSContainer scaleCTS = new();

    public UILobbyDoctorPresenter(Model model, UILobbyDoctorView view)
    {
      this.model = model;
      this.view = view;

      foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        positionOffsets[direction] = Vector2.zero;

      eulerOffsets[Direction.Left] = 0.0f;
      eulerOffsets[Direction.Right] = 0.0f;

      subscribeHandle = new(
        () =>
        {
          model.inputActionSubscriber.SubscribePhase(LRInputType.LeftUp, OnLeftUp, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.LeftRight, OnLeftRight, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.LeftDown, OnLeftDown, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.LeftLeft, OnLeftLeft, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.RightRight, OnRightRight, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.RightLeft, OnRightLeft, InputPhase.Performed);
          model.inputActionSubscriber.Subscribe(LRInputType.UISubmit, OnSubmit);         
        },
        () =>
        {
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.LeftUp, OnLeftUp, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.LeftRight, OnLeftRight, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.LeftDown, OnLeftDown, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.LeftLeft, OnLeftLeft, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.RightRight, OnRightRight, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.RightLeft, OnRightLeft, InputPhase.Performed);
          model.inputActionSubscriber.Unsubscribe(LRInputType.UISubmit, OnSubmit);
        });

      updateCTS.Create();
      var updateToken = updateCTS.token;
      UpdateAsync(updateToken).Forget();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {      
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
      scaleCTS.Dispose();
      updateCTS.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnSubmit(InputPhase inputPhase)
    {
      var targetDuration = model.uiSO.Lobby.PortraitYoyoDuration;
      var targetScale = inputPhase switch
      {
        InputPhase.Performed => model.uiSO.Lobby.PortraitInputScaleValue,
        InputPhase.Canceled => 1.0f,
        _ => throw new NotImplementedException(),
      };
      scaleCTS.Cancel();
      scaleCTS.Create();
      var token = scaleCTS.token;
      ScaleAsync(targetDuration, targetScale, token).Forget();
    }

    private async UniTask ScaleAsync(float duration, float scale, CancellationToken token)
    {
      try
      {
        await
          DOTween
          .Sequence()
          .Join(view.RectTransform.DOScale(scale, duration))
          .SetLoops(2, LoopType.Yoyo)
          .SetLink(view.gameObject)
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException)
      {
        view.RectTransform.localScale = Vector3.one;
      }
    }

    private void OnLeftUp()
    {
      var token = updateCTS.token;
      YoyoPositionAsync(Direction.Up, token).Forget();
    }

    private void OnLeftRight()
    {
      var token = updateCTS.token;
      YoyoPositionAsync(Direction.Right, token).Forget();
    }

    private void OnLeftDown()
    {
      var token = updateCTS.token;
      YoyoPositionAsync(Direction.Down, token).Forget();
    }

    private void OnLeftLeft()
    {
      var token = updateCTS.token;
      YoyoPositionAsync(Direction.Left, token).Forget();
    }

    private void OnRightLeft()
    {
      var token = updateCTS.token;
      YoyoEulerAsync(Direction.Left, token).Forget();
    }

    private void OnRightRight()
    {
      var token = updateCTS.token;
      YoyoEulerAsync(Direction.Right, token).Forget();
    }

    private async UniTask YoyoEulerAsync(Direction direction, CancellationToken token)
    {
      try
      {
        var duration = 0.0f;
        var targetDuration = model.uiSO.Lobby.PortraitYoyoDuration * 0.5f;
        var currentValue = eulerOffsets[direction];
        var targetValue = model.uiSO.Lobby.PortraitYoyoEuler * -direction.ParseVector2().x;
        while (duration < targetDuration)
        {
          token.ThrowIfCancellationRequested();

          eulerOffsets[direction] = Mathf.Lerp(currentValue, targetValue, duration / targetDuration);

          duration += Time.deltaTime;
          await UniTask.Yield();
        }

        while (duration > 0.0f)
        {
          token.ThrowIfCancellationRequested();

          eulerOffsets[direction] = Mathf.Lerp(currentValue, targetValue, duration / targetDuration);

          duration -= Time.deltaTime;
          await UniTask.Yield();
        }
        eulerOffsets[direction] = 0.0f;
      }
      catch (OperationCanceledException)
      {
        eulerOffsets[direction] = 0.0f;
      }
    }

    private async UniTask UpdateAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var posOffset = Vector2.zero;
          foreach (var addOffset in positionOffsets.Values)
            posOffset += addOffset;
          var currentPos = view.Content.anchoredPosition;

          if (currentPos != posOffset)
            view.Content.anchoredPosition = posOffset;

          var eulerOffset = 0.0f;
          foreach (var euler in eulerOffsets.Values)
            eulerOffset += euler;
          var currentEuler = view.Content.eulerAngles.z;
          if (currentEuler != eulerOffset)
            view.Content.eulerAngles = new Vector3(0.0f, 0.0f, eulerOffset);

          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask YoyoPositionAsync(Direction direction, CancellationToken token)
    {
      try
      {
        var duration = 0.0f;
        var targetDuration = model.uiSO.Lobby.PortraitYoyoDuration * 0.5f;
        var currentValue = positionOffsets[direction];
        var targetValue = direction.ParseVector2() * model.uiSO.Lobby.PortraitYoyoLength;
        while (duration < targetDuration)
        {
          token.ThrowIfCancellationRequested();

          positionOffsets[direction] = Vector2.Lerp(currentValue, targetValue, duration / targetDuration);

          duration += Time.deltaTime;
          await UniTask.Yield();
        }

        while (duration > 0.0f)
        {
          token.ThrowIfCancellationRequested();

          positionOffsets[direction] = Vector2.Lerp(currentValue, targetValue, duration / targetDuration);

          duration -= Time.deltaTime;
          await UniTask.Yield();
        }
        positionOffsets[direction] = Vector2.zero;
      }
      catch (OperationCanceledException)
      {
        positionOffsets[direction] = Vector2.zero;
      }
    }
  }
}