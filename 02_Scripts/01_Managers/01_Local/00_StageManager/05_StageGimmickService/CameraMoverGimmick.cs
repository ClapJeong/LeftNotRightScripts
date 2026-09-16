using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Local.CameraService;
using LR.Stage.Player.Enum;
using LR.Table.StageGimmick;
using LR.UI;
using LR.UI.GameScene.StageGimmick;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace LR.Manager.Stage.Gimmick
{
  public class CameraMoverGimmick : IStageGimmick
  {
    private readonly IPlayerGetter playerGetter;
    private readonly ICameraValueService cameraValueService;
    private readonly IStageStateProvider stageStateProvider;
    private readonly CameraMoverData data;
    private readonly SubscribeHandle subscribeHandle;

    private UICameraMoverPresenter presenter;

    private readonly CTSContainer updateCTS = new();
    private readonly CTSContainer sizeCTS = new();
    private readonly Vector3 originPosition = new(0.0f, 0.0f, -10.0f);
    private readonly float originSize;

    private readonly float valueUnit;
    private float value = 0.0f;
    private bool enableInput = false;

    public CameraMoverGimmick(
      IPlayerGetter playerGetter,
      ICameraValueService cameraValueService,
      IStageStateProvider stageStateProvider,
      CameraMoverData data,
      IInputActionSubscriber inputActionSubscriber)
    {
      this.playerGetter = playerGetter;
      this.cameraValueService = cameraValueService;
      this.stageStateProvider = stageStateProvider;
      this.data = data;

      subscribeHandle = new(
        () =>
        {
          inputActionSubscriber.SubscribePhase(LRInputType.LeftUp, OnLeftUpPerformed, InputPhase.Performed);
          inputActionSubscriber.SubscribePhase(LRInputType.LeftRight, OnLeftRightPerformed, InputPhase.Performed);
          inputActionSubscriber.SubscribePhase(LRInputType.LeftDown, OnLeftDownPerformed, InputPhase.Performed);
          inputActionSubscriber.SubscribePhase(LRInputType.LeftLeft, OnLeftLeftPerformed, InputPhase.Performed);

          inputActionSubscriber.SubscribePhase(LRInputType.RightUp, OnRightUpPerformed, InputPhase.Performed);
          inputActionSubscriber.SubscribePhase(LRInputType.RightRight, OnRightRightPerformed, InputPhase.Performed);
          inputActionSubscriber.SubscribePhase(LRInputType.RightDown, OnRightDownPerformed, InputPhase.Performed);
          inputActionSubscriber.SubscribePhase(LRInputType.RightLeft, OnRightLeftPerformed, InputPhase.Performed);
        },
        () =>
        {
          inputActionSubscriber.UnsubscribePhase(LRInputType.LeftUp, OnLeftUpPerformed, InputPhase.Performed);
          inputActionSubscriber.UnsubscribePhase(LRInputType.LeftRight, OnLeftRightPerformed, InputPhase.Performed);
          inputActionSubscriber.UnsubscribePhase(LRInputType.LeftDown, OnLeftDownPerformed, InputPhase.Performed);
          inputActionSubscriber.UnsubscribePhase(LRInputType.LeftLeft, OnLeftLeftPerformed, InputPhase.Performed);

          inputActionSubscriber.UnsubscribePhase(LRInputType.RightUp, OnRightUpPerformed, InputPhase.Performed);
          inputActionSubscriber.UnsubscribePhase(LRInputType.RightRight, OnRightRightPerformed, InputPhase.Performed);
          inputActionSubscriber.UnsubscribePhase(LRInputType.RightDown, OnRightDownPerformed, InputPhase.Performed);
          inputActionSubscriber.UnsubscribePhase(LRInputType.RightLeft, OnRightLeftPerformed, InputPhase.Performed);
        });

      originSize = cameraValueService.GetInitializedOrthographizSize();
      valueUnit = 1.0f / data.ValueUnit;
    }

    public void Begin()
    {
      sizeCTS.Cancel();
      enableInput = true;
      cameraValueService.SetPosition(originPosition);
      cameraValueService.SetSize(originSize);

      updateCTS.Cancel();
      updateCTS.Create();
      var token = updateCTS.token;
      UpdateAsync(token).Forget();
    }

    public void Complete()
    {      
      enableInput = true;
      sizeCTS.Cancel();
      updateCTS.Cancel();
    }

    public void Dispose()
    {
      subscribeHandle.Dispose();
      presenter.Dispose();
      sizeCTS.Dispose();
      updateCTS.Dispose();
    }

    public void InjectUI(IUIPresenter presenter)
    {
      this.presenter = presenter as UICameraMoverPresenter;
    }

    public void Pause()
    {
      enableInput = false;
    }

    public void Restart()
    {
      value = 0.0f;
      presenter.UpdateValue(value, true);
      sizeCTS.Cancel();
      cameraValueService.SetPosition(originPosition);
      cameraValueService.SetSize(originSize);
      enableInput = false;
      updateCTS.Cancel();
    }

    public void Resume()
    {
      enableInput = true;
    }

    private async UniTask UpdateAsync(CancellationToken token)
    {
      try
      {
        subscribeHandle.Subscribe();
        while (true)
        {
          token.ThrowIfCancellationRequested();

          if (!stageStateProvider.IsPlayingState)
          {
            await UniTask.Yield();
            continue;
          }

          UpdatePosition();

          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException)
      {
        subscribeHandle.Unsubscribe();
      }
    }

    private void UpdatePosition()
    {      
      if (value == 0.0f)
        return;

      var sign = Mathf.Sign(value);
      var targetPlayerType =  sign == -1.0f ? PlayerType. Left 
                                            : PlayerType. Right;
      var targetPlayerPosition = playerGetter
        .GetPlayer(targetPlayerType)
        .GetMoveController()
        .GetCurrentPosition();
      var currentCameraPosition = cameraValueService.GetPosition();
      var lerpValue = Mathf.Lerp(0.0f, data.MaxLerpSpeed, Mathf.Abs(value));
      var lerpPosition = Vector3.Lerp(currentCameraPosition, targetPlayerPosition, Time.deltaTime * lerpValue);
      lerpPosition.z = originPosition.z;
      cameraValueService.SetPosition(lerpPosition);
    }

    private void AddValue(float addValue)
    {
      if (!enableInput)
        return;
      if (value == -1.0f && addValue < 0.0f ||
          value == 1.0f && addValue > 0.0f)
        return;

      value = Mathf.Clamp(value + addValue, -1.0f, 1.0f);
      presenter.UpdateValue(value);

      sizeCTS.Cancel();
      sizeCTS.Create();
      var token = sizeCTS.token;
      var targetSize = Mathf.Lerp(originSize, data.MinScaleRatio * originSize, Mathf.Abs(value));
      UpdateScaleAsync(targetSize, token).Forget();
    }

    private async UniTask UpdateScaleAsync(float targetSize, CancellationToken token, bool isImmediately = false)
    {
      try
      {
        var currentSize = cameraValueService.GetCurrentOrthographizSize();
        var time = 0.0f;
        var targetDuration = isImmediately ? 0.0f : data.ScaleChangeDuration;
        while(time < targetDuration)
        {
          token.ThrowIfCancellationRequested();
          if (!stageStateProvider.IsPlayingState)
          {
            await UniTask.Yield();
            continue;
          }

          var lerpSize = Mathf.Lerp(currentSize, targetSize, time / targetDuration);
          cameraValueService.SetSize(lerpSize);

          time += Time.deltaTime;
          await UniTask.Yield();
        }
        cameraValueService.SetSize(targetSize);
      }
      catch (OperationCanceledException) { }
    }

    private void OnLeftUpPerformed()
      => AddValue(-valueUnit);
    private void OnLeftRightPerformed()
      => AddValue(-valueUnit);
    private void OnLeftDownPerformed()
      => AddValue(-valueUnit);
    private void OnLeftLeftPerformed()
      => AddValue(-valueUnit);

    private void OnRightUpPerformed()
      => AddValue(valueUnit);
    private void OnRightRightPerformed()
      => AddValue(valueUnit);
    private void OnRightDownPerformed()
      => AddValue(valueUnit);
    private void OnRightLeftPerformed()
      => AddValue(valueUnit);
  }
}
