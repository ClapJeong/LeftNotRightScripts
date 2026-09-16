using Cysharp.Threading.Tasks;
using LR.Manager.Stage;
using LR.UI.Enum;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.GameScene.LocalRecord
{
  public class UIStageLocalRecordPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IStageRecorderService stageRecorderService;
    }

    private readonly Model model;
    private readonly UIStageLocalRecordView view;

    public UIStageLocalRecordPresenter(Model model, UIStageLocalRecordView view)
    {
      this.model = model;
      this.view = view;

      DeactivateAsync(true).Forget();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {      
      await view.ShowAsync(isImmedieately, token);
      UpdateTexts();

      await UniTask.WaitForEndOfFrame();
      UpdateTexts();
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void UpdateTexts()
    {
      var sumData = model.stageRecorderService.GetSumResult();

      var leftSum = sumData.LeftSum;
      view.LeftHitFont.UpdateText(-leftSum);
      view.LeftAnimator.Play(leftSum == 0.0f ? AnimatorHash.LocalRecordCharacter.Perfect : AnimatorHash.LocalRecordCharacter.Hit);

      view.RunningFont.UpdateText(-sumData.Running);

      var rightSum = sumData.RightSum;
      view.RightHitFont.UpdateText(-rightSum);
      view.RightAnimator.Play(rightSum == 0.0f ? AnimatorHash.LocalRecordCharacter.Perfect : AnimatorHash.LocalRecordCharacter.Hit);

      view.ResultFont.UpdateText(-sumData.TotalSum);
    }
  }
}