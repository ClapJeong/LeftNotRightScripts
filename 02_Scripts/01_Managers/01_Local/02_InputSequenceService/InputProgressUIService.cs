using Cysharp.Threading.Tasks;
using LR.UI.GameScene.InputProgress;
using UnityEngine;
using LR.UI.Enum;
using System.Collections.Generic;
using LR.Manager.UI;
using Zenject;

public class InputProgressUIService : IInputProgressUIService
{
  [Inject] private readonly DiContainer diContainer = null;
  [Inject] private readonly LocalManager localManager = null;
  [Inject] private readonly ICanvasProvider canvasProvider = null;
  [Inject] private readonly IResourceManager resourceManager = null;
  [Inject] private readonly AddressableKeySO addressableSO = null;

  private readonly List<string> keys = new();

  public async UniTask<IUIInputProgressPresenter> GetPresenterAsync(InputProgressEnum.UIType type, Transform followTarget)
  {
    var presenter = await CreateAsync(type);
    presenter.SetFollowTransform(followTarget);
    return presenter;
  }

  public void Dispose()
  {
    foreach(var key in keys)
      resourceManager.ReleaseAsset(key);
  }

  private async UniTask<IUIInputProgressPresenter> CreateAsync(InputProgressEnum.UIType type)
  {    
    var viewKey = addressableSO.Path.UI + type.ToString();
    if(!keys.Contains(viewKey))
      keys.Add(viewKey);
    switch (type)
    {
      case InputProgressEnum.UIType.DefaultProgress:
        {
          var model = diContainer.Instantiate<UIDefaultProgressPresenter.Model>();
          var view = await resourceManager.CreateAssetAsync<UIDefaultProgressView>(viewKey, canvasProvider.GetCanvas(RootType.Overlay).transform);
          var presenter = new UIDefaultProgressPresenter(model, view);
          presenter.AttachOnDestroy(localManager.gameObject);
          return presenter;
        }

      default: throw new System.NotImplementedException();
    }
  }
}
