using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.Loading
{
  public class UILoadingView : BaseUIView
  {
    [SerializeField] private Image image;

    private void Awake()
    {
      image.material.SetFloat(ShaderHash.Loading._Scale, 0.0f);
    }

    public void UpdateDirection(Vector2 vector2)
    {
      image.material.SetVector(ShaderHash.Loading._ScrollDir, vector2);
    }


    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      var duration = isImmediately ? 0.0f : UISO.LoadingFadeDuration;
      var time = 0.0f;
      try
      {
        while (time < duration)
        {
          image.material.SetFloat(ShaderHash.Loading._Scale, 1.0f - (time / duration));
          time += Time.deltaTime;
          await UniTask.Yield();
        }
        image.material.SetFloat(ShaderHash.Loading._Scale, 0.0f);
      }
      catch (OperationCanceledException) { }
      visibleState = VisibleState.Hidden;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;
      var duration = isImmediately ? 0.0f : UISO.LoadingFadeDuration;
      var time = 0.0f;
      try
      {
        while (time < duration)
        {
          image.material.SetFloat(ShaderHash.Loading._Scale, time / duration);
          time += Time.deltaTime;
          await UniTask.Yield();
        }
        image.material.SetFloat(ShaderHash.Loading._Scale, 1.0f);
      }
      catch (OperationCanceledException) { }
      visibleState = VisibleState.Showen;
    }
  }
}
