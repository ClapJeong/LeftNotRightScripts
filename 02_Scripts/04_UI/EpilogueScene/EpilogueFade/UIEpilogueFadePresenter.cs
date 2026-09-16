using Cysharp.Threading.Tasks;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;

namespace LR.UI.EpilogueScene
{
  public class UIEpilogueFadePresenter : IUIPresenter
  {
    public class Model
    {

    }

    private readonly Model model;
    private readonly UIEpilogueFadeView view;

    public UIEpilogueFadePresenter(Model model, UIEpilogueFadeView view)
    {
      this.model = model;
      this.view = view;
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
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();
  }
}
