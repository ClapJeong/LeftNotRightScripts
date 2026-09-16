using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Input;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.Lobby
{
  public class UILobbyPortraitPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public UISO uiSO;
      [Inject] public IGameModeService gameModeService;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public PlayerType playerType;
    }

    private class OffsetSet
    {
      public CTSContainer cts;
      public Vector2 offset;
    }

    private readonly Model model;
    private readonly UILobbyPortraitView view;
    
    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer updateCTS = new();
    private readonly CTSContainer scaleCTS = new();
    private readonly Dictionary<Direction, OffsetSet> offsets = new();

    public UILobbyPortraitPresenter(Model model, UILobbyPortraitView view)
    {
      this.model = model;
      this.view = view;

      if (model.gameModeService.GetCurrentGameMode() != IGameModeService.GameMode.Demo &&
         model.gameDataProvider.IsAllClear())
        view.PortraitImage.sprite = view.ClearSprite;

      foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        offsets[direction] = new()
        {
          cts = new(),
          offset = Vector2.zero,
        };

      subscribeHandle = new(
        () =>
        {
          model.inputActionSubscriber.Subscribe(Direction.Up.ParseToLRInputType(model.playerType), OnUp);
          model.inputActionSubscriber.Subscribe(Direction.Right.ParseToLRInputType(model.playerType), OnRight);
          model.inputActionSubscriber.Subscribe(Direction.Down.ParseToLRInputType(model.playerType), OnDown);
          model.inputActionSubscriber.Subscribe(Direction.Left.ParseToLRInputType(model.playerType), OnLeft);
          if(model.playerType == PlayerType.Right)
            model.inputActionSubscriber.Subscribe(LRInputType.UISubmit, OnSubmit);
        },
        () =>
        {
          model.inputActionSubscriber.Subscribe(Direction.Up.ParseToLRInputType(model.playerType), OnUp);
          model.inputActionSubscriber.Subscribe(Direction.Right.ParseToLRInputType(model.playerType), OnRight);
          model.inputActionSubscriber.Subscribe(Direction.Down.ParseToLRInputType(model.playerType), OnDown);
          model.inputActionSubscriber.Subscribe(Direction.Left.ParseToLRInputType(model.playerType), OnLeft);
          if (model.playerType == PlayerType.Right)
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
      foreach (var set in offsets.Values)
        set.cts.Dispose();
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

    private void OnUp(InputPhase inputPhase)
    {
      var direction = Direction.Up;
      var set = offsets[direction];
      set.cts.Cancel();
      set.cts.Create();
      var token = set.cts.token;
      var target = inputPhase == InputPhase.Canceled ? Vector2.zero
                                                     : direction.ParseVector2() * model.uiSO.Lobby.PortraitYoyoLength;
      MoveAsync(direction, target, token).Forget();
    }

    private void OnRight(InputPhase inputPhase)
    {
      var direction = Direction.Right;
      var set = offsets[direction];
      set.cts.Cancel();
      set.cts.Create();
      var token = set.cts.token;
      var target = inputPhase == InputPhase.Canceled ? Vector2.zero
                                                     : direction.ParseVector2() * model.uiSO.Lobby.PortraitYoyoLength;
      MoveAsync(direction, target, token).Forget();
    }

    private void OnDown(InputPhase inputPhase)
    {
      var direction = Direction.Down;
      var set = offsets[direction];
      set.cts.Cancel();
      set.cts.Create();
      var token = set.cts.token;
      var target = inputPhase == InputPhase.Canceled ? Vector2.zero
                                                     : direction.ParseVector2() * model.uiSO.Lobby.PortraitYoyoLength;
      MoveAsync(direction, target, token).Forget();
    }

    private void OnLeft(InputPhase inputPhase)
    {
      var direction = Direction.Left;
      var set = offsets[direction];
      set.cts.Cancel();
      set.cts.Create();
      var token = set.cts.token;
      var target = inputPhase == InputPhase.Canceled ? Vector2.zero
                                                     : direction.ParseVector2() * model.uiSO.Lobby.PortraitYoyoLength;
      MoveAsync(direction, target, token).Forget();
    }

    private async UniTask UpdateAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var offset = Vector2.zero;
          foreach (var addOffset in offsets.Values)
            offset += addOffset.offset;
          var current = view.PortraitRectTransform.anchoredPosition;

          if(current != offset)
            view.PortraitRectTransform.anchoredPosition = offset;

          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask MoveAsync(Direction direction, Vector2 target, CancellationToken token)
    {
      try
      {
        var duration = 0.0f;
        var targetDuration = model.uiSO.Lobby.PortraitYoyoDuration * 0.5f;
        var currentValue = offsets[direction].offset;
        while(duration < targetDuration)
        {
          token.ThrowIfCancellationRequested();

          offsets[direction].offset = Vector2.Lerp(currentValue, target, duration / targetDuration);

          duration += Time.deltaTime;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        offsets[direction].offset = target;
      }
    }
  }
}