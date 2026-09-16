using Cysharp.Threading.Tasks;
using LR.Manager.UI;
using LR.UI.Enum;
using LR.UI.GameScene.InputQTE;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class InputQTEUIService : IInputQTEUIService
{
  [Inject] private readonly DiContainer diContainer = null;
  [Inject] private readonly LocalManager localManager = null;
  [Inject] private readonly ICanvasProvider canvasProvider = null;
  [Inject] private readonly IResourceManager resourceManager = null;
  [Inject] private readonly AddressableKeySO addressableSO = null;

  private readonly List<string> keys = new();

  public async UniTask<IUIInputQTEPresenter> GetPrsenterAsync(InputQTEEnum.UIType type, Transform followTarget)
  {
    var presenter = await CreatePresenterAsync(type);
    presenter.SetFollowTransform(followTarget);
    return presenter;
  }

  public void Dispose()
  {
    foreach (var key in keys)
      resourceManager.ReleaseAsset(key);
  }

  private async UniTask<IUIInputQTEPresenter> CreatePresenterAsync(InputQTEEnum.UIType type)
  {
    var viewKey = addressableSO.Path.UI + type.ToString();
    if(!keys.Contains(viewKey))
      keys.Add(viewKey);
    switch (type)
    {
      case InputQTEEnum.UIType.DefaultQTE:
        {
          var model = diContainer.Instantiate<UIDefaultQTEPresenter.Model>();
          var view = await resourceManager.CreateAssetAsync<UIDefaultQTEView>(viewKey, canvasProvider.GetCanvas(RootType.Overlay).transform);
          var presenter = new UIDefaultQTEPresenter(model, view);
          presenter.AttachOnDestroy(localManager.gameObject);
          return presenter;
        }

      default: throw new NotImplementedException();
    }    
  }
}
