using Cysharp.Threading.Tasks;
using LR.Manager.Sound;
using LR.Manager.UI;
using LR.Table.Dialogue;
using LR.UI.Enum;
using LR.UI.GameScene.Dialogue.Root;
using LR.UI.GameScene.Player;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.UI.GameScene.Dialogue
{
  public class UIDialogueRootPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public IBGMController bgmController;
      [Inject] public SoundSO soundSO;
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public DialogueData dialogueData;
      [Inject] public UnityAction onComplete;
      [Inject] public int dialogueIndex;
      [Inject] public bool enableLobbyBackground;
      [Inject] public bool showPotraitPresenter;
    }

    private readonly Model model;
    private readonly UIDialogueRootView view;

    private readonly SequenceController sequenceController;

    public UIDialogueRootPresenter(Model model, UIDialogueRootView view)
    {
      this.model = model;
      this.view = view;

      var controllerModel = new SequenceController.Model(
        view.gameObject,
        view.backgroundView,
        view.leftTalkingCharacterView,
        view.centerTalkingCharacterView,
        view.rightTalkingCharacterView,
        view.talkingInputView,
        model.onComplete,
        model.dialogueIndex,
        model.enableLobbyBackground);
      sequenceController = model.diContainer.Instantiate<SequenceController>(new object[] { controllerModel });

      view.HideAsync(true).Forget();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      model.bgmController.UpdateVolume(model.soundSO.BGMVolume.DialogueVolume);
      foreach (var portraitPresenter in model.presenterContainer.GetAll<UIPlayerStatePortraitPresenter>())
        portraitPresenter.DeactivateAsync(isImmedieately, token).Forget();
      await view.ShowAsync(isImmedieately, token);
      await sequenceController.PlayFirstTalkingDataAsync(model.dialogueData);
    }


    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      if(model.showPotraitPresenter)
        foreach (var portraitPresenter in model.presenterContainer.GetAll<UIPlayerStatePortraitPresenter>())
          portraitPresenter.ActivateAsync(isImmedieately, token).Forget();

      await view.HideAsync(isImmedieately, token);
      Dispose();
      model.bgmController.UpdateVolume(1.0f);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      var addressableKeySO = model.diContainer.Resolve<AddressableKeySO>();
      var resourceManager = model.diContainer.Resolve<IResourceManager>();

      var viewKey = addressableKeySO.Path.UI + addressableKeySO.UIName.DialogueRoot;
      resourceManager.ReleaseAsset(viewKey);
      sequenceController.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public static async UniTask<IUIPresenter> CreateDialogueUIAsync(
      DiContainer diContainer,
      DialogueData dialogueData,
      UnityAction onComplete,
      int index,
      bool enableLobbyBackground,
      bool showPortraitAfterDialogue,
      GameObject destroyAttachGameObject)
    {
      var addressableKeySO = diContainer.Resolve<AddressableKeySO>();
      var canvasProvider = diContainer.Resolve<ICanvasProvider>();
      var resourceManager = diContainer.Resolve<IResourceManager>();

      var key =addressableKeySO.Path.UI + addressableKeySO.UIName.DialogueRoot;
      var root = canvasProvider.GetCanvas(RootType.Overlay).transform;

      var model = diContainer.Instantiate<UIDialogueRootPresenter.Model>(new object[]
      {
        dialogueData,
        onComplete,
        index,
        enableLobbyBackground,
        showPortraitAfterDialogue
      });
      var view = await resourceManager.CreateAssetAsync<UIDialogueRootView>(key, root);
      var presenter = new UIDialogueRootPresenter(model, view);
      presenter.AttachOnDestroy(destroyAttachGameObject);

      return presenter;
    }
  }
}
