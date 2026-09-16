using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.Lobby.Background
{
  public class BackgroundOffsetController : IDisposable
  {
    private class RightOffsetSet : IDisposable
    {      
      public float length;

      private readonly UISO uiso;
      private readonly CTSContainer cts = new();
      private bool isPlaying = false;

      public RightOffsetSet(UISO uiso)
      {
        this.uiso = uiso;
      }

      public async UniTask PlayAsync()
      {
        if (isPlaying)
          return;

        cts.Cancel();
        cts.Create();
        var token = cts.token;
        try
        {
          isPlaying = true;

          var duration = 0.0f;
          var targetDuration = uiso.Lobby.BackgroundRightOffsetDuration * 0.5f;
          while (duration < targetDuration)
          {
            token.ThrowIfCancellationRequested();

            length = uiso.Lobby.BackgroundRightOffsetLength * (duration / targetDuration);

            duration += Time.deltaTime;
            await UniTask.Yield();
          }
          while (duration > 0.0f)
          {
            token.ThrowIfCancellationRequested();

            length = uiso.Lobby.BackgroundRightOffsetLength * (duration / targetDuration);

            duration -= Time.deltaTime;
            await UniTask.Yield();
          }
          length = 0.0f;

          isPlaying = false;
        }
        catch (OperationCanceledException) { }
      }

      public void Dispose()
      {
        cts.Dispose();
      }
    }

    [Inject] private readonly UISO uiSO = null;
    [Inject] private readonly IInputActionSubscriber inputActionSubscriber = null;
    [Inject] private readonly Image image = null;

    private readonly CTSContainer leftUpdateCTS = new();
    private readonly Dictionary<Direction, RightOffsetSet> rightOffsetSet = new();

    private Direction currentLeftDirection;
    private Vector2 leftOffset = Vector2.zero;

    public void StartOffsetUpdate(Direction beginDirection)
    {
      inputActionSubscriber.SubscribePhase(LRInputType.LeftUp, OnLeftUpPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.LeftRight, OnLeftRightPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.LeftDown, OnLeftDownPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.LeftLeft, OnLeftLeftPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.RightUp, OnRightUpPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.RightRight, OnRightRightPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.RightDown, OnRightDownPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.RightLeft, OnRightLeftPerformed, InputPhase.Performed);

      currentLeftDirection = beginDirection;
      LeftOffsetUpdateAsync().Forget();
    }

    private void OnLeftUpPerformed() => currentLeftDirection = Direction.Up;
    private void OnLeftRightPerformed() => currentLeftDirection = Direction.Right;
    private void OnLeftDownPerformed() => currentLeftDirection = Direction.Down;
    private void OnLeftLeftPerformed() => currentLeftDirection = Direction.Left;

    private void OnRightUpPerformed() => PlayRightOffset(Direction.Up);
    private void OnRightRightPerformed() => PlayRightOffset(Direction.Right);
    private void OnRightDownPerformed() => PlayRightOffset(Direction.Down);
    private void OnRightLeftPerformed() => PlayRightOffset(Direction.Left);

    private void PlayRightOffset(Direction direction)
    {
      if (rightOffsetSet.TryGetValue(direction, out var existSet))
        existSet.PlayAsync().Forget();
      else
      {
        var newSet = new RightOffsetSet(uiSO);
        newSet.PlayAsync().Forget();
        rightOffsetSet[direction] = newSet;
      }
    }

    private async UniTask LeftOffsetUpdateAsync()
    {
      var token = leftUpdateCTS.token;
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          leftOffset += Time.deltaTime * uiSO.Lobby.BackgroundLeftOffsetSpeed * currentLeftDirection.ParseVector2();
          UpdateMaterialOffset();
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

    private void UpdateMaterialOffset()
    {
      var total = leftOffset;
      foreach (var pair in rightOffsetSet)
        total += pair.Key.ParseVector2() * pair.Value.length;

      image.material.SetVector(ShaderHash.LobbyBackground._Offset, total);
    }

    public void Dispose()
    {
      inputActionSubscriber.UnsubscribePhase(LRInputType.LeftUp, OnLeftUpPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.LeftRight, OnLeftRightPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.LeftDown, OnLeftDownPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.LeftLeft, OnLeftLeftPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightUp, OnRightUpPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightRight, OnRightRightPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightDown, OnRightDownPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightLeft, OnRightLeftPerformed, InputPhase.Performed);

      leftUpdateCTS.Dispose();
      foreach (var set in rightOffsetSet.Values)
        set.Dispose();
    }
  }
}
