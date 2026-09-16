using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.Enum;
using LR.UI.GameScene.Player;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using LR.Manager.UI;
using Zenject;

namespace LR.UI.GameScene.StageGimmick
{
  public class UIInputRequirePresenter : IUIStageGimmickPresenter
  {
    public class Model
    {
      [Inject] public UISO uiSO;
      [Inject] public IUIPresenterContainer presenterContainer;
    }

    private readonly Model model;
    private readonly UIInputRequireView view;
    
    private readonly CTSContainer bombCTS = new();

    private UIPlayerInputPresenter leftInputPresenter;
    private UIPlayerInputPresenter rightInputPresenter;

    private float leftPrevNormalized = 1.0f;
    private float rightPrevNormalized = 1.0f;

    public UIInputRequirePresenter(Model model, UIInputRequireView view)
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
      bombCTS.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void OnUpdateLeftFill(float normalized)
    {
      leftInputPresenter ??= model
        .presenterContainer
        .GetAll<UIPlayerInputPresenter>()
        .FirstOrDefault(presenter => presenter.PlayerType == LR.Stage.Player.Enum.PlayerType.Left);

      if (view == null)
        return;

      view.LeftSet.FillImage.fillAmount = normalized;
      leftInputPresenter?.InputRequireModule.UpdateNormalized(normalized);

      if(normalized > leftPrevNormalized)
      {
        view.LeftSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._Direction, 1.0f);
        view.LeftSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._ArrowRotation, 270.0f);
        view.LeftSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._ArrowSpeed, model.uiSO.StageGimmick.InputRequireIncreaseSpeed);
      }
      else if(normalized < leftPrevNormalized)
      {
        view.LeftSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._Direction, -1.0f);
        view.LeftSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._ArrowRotation, 90.0f);
        view.LeftSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._ArrowSpeed, model.uiSO.StageGimmick.InputRequireDecreaseSpeed);
      }
      leftPrevNormalized = normalized;
    }

    public void OnUpdateRightFill(float normalized)
    {
      rightInputPresenter ??= model.presenterContainer.GetAll<UIPlayerInputPresenter>().FirstOrDefault(presenter => presenter.PlayerType == LR.Stage.Player.Enum.PlayerType.Right);

      if (view == null)
        return;

      view.RightSet.FillImage.fillAmount = normalized;
      rightInputPresenter?.InputRequireModule.UpdateNormalized(normalized);

      if (normalized > rightPrevNormalized)
      {
        view.RightSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._Direction, 1.0f);
        view.RightSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._ArrowRotation, 270.0f);
        view.RightSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._ArrowSpeed, model.uiSO.StageGimmick.InputRequireIncreaseSpeed);
      }
      else if (normalized < rightPrevNormalized)
      {
        view.RightSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._Direction, -1.0f);
        view.RightSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._ArrowRotation, 90.0f);
        view.RightSet.BackgroundImage.material.SetFloat(ShaderHash.InputRequireFill._ArrowSpeed, model.uiSO.StageGimmick.InputRequireDecreaseSpeed);
      }
      rightPrevNormalized = normalized;
    }

    public void OnLeftExhaust()
    {
      leftInputPresenter?.InputRequireModule.UpdateExhausted(true);
    }

    public void OnRightExhaust()
    {
      rightInputPresenter?.InputRequireModule.UpdateExhausted(true);
    }

    public void LeftRegenBegin()
    {
      view.LeftSet.CanvasGroup.alpha = 0.4f;
    }

    public void LeftRegenComplete()
    {
      view.LeftSet.CanvasGroup.alpha = 1.0f;
      leftInputPresenter?.InputRequireModule.UpdateExhausted(false);
    }

    public void RightRegenBegin()
    {
      view.RightSet.CanvasGroup.alpha = 0.4f;
    }

    public void RightRegenComplete()
    {
      view.RightSet.CanvasGroup.alpha = 1.0f;
      rightInputPresenter?.InputRequireModule.UpdateExhausted(false);
    }

    public void ShakeUI()
    {
      bombCTS.Cancel();
      bombCTS.Create();
      var token = bombCTS.token;
      view
        .RootRectTransform
        .DOShakeAnchorPos(
        model.uiSO.StageGimmick.GimmickFailShakeDuration,
        model.uiSO.StageGimmick.GimmickFailShakeStrength,
        model.uiSO.StageGimmick.GimmickFailShakeVibrato)
        .ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
    }
  }
}
