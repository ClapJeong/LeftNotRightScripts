using Cysharp.Threading.Tasks;
using LR.UI.Indicator;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using LR.UI.Enum;
using LR.Manager.UI;
using Zenject;

namespace LR.UI.Lobby
{
  public class UIChapterPanelExitButtonPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IUIIndicatorPresenter indicator;      
      [Inject] public UnityAction onExit;
    }

    private readonly Model model;
    private readonly UIChapterPanelExitButtonView view;

    private readonly SubscribeHandle subscribeHandle;

    public UIChapterPanelExitButtonPresenter(Model model, UIChapterPanelExitButtonView view)
    {
      this.model = model;
      this.view = view;

      view.SubmitDirectionSet.Subscribe(model.onExit);

      subscribeHandle = new(
        () => model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelect),
        () => model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelect));
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Subscribe();
      await view.ShowAsync(isImmedieately, token);
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

    private void OnSelect(GameObject gameObject)
    {
      if(gameObject == view.gameObject)
      {
        model.indicator.SetLeftInputGuide(Direction.Up);
      }
    }
  }
}
