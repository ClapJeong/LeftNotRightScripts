using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.Loading
{
  public class UILoadingPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IGameDataProvider gameDataProvider;
    }

    private readonly Model model;
    private readonly UILoadingView view;

    public UILoadingPresenter(Model model, UILoadingView view)
    {
      this.model = model;
      this.view = view;

      UpdateDirection();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.HideAsync(isImmedieately, token);
      Dispose();
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

    private void UpdateDirection()
    {
      var stage = model.gameDataProvider.GetSelectedStage();
      
      var direction = stage switch
      {
        1 => Vector2.up,
        2 => Vector2.right,
        3 => Vector2.down,
        4 => Vector2.left,
        _ => Vector2.zero,
      };
      view.UpdateDirection(-direction);
    }

    public static async UniTask<IUIPresenter> CreateAsync(
      DiContainer diContainer,
      AddressableKeySO addressableKeySO,
      ICanvasProvider canvasProvider,
      IResourceManager resourceManager)
    {
      var model = diContainer.Instantiate<UILoadingPresenter.Model>();
      var loadingUIViewPath = addressableKeySO.Path.UI + addressableKeySO.UIName.Loading;
      var canvasRoot = canvasProvider.GetCanvas(RootType.SceneLoading);
      var view = await resourceManager.CreateAssetAsync<UILoadingView>(loadingUIViewPath, canvasRoot.transform);      
      var presenter = new UILoadingPresenter(model, view);
      return presenter;
    }
  }
}