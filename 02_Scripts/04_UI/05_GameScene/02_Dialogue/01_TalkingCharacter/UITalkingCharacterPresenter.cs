using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Table.Dialogue;
using LR.UI.Enum;
using LR.UI.GameScene.Dialogue.Character;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;
using static DialogueDataEnum;

namespace LR.UI.GameScene.Dialogue
{
  public class UITalkingCharacterPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public UISO uiSO;
      [Inject] public AddressableKeySO AddressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public ISFXController sfxController;
      [Inject] public CharacterPositionType positionType;
      [Inject] public UIPortraitData portraitData;
      [Inject] public UITextPresentationData textPresentationData;
      [Inject] public SoundSO soundSO;
      [Inject] public IDifficultyService difficultyService;
    }

    private readonly Model model;
    private readonly UITalkingCharacterView view;

    private readonly PortraitController portraitController;
    private readonly TextController textController;
    private readonly BoxController boxController;
    private SpriteAtlas atlas;

    public UITalkingCharacterPresenter(Model model, UITalkingCharacterView view)
    {
      this.model = model;
      this.view = view;

      portraitController = new(
        model.positionType,
        model.portraitData, 
        view.PortraitAnimator, 
        view.SetA, 
        view.SetB,
        view.EmotionAnimator);
      textController = new(
        model.positionType,
        model.textPresentationData, 
        view.DialogueBackground,
        view.DialogueLocalize, 
        view.DialogueTMP,
        view.AnimatorTMP,
        view.Typewriter,
        model.sfxController,
        model.soundSO);
      boxController = new(model.uiSO, model.positionType, view.BoxRectTransform);

      textController.ClearText();

      var enableHat = model.difficultyService.CurrentDifficulty == IDifficultyService.Difficulty.Easy;
      view.SetA.HatView?.Initialize(enableHat);
      view.SetB.HatView?.Initialize(enableHat);

      LoadAtlasAsync()
  .ContinueWith(() =>
  {
    CacheTransparent();
    portraitController.Clear();
  });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      view.DialogueTMP.text = "";
      await view.HideAsync(isImmedieately, token);
      await UniTask.CompletedTask;
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      boxController.Dispose();
      ReleaseAtlas();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();
    
    public void ClearView()
    {
      portraitController.Clear();

      textController.ClearText();      
    }

    public async UniTask PlayCharacterDataAsync(DialogueCharacterData data, bool isFirstTalk = false)
    {
      var portrait = GetPortraitSprite(data.Portrait);
      portraitController.SetImage(portrait, data.Portrait == 0, data.PortraitChangeType, isFirstTalk);
      portraitController.PlayEmotion(data.PortraitEmotion);
      portraitController.PlayAnimation(data.PortraitAnimationType);
      portraitController.SetAlpha(Portrait.AlphaType.Max);

      view.SetA.HatView?.UpdateHatPosition(data.Portrait);
      view.SetB.HatView?.UpdateHatPosition(data.Portrait);

      await textController.SetDialogueAsync(data.DialogueKey,
        () =>
        {
          boxController.JumpAsync().Forget();
          if (model.positionType != CharacterPositionType.Center)
            boxController.RotateAsync().Forget();
        });
    }


    public void CompleteDialogueImmedieately()
    {
      textController.CompleteDialogueImmediately();
      boxController.CompleteDialogueImmediately();
    }

    public void ClearText()
      => textController.ClearText();

    private Sprite GetPortraitSprite(int index)
    {
      var spriteName = model.positionType switch
      {
        CharacterPositionType.Left => ((DialogueDataEnum.Portrait.Left)index).ToString(),
        CharacterPositionType.Center => ((DialogueDataEnum.Portrait.Center)index).ToString(),
        CharacterPositionType.Right => ((DialogueDataEnum.Portrait.Right)index).ToString(),
        _ => throw new NotImplementedException(),
      };

      return atlas.GetSprite(spriteName);
    }

    private async UniTask LoadAtlasAsync()
    {
      atlas = await model.resourceManager.LoadAssetAsync<SpriteAtlas>(
        model.AddressableKeySO.Path.SpriteAtlas +
        model.AddressableKeySO.AtlasName.GetDialoguePortrait(model.positionType));
    }

    private void ReleaseAtlas()
    {
      model.resourceManager.ReleaseAsset(
        model.AddressableKeySO.Path.SpriteAtlas +
        model.AddressableKeySO.AtlasName.GetDialoguePortrait(model.positionType));
    }

    private void CacheTransparent()
    {
      var transparent = GetPortraitSprite(0);
      portraitController.SetTransparent(transparent);
    }
  }
}
