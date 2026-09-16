using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.Enum;
using LR.UI.GameScene.Player;
using System;
using System.Threading;
using UnityEngine;
using LR.Manager.UI;

namespace LR.UI.GameScene.StageGimmick
{
  public class UISwapPresenter : IUIStageGimmickPresenter
  {
    public class Model
    {
      public IUIPresenterContainer presenterContainer;
      public UISO uiSO;
      public ColorSO colorSO;

      public Model(IUIPresenterContainer presenterContainer, UISO uiSO, ColorSO colorSO)
      {
        this.presenterContainer = presenterContainer;
        this.uiSO = uiSO;
        this.colorSO = colorSO;
      }
    }

    private readonly Model model;
    private readonly UISwapView view;

    private readonly CTSContainer swapCTS = new();

    public UISwapPresenter(Model model, UISwapView view)
    {
      this.model = model;
      this.view = view;

      model.presenterContainer.Add(this);
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
      model.presenterContainer.Remove(this);
      swapCTS.Cancel();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void UpdateFill(float normalized)
    {
      view.LeftFillImage.fillAmount = normalized;
      view.RightFillImage.fillAmount = normalized;
    }

    public void Swap(bool isSwapped, bool isImmedieately)
    {
      swapCTS.Cancel();
      swapCTS.Create();
      var token = swapCTS.token;
      SwapRootAsync(isSwapped, isImmedieately, token).Forget();

      foreach (var inputPresenter in model.presenterContainer.GetAll<UIPlayerInputPresenter>())
        inputPresenter.SwapModule.Swap(isImmedieately);
    }

    private async UniTask SwapRootAsync(bool isSwapped, bool isImmedieately, CancellationToken token)
    {
      var isColorSwapComplete = false;
      try
      {
        var duration = isImmedieately ? 0.0f : model.uiSO.StageGimmick.SwapDuration;
        await DOTween
          .Sequence()
          .Append(view.InputRootRectTransform.DORotate(new Vector3(0.0f, 90.0f, 0.0f), duration))
          .Join(view.RootRectTransform.DOScale(model.uiSO.StageGimmick.SwapScale, duration))
          .AppendCallback(() =>
          {
            ApplySwapColor(isSwapped);
            isColorSwapComplete = true;
          })
          .Append(view.InputRootRectTransform.DORotate(Vector3.zero, duration))
          .Join(view.RootRectTransform.DOScale(Vector3.one, duration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException)
      {
        
      }
      finally
      {
        if(!isColorSwapComplete)
          ApplySwapColor(isSwapped);
      }
    }

    private void ApplySwapColor(bool isSwapped)
    {
      var leftColor = model.colorSO.GetPlayerColor(!isSwapped);
      var rightColor = model.colorSO.GetPlayerColor(isSwapped);

      view.LeftInputImage.color = leftColor;
      view.LeftFillImage.color = leftColor;

      view.RightInputImage.color = rightColor;
      view.RightFillImage.color = rightColor;
    }
  }
}
