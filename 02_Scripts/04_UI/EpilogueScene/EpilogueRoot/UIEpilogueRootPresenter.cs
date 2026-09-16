using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.UI.EpilogueScene
{
  public class UIEpilogueRootPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public IBGMController bgmController;
      [Inject] public SoundSO soundSO;
      [Inject] public UnityAction onFirstFadeComplete;
      [Inject] public float firstFadeShowDelay;
      [Inject] public UnityAction onCreditComplete;
    }

    private readonly Model model;
    private readonly UIEpilogueRootView view;

    private readonly UIEpilogueFadePresenter fadePresenter;
    private readonly UIEpilogueContentPresenter contentPresenter;

    public UIEpilogueRootPresenter(Model model, UIEpilogueRootView view)
    {
      this.model = model;
      this.view = view;

      var fadeModel = model.diContainer.Instantiate<UIEpilogueFadePresenter.Model>();
      fadePresenter = new(fadeModel, view.FadeView);
      fadePresenter.DeactivateAsync(true).Forget();

      var contentModel = model.diContainer.Instantiate<UIEpilogueContentPresenter.Model>(new object[]
      {
        (UnityAction)(()=>{model.onCreditComplete?.Invoke(); })
      });
      contentPresenter = new(contentModel, view.ContentView);
      contentPresenter.DeactivateAsync(true).Forget();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      FadeDownBGMAsync().Forget();
      await view.ShowAsync(isImmedieately, token);
      await fadePresenter.ActivateAsync(isImmedieately, token);            
      model.onFirstFadeComplete?.Invoke();
      await UniTask.WaitForSeconds(model.firstFadeShowDelay, false, PlayerLoopTiming.Update, token);
      await contentPresenter.ActivateAsync(true, token);
      await fadePresenter.DeactivateAsync(isImmedieately, token);      
    }    

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await fadePresenter.ActivateAsync(isImmedieately, token);
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

    private async UniTask FadeDownBGMAsync()
    {
      var duration = 0.0f;
      while (duration < model.soundSO.BGMVolume.EpilogueFadeDuration)
      {
        duration += Time.deltaTime;
        model.bgmController.UpdateVolume(1.0f - (duration / model.soundSO.BGMVolume.EpilogueFadeDuration));

        await UniTask.Yield();
      }
      model.bgmController.UpdateVolume(0.0f);
    }

    public static async UniTask<IUIPresenter> CreateAsync(
      DiContainer diContainer,
      UnityAction onFirstFadeComplete,
      float firstFadeShowDelay,
      UnityAction onCreditComplete)
    {
      var addressableKeySO = diContainer.Resolve<AddressableKeySO>();
      var canvasProvider = diContainer.Resolve<ICanvasProvider>();
      var resourceManager = diContainer.Resolve<IResourceManager>();

      var key = addressableKeySO.Path.UI + addressableKeySO.UIName.EpilogueRoot;
      var root = canvasProvider.GetCanvas(RootType.Overlay).transform;
      var model = diContainer.Instantiate<UIEpilogueRootPresenter.Model>(new object[]
      {
        (UnityAction)(()=>{ onFirstFadeComplete?.Invoke(); }),
        firstFadeShowDelay,
        (UnityAction)(()=>{ onCreditComplete?.Invoke(); }),
      });
      var view = await resourceManager.CreateAssetAsync<UIEpilogueRootView>(key, root);
      var presenter = new UIEpilogueRootPresenter(model, view);
      presenter.AttachOnDestroy(diContainer.Resolve<LocalManager>().gameObject);

      return presenter;
    }
  }
}
