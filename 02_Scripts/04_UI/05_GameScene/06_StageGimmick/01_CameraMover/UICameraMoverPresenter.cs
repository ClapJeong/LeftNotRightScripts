using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.UI;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.GameScene.StageGimmick
{
  public class UICameraMoverPresenter : IUIStageGimmickPresenter
  {
    public class Model
    {
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public UISO uiSO;
      [Inject] public ColorSO colorSO;
    }

    private readonly Model model;
    private readonly UICameraMoverView view;

    private readonly CTSContainer updateCTS = new();

    public UICameraMoverPresenter(Model model, UICameraMoverView view)
    {
      this.model = model;
      this.view = view;

      view.ArrowImage.color = model.colorSO.CenterColor;
      view.OutlineImage.color = model.colorSO.CenterColor;

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
      updateCTS.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void UpdateValue(float value, bool isImmediately = false)
    {
      var targetPlayerType = value > 0.0f ? PlayerType.Right 
                                          : PlayerType.Left;

      var targetColor = Color.Lerp(
        model.colorSO.CenterColor,
        model.colorSO.GetPlayerColor(targetPlayerType),
        Mathf.Abs(value));
      view.ArrowImage.color = targetColor;
      view.OutlineImage.color = targetColor;

      updateCTS.Cancel();
      updateCTS.Create();
      var token = updateCTS.token;

      var targetEulerZ = Mathf.Lerp(
        -model.uiSO.StageGimmick.CameraMoverMaxEuler,
        model.uiSO.StageGimmick.CameraMoverMaxEuler,
        (value + 1.0f) * 0.5f);
      UpdateArrowRotationAsync(targetEulerZ, token, isImmediately).Forget();

      var targetRectSet = value > 0.0f ? view.RightRectSet 
                                       : view.LeftRectSet;

      var abs = Mathf.Abs(value);
      var targetPosition = Vector3.Lerp(
        view.OriginRectSet.AnchoredPosition,
        targetRectSet.AnchoredPosition,
        abs);
      var targetPivot = Vector2.Lerp(
        view.OriginRectSet.Pivot,
        targetRectSet.Pivot,
        abs);
      var targetSize = Vector3.Lerp(
        view.OriginRectSet.Size,
        targetRectSet.Size,
        abs);
      var targetAnchorMin = Vector2.Lerp(
        view.OriginRectSet.AnchorMin,
        targetRectSet.AnchorMin,
        abs);
      var targetAnchorMax = Vector2.Lerp(
        view.OriginRectSet.AnchorMax,
        targetRectSet.AnchorMax,
        abs);
      UpdateRectAsync(
        targetPosition, 
        targetPivot,
        targetSize, 
        targetAnchorMin, 
        targetAnchorMax,
        token, 
        isImmediately).Forget();
    }

    private async UniTask UpdateArrowRotationAsync(float targetEulerZ, CancellationToken token, bool isImmediately)
    {
      try
      {
        var duration = isImmediately ? 0.0f : model.uiSO.StageGimmick.CameraMoverUpdateDuration;
        await
          view
          .ArrowRectTransform
          .DORotate(new Vector3(0.0f, 0.0f, targetEulerZ), duration)
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask UpdateRectAsync(
      Vector2 anchoredPosition, 
      Vector2 pivot,
      Vector2 size, 
      Vector2 anchorMin,
      Vector2 anchorMax,
      CancellationToken token, 
      bool isImmediately)
    {
      try
      {
        var beginPosition = view.ContentRectTransform.anchoredPosition;
        var beginPivot = view.ContentRectTransform.pivot;
        var beginSize = new Vector2(view.ContentRectTransform.rect.width, view.ContentRectTransform.rect.height);
        var beginAnchorMin = view.ContentRectTransform.anchorMin;
        var beginAnchorMax = view.ContentRectTransform.anchorMax;
        var duration = isImmediately ? 0.0f : model.uiSO.StageGimmick.CameraMoverUpdateDuration;

        var time = 0.0f;
        while (time < duration)
        {
          token.ThrowIfCancellationRequested();

          var t = time / duration;          
          view.ContentRectTransform.anchoredPosition = Vector3.Lerp(beginPosition, anchoredPosition, t);
          view.ContentRectTransform.pivot = Vector2.Lerp(beginPivot, pivot, t);
          view.ContentRectTransform.anchorMin = Vector2.Lerp(beginAnchorMin, anchorMin, t);
          view.ContentRectTransform.anchorMax = Vector2.Lerp(beginAnchorMax, anchorMax, t);
          view.ContentRectTransform.SetSize(Vector2.Lerp(beginSize, size, t));

          time += Time.deltaTime;
          await UniTask.Yield();
        }

        view.ContentRectTransform.anchoredPosition = anchoredPosition;
        view.ContentRectTransform.pivot = pivot;
        view.ContentRectTransform.anchorMin = anchorMin;
        view.ContentRectTransform.anchorMax = anchorMax;
        view.ContentRectTransform.SetSize(size);
      }
      catch (OperationCanceledException) { }
    }
  }
}
