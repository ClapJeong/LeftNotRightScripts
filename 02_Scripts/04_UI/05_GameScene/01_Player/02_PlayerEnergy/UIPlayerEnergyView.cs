using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using LR.UI.Enum;
using System;
using DG.Tweening;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerEnergyView : BaseUIView
  {
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector2 hidePosition;
    [field: SerializeField] public Image FillImage { get; private set; }
    [field: SerializeField] public RectTransform FillImageRectTransform { get; private set; }
    [field: SerializeField] public RectTransform CenterRectTransform { get; private set; }

    private void OnValidate()
    {
      if(FillImage != null)
      {
        var size = RectTransform.rect.size;
        FillImage.material.SetVector(ShaderHash.EnergyBar._RectSize, new Vector4(Screen.width, size.y, 0.0f, 0.0f));
      }
    }

    public void UpdateArrowScale(float scale)
    {
      FillImage.material.SetFloat(ShaderHash.EnergyBar._ArrowScale, scale);
    }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      var fadeDuration = isImmediately ? 0.0f : UISO.Player.FadeDuration;
      try
      {
        await RectTransform.DOAnchorPos(hidePosition, fadeDuration).ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }     
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;
      var fadeDuration = isImmediately ? 0.0f : UISO.Player.FadeDuration;
      try
      {
        await RectTransform.DOAnchorPos(Vector2.zero, fadeDuration).ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = VisibleState.Showen;
      }
      catch (OperationCanceledException) { }
    }
  }
}