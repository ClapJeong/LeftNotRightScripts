using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.UI.Enum;
using LR.UI.GameScene.Dialogue.Character;
using LR.UI.Indicator;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace LR.UI.VolumeControl
{
  public class UIVolumeControlPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public UISO uiSO;
      [Inject] public IVolumeProvider volumeProvider;
      [Inject] public IVolumeController volumeController;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public ISFXController sfxController;
    }

    private readonly Model model;
    private readonly UIVolumeControlView view;

    private readonly SubscribeHandle subscribeHandle;

    public UIVolumeControlPresenter(Model model, UIVolumeControlView view)
    {
      this.model = model;
      this.view = view;

      model.diContainer.Inject(view.MasterVolumeSet);
      model.diContainer.Inject(view.BGMVolumeSet);
      model.diContainer.Inject(view.SFXVolumeSet);

      subscribeHandle = new(
        () =>
        {
          model.inputActionSubscriber.SubscribePhase(LRInputType.RightLeft, OnLeftPerformed, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.RightRight, OnRightPerformed, InputPhase.Performed);
        },
        () =>
        {
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.RightLeft, OnLeftPerformed, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.RightRight, OnRightPerformed, InputPhase.Performed);
        });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.ShowAsync(isImmedieately, token);
      subscribeHandle.Subscribe();
    }    

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnLeftPerformed()
    {
      var currenSelectedGameObject = EventSystem.current.currentSelectedGameObject;

      if (currenSelectedGameObject == view.MasterVolumeSet.gameObject)
        SubVolume(VolumeType.Master);
      else if (currenSelectedGameObject == view.BGMVolumeSet.gameObject)
        SubVolume(VolumeType.BGM);
      else if (currenSelectedGameObject == view.SFXVolumeSet.gameObject)
        SubVolume(VolumeType.SFX);
    }

    private void AddVolume(VolumeType volumeType)
    {
      model.sfxController.PlayOnce(AudioSourceType.RightUI, SFX.UIGoodSubmit);
      var prev = model.volumeProvider.GetNormalizedVolume(volumeType);
      var target = prev + model.uiSO.VolumeControl.VolumeUnit;
      model.volumeController.SetVolume(volumeType,  target);
    }

    private void OnRightPerformed()
    {
      var currenSelectedGameObject = EventSystem.current.currentSelectedGameObject;

      if (currenSelectedGameObject == view.MasterVolumeSet.gameObject)
        AddVolume(VolumeType.Master);
      else if (currenSelectedGameObject == view.BGMVolumeSet.gameObject)
        AddVolume(VolumeType.BGM);
      else if (currenSelectedGameObject == view.SFXVolumeSet.gameObject)
        AddVolume(VolumeType.SFX);
    }

    private void SubVolume(VolumeType volumeType)
    {
      model.sfxController.PlayOnce(AudioSourceType.RightUI, SFX.UIGoodSubmit);
      var prev = model.volumeProvider.GetNormalizedVolume(volumeType);
      var target = prev - model.uiSO.VolumeControl.VolumeUnit;
      model.volumeController.SetVolume(volumeType, target);
    }

  }
}
