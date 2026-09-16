using Cysharp.Threading.Tasks;
using LR.UI.Preloading;
using LR.UI.Enum;
using UnityEngine.Events;
using LR.Manager.UI;
using Zenject;

public class VeryFirstService
{
  [Inject] private readonly DiContainer diContainer = null;
  [Inject] private readonly AddressableKeySO addressableKeySO = null;
  [Inject] private readonly IResourceManager resourceManager = null;
  [Inject] private readonly ICanvasProvider canvasProvider = null;

  private UIVeryFirstLocale firstLocale;
  private UIVeryFirstCutscene firstCutscene;

  public async UniTask CreateFirstLocaleUI()
  {
    var key = addressableKeySO.Path.UI + addressableKeySO.UIName.VeryFirstLocale;
    firstLocale = await resourceManager.CreateAssetAsync<UIVeryFirstLocale>(key, canvasProvider.GetCanvas(RootType.SceneLoading).transform);
  }

  public async UniTask InitializeFirstLocaleUIAsync(UnityAction onConfirm)
  {
    var model = diContainer.Instantiate<UIVeryFirstLocale.Model>(new object[] { onConfirm });
    await firstLocale.InitializeAsync(model);
  }

  public async UniTask DestroyFirstLocaleUIAsync()
    => await firstLocale.DestroyAsync();

  public async UniTask CreateFirstTimelineAsync()
  {
    var key = addressableKeySO.Path.UI + addressableKeySO.UIName.VeryFirstCutscene;
    firstCutscene = await resourceManager.CreateAssetAsync<UIVeryFirstCutscene>(key, canvasProvider.GetCanvas(RootType.SceneLoading).transform);
    diContainer.Inject(firstCutscene);
  }

  public void PlayFirstTimeline(UnityAction onComplete)
  {
    firstCutscene.PlayCutscene(onComplete);    
  }

  public void DestroyCutscene()
  {
    firstCutscene.DestroyAsync().Forget();
    firstCutscene = null;
  }
}
