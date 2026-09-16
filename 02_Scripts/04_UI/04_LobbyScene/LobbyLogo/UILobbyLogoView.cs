using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LR.UI.Lobby
{
  public class UILobbyLogoView : BaseUIView
  {
    [field: Header("[ Base ]")]
    [field: SerializeField] public GameObject DemoObject { get; private set; }
    [field: SerializeField] public UILobbyPortraitView LeftView { get; private set; }
    [field: SerializeField] public UILobbyDoctorView DoctorView { get; private set; }
    [field: SerializeField] public UILobbyPortraitView RightView { get; private set; }
    [field: SerializeField] public RectTransform LogoImageRectTransform { get; private set; }

    [System.Serializable]
    public class GlassSet
    {
      [field: SerializeField] public RectTransform RectTransform { get; private set; }
      [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

      [HideInInspector] public Vector2 initializedAnchoredPosition;
    }
    [field: Header("[ Decorate ]")]
    [field: SerializeField] public List<GlassSet> GlasseSets { get; private set; }
    [field: SerializeField] public List<GameObject> EasyHats { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }
  }
}