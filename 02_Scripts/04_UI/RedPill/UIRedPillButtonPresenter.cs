using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI
{
  public class UIRedPillButtonPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IRedPillModeService redPillModeService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
    }

    private readonly Model model;
    private readonly UIRedPillButtonView view;

    private readonly SubscribeHandle subscribeHandle;

    public UIRedPillButtonPresenter(Model model, UIRedPillButtonView view)
    {
      this.model = model;
      this.view = view;

      var current = model.redPillModeService.IsRedPillEnable;
      updateButton(current);

      subscribeHandle = new(() =>
      {
        view.Submit.Subscribe(FlipRedPillMode);
        model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectThis);
      },
      () =>
      {
        view.Submit.Subscribe(FlipRedPillMode);
        model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectThis);
      });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
      subscribeHandle.Subscribe();
    }    

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await UniTask.CompletedTask;
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

    private void OnSelectThis(GameObject gameObject)
    {
      view.DescriptionCanvasGroup.alpha = gameObject == view.gameObject ? 1.0f : 0.0f;
    }

    private void FlipRedPillMode()
    {
      var target = !model.redPillModeService.IsRedPillEnable;
      model.redPillModeService.EnableRedPillMode(target);

      updateButton(target);
    }

    private void updateButton(bool isRedPillMode)
    {
      var targetAlpha = isRedPillMode ? 1.0f : 0.4f;
      view.CanvasGroup.alpha = targetAlpha;
    }
  }
}