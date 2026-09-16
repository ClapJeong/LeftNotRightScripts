using Cysharp.Threading.Tasks;
using LR.Manager.Sound;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.Speedrun
{
  public class UISpeedRunPresenter : IUIPresenter
  {
    public class Model
    {
    }

    private readonly Model model;
    private readonly UISpeedRunView view;

    public UISpeedRunPresenter(Model model, UISpeedRunView view)
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

    public void UpdateTimer(float time)
      => view.Timer.UpdateText(time);

    public void UpdateRestartCount(int count)
      => view.RestartCount.UpdateText(count);
  }
}
