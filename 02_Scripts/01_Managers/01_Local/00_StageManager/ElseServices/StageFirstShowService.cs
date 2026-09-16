using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Stage.Complete;
using LR.Stage.StageDataContainer;
using System;
using UnityEngine;
using Zenject;

namespace LR.Manager.Stage
{
  public class StageFirstShowService : IDisposable
  {
    [Inject] private readonly ICameraValueService cameraValueService = null;
    [Inject] private readonly StageDataContainer stageDataContainer = null;
    [Inject] private readonly StageShowDataSO stageShowDataSO = null;
    [Inject] private readonly StageCameraController stageCameraController = null;
    [Inject] private readonly StageLightController stageLightController = null;
    [Inject] private readonly ISFXController sfxController = null;
    [Inject] private readonly IInputActionSubscriber inputActionSubscriber = null;

    private readonly CTSContainer cts = new();

    public async UniTask PlayAsync()
    {
      inputActionSubscriber.SubscribePhase(LRInputType.DialogueSkip, SkipShow, InputPhase.Performed);

      var token = cts.token;

      var leftPosition = stageDataContainer.leftPlayerClearTileView.transform.position;
      var rightPosition = stageDataContainer.rightPlayerClearTileView.transform.position;
      var centerPosition = (leftPosition + rightPosition) * 0.5f;
      var targetCameraPosition = centerPosition * 0.3f;

      var beginSize = cameraValueService.GetCurrentOrthographizSize();
      var fitSize = CalculateSize(leftPosition, rightPosition);
      var minSize = Mathf.Max(stageShowDataSO.ZoomMinSize, beginSize);
      var targetSize = Mathf.Max(minSize, fitSize + stageShowDataSO.ZoomAdditionalSpace);

      try
      {
        await UniTask.WhenAll(
          stageCameraController.ChangeCameraSizeAsync(beginSize, targetSize, stageShowDataSO.ZoomBeginDuration, token),
          stageCameraController.PositonLerpAsync(targetCameraPosition, stageShowDataSO.MoveBeginDuration, token));

        await UniTask.WaitForSeconds(stageShowDataSO.ClearTurnOnDelay, false, PlayerLoopTiming.Update, token);

        EnableClearObjects();

        sfxController.PlayOnce(AudioSourceType.Center, SFX.ClearTurnOn);
        await UniTask.WaitForSeconds(stageShowDataSO.LightTurnOnDelay, false, PlayerLoopTiming.Update, token);
        stageLightController.EnablePlayerLights(true);
        sfxController.PlayOnce(AudioSourceType.Center, SFX.StageTurnOn);
        await UniTask.WaitForSeconds(stageShowDataSO.ZoomOutDelay, false, PlayerLoopTiming.Update, token);

        var currentSize = cameraValueService.GetCurrentOrthographizSize();
        await UniTask.WhenAll(
          stageCameraController.ChangeCameraSizeAsync(currentSize, beginSize, stageShowDataSO.ZoomEndDuration, token),
          stageCameraController.PositonLerpAsync(Vector2.zero, stageShowDataSO.MoveEndDuration, token));
      }
      catch (OperationCanceledException) { }
      finally
      {
        EnableClearObjects();
        stageLightController.EnablePlayerLights(true);
        cameraValueService.SetSize(beginSize);
        cameraValueService.SetPosition(new Vector3(0.0f, 0.0f, -10.0f));
        inputActionSubscriber.UnsubscribePhase(LRInputType.DialogueSkip, SkipShow, InputPhase.Performed);
      }
    }

    private void EnableClearObjects()
    {
      stageDataContainer.leftPlayerClearTileView.EnableLight = true;
      stageDataContainer.leftPlayerClearTileView.IdleParticle.Play();
      stageDataContainer.rightPlayerClearTileView.EnableLight = true;
      stageDataContainer.rightPlayerClearTileView.IdleParticle.Play();
    }

    private void SkipShow()
    {
      cts.Cancel();
    }

    float CalculateSize(Vector3 A, Vector3 B)
    {
      var diff = B - A;

      float width = Mathf.Abs(diff.x);
      float height = Mathf.Abs(diff.y);

      float screenAspect = (float)Screen.width / Screen.height;

      float sizeFromHeight = height * 0.5f;
      float sizeFromWidth = width * 0.5f / screenAspect;

      return Mathf.Max(sizeFromHeight, sizeFromWidth);
    }

    public void Dispose()
    {
      cts.Dispose();
    }
  }
}
