using Cysharp.Threading.Tasks;
using LR.Manager.Local.CameraService;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.Stage.Complete
{
  public class StageCameraController
  {
    [Inject] private readonly ICameraValueService cameraValueService = null;
    [Inject] private readonly StageCompleteDataSO data = null;

    public async UniTask ChangeCameraSizeAsync(float begin, float target, float duration, CancellationToken token)
    {
      try
      {
        var time = 0.0f;
        while (time < duration)
        {
          token.ThrowIfCancellationRequested();

          cameraValueService.SetSize(Mathf.Lerp(begin, target, data.ZoomCurve.Evaluate(time / duration)));

          time += Time.deltaTime;
          await UniTask.Yield();
        }
        cameraValueService.SetSize(target);
      }
      catch (OperationCanceledException) { }
    }

    public async UniTask PositonLerpAsync(Vector3 targetPosition, float duration, CancellationToken token)
    {
      var beginPosition = cameraValueService.GetPosition();
      targetPosition.z = -10.0f;
      try
      {
        var time = 0.0f;
        while (time < duration)
        {
          token.ThrowIfCancellationRequested();

          cameraValueService.SetPosition(Vector3.Lerp(beginPosition, targetPosition, data.ZoomCurve.Evaluate(time / duration)));

          time += Time.deltaTime;
          await UniTask.Yield();
        }
        cameraValueService.SetPosition(targetPosition);
      }
      catch (OperationCanceledException) { }
    }
  }
}
