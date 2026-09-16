using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Local.CameraService;
using LR.Table.StageGimmick;
using LR.UI;
using LR.UI.GameScene.StageGimmick;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace LR.Manager.Stage.Gimmick
{
  public class CameraRotatorGimmick : IStageGimmick
  {
    private readonly ICameraValueService cameraValueService;
    private readonly CameraBalanceData data;
    private readonly InputActionSet inputActionSet;

    private UICameraRotatorPresenter uiPresenter;

    private readonly CTSContainer rotateCTS = new();    
    private float currentEulerZ = 0.0f;

    public CameraRotatorGimmick(
      ICameraValueService cameraValueService,
      CameraBalanceData data, 
      IInputActionSubscriber inputActionSubscriber)
    {
      this.cameraValueService = cameraValueService;
      this.data = data;

      inputActionSet = new(inputActionSubscriber);
    }

    public void InjectUI(IUIPresenter presenter)
    {
      this.uiPresenter = presenter as UICameraRotatorPresenter;

      uiPresenter.ResetLeft();
      uiPresenter.ResetRight();
    }

    public void Begin()
    {
      inputActionSet.Enable(true);
      rotateCTS.Cancel();
      rotateCTS.Create();
      var token = rotateCTS.token;
      UpdateRotateAsync(token).Forget();
    }

    public void Complete()
    {
      inputActionSet.Enable(false);
      rotateCTS.Cancel();
      rotateCTS.Create();
      var token = rotateCTS.token;
      RevertRotateAsync(token).Forget();
    }

    public void Dispose()
    {
      inputActionSet.Dispose();
      rotateCTS.Dispose();
      uiPresenter.Dispose();
    }

    public void Pause()
    {
      inputActionSet.Enable(false);
      rotateCTS.Cancel();
      rotateCTS.Dispose();
    }

    public void Restart()
    {
      uiPresenter.ResetLeft();
      uiPresenter.ResetRight();
      currentEulerZ = 0.0f;
      UpdateRotations();
      inputActionSet.Enable(false);
      rotateCTS.Cancel();
    }

    public void Resume()
    {
      inputActionSet.Enable(true);
      rotateCTS.Cancel();
      rotateCTS.Create();
      var token = rotateCTS.token;
      UpdateRotateAsync(token).Forget();
    }

    private async UniTask UpdateRotateAsync(CancellationToken token)
    {
      try
      {
        var targetEulerZ = 0.0f;
        var leftCount = 0;
        var rightCount = 0;

        while (true)
        {
          token.ThrowIfCancellationRequested();

          leftCount = inputActionSet.LeftCount;
          rightCount = inputActionSet.RightCount;
          uiPresenter.UpdateLeftCount(inputActionSet.leftInputs);
          uiPresenter.UpdateRightcount(inputActionSet.rightInputs);
          if (leftCount == rightCount)
            targetEulerZ = 0.0f;
          else
            targetEulerZ = data.MaxRotation * Mathf.Sign(rightCount - leftCount);

          if(targetEulerZ != currentEulerZ)
          {
            var dir = Mathf.Sign(targetEulerZ - currentEulerZ);

            currentEulerZ = Mathf.Lerp(currentEulerZ, targetEulerZ, Time.deltaTime * data.LerpValue);

            if (dir < 0)
              currentEulerZ = Mathf.Max(targetEulerZ, currentEulerZ);
            else if (dir > 0)
              currentEulerZ = Mathf.Min(targetEulerZ, currentEulerZ);

            UpdateRotations();
          }

          await UniTask.Yield();
        }

      }
      catch (OperationCanceledException) { }
    }

    private async UniTask RevertRotateAsync(CancellationToken token)
    {
      if (currentEulerZ == 0.0f)
        return;

      var prevValue = currentEulerZ;
      var targetValue = 0.0f;
      var duration = 0.0f;
      var curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
      try
      {
        while (duration < data.RevertDuration)
        {
          token.ThrowIfCancellationRequested();

          currentEulerZ = Mathf.Lerp(prevValue, targetValue, curve.Evaluate(duration / data.RevertDuration));
          UpdateRotations();
          duration += Time.deltaTime;
          await UniTask.Yield();
        }
        currentEulerZ = targetValue;
        UpdateRotations();
      }
      catch (OperationCanceledException) { }
    }

    private void UpdateRotations()
    {
      cameraValueService.SetEuler(new Vector3(0.0f, 0.0f, currentEulerZ));
      uiPresenter.UpdateEuler(currentEulerZ);
    }
  }
}
