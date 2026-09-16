using Cysharp.Threading.Tasks;
using DG.Tweening;
using Febucci.TextAnimatorCore.Text;
using LR.Manager.Device;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.UI.Enum;
using LR.UI.GameScene.Dialogue.Character;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using Zenject;

namespace LR.UI.EpilogueScene
{
  public class UIEpilogueContentPresenter : IUIPresenter
  {
    public class Model
    {
      public readonly string LeftColorOpenTag = "<c_left>";
      public readonly string RightColorOpenTag = "<c_right>";
      public readonly string DoctorColorOpenTag = "<c_doc>";
      public readonly string ColorTagFormat = "<color=#{0}>";
      public readonly string ColorCloseTagBefore = "</c>";
      public readonly string ColorCloseTagAfter = "</color>";
     
      [Inject] public IBGMController bgmController;
      [Inject] public IResourceManager resourceManager;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public UISO uiSO;
      [Inject] public DialogueUIDataSO dialogueUIDataSO;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public ISFXController sfxController;
      [Inject] public SoundSO soundSO;
      [Inject] public UnityAction onComplete;
    }

    private readonly Model model;
    private readonly UIEpilogueContentView view;

    private readonly SubscribeHandle subscribeHandle;

    private const int MaxIndex = 7;
    private const int BGMIndex = 4;
    private const char Dot = '.';
    private readonly string textFormat = "eplg_{0}";    
    private readonly string leftColorTag;
    private readonly string rightColorTag;
    private readonly string docColorTag;

    private Dictionary<LRDeviceType, SpriteAtlas> inputAtlases = new();
    private int index = 0;
    private bool illustFlag = false;
    private bool isTalking = false;

    public UIEpilogueContentPresenter(Model model, UIEpilogueContentView view)
    {
      this.model = model;
      this.view = view;

      leftColorTag = string.Format(model.ColorTagFormat, ColorUtility.ToHtmlStringRGB(model.dialogueUIDataSO.TextPresentationData.LeftColor));
      rightColorTag = string.Format(model.ColorTagFormat, ColorUtility.ToHtmlStringRGB(model.dialogueUIDataSO.TextPresentationData.RightColor));
      docColorTag = string.Format(model.ColorTagFormat, ColorUtility.ToHtmlStringRGB(model.dialogueUIDataSO.TextPresentationData.DoctorColor));

      subscribeHandle = new(
        () =>
        {
          model.deviceEvnetSubscriber.SubscribeDeviceEvent(OnDeviceChanged);
          model.inputActionSubscriber.SubscribePhase(LRInputType.DialogueSkip, OnNext, InputPhase.Performed);
        },
        () =>
        {
          model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(OnDeviceChanged);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.DialogueSkip, OnNext, InputPhase.Performed);
        });

      view.TypeWriter.onCharacterVisible.AddListener(PlayTalkingSFX);
      view.TypeWriter.onTextShowed.AddListener(() =>
      {
        view.NextInputImage.SetAlpha(1.0f);
        isTalking = false;
      });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      if (inputAtlases.Count == 0)
      {
        inputAtlases = await InputAtlasProvider.GetInputAtlasesAsync(model.addressableKeySO, model.resourceManager);
        var currentDevice = model.deviceProvider.CurrentDeviceType;
        if (inputAtlases.TryGetValue(currentDevice, out var inputAtlas))
          view.NextInputImage.sprite = inputAtlas.GetSprite(InputIconAssetName.GetInputTypeName(LRInputType.DialogueSkip, true));
      }        

      isTalking = true;
      foreach (var canvasGroup in view.CanvasGroups)
        canvasGroup.alpha = 0.0f;
            
      await view.ShowAsync(isImmedieately, token);

      view.NextInputImage.SetAlpha(model.uiSO.Epilogue.InputDisableAlpha);
      view.LocalizeStringEvent.SetEntry(string.Format(textFormat, index));
      var fadeDuration = model.uiSO.Epilogue.IllustFadeDuration;
      _ = view.CanvasGroups[index].DOFade(1.0f, fadeDuration).Play();
      index++;

      subscribeHandle.Subscribe();
    }

    private async UniTask SetLocalizeKeyAsync(string key, CancellationToken token)
    {
      string resolvedText = null;

      void OnUpdate(string value) => resolvedText = value;

      try
      {
        view.LocalizeStringEvent.OnUpdateString.AddListener(OnUpdate);
        view.LocalizeStringEvent.SetEntry(key);

        await UniTask.WaitUntil(
            () => resolvedText != null,
            cancellationToken: token);
      }
      catch (OperationCanceledException) { }
      finally
      {        
        view.LocalizeStringEvent.OnUpdateString.RemoveListener(OnUpdate);

        var processed = ReplaceTags(view.TMP.text);

        view.TypeWriter.ShowText(processed);        
      }
    }

    private void PlayTalkingSFX(CharacterData data)
    {
      if (char.IsWhiteSpace(data.info.character) || data.info.character == Dot)
        return;

      model.sfxController.PlayOnce(AudioSourceType.CenterUI, model.soundSO.GetRandomTalkingSFX(CharacterPositionType.Center));
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

    private void OnDeviceChanged(LRDeviceType deviceType)
    {
      if (inputAtlases.TryGetValue(deviceType, out var inputAtlas))
        view.NextInputImage.sprite = inputAtlas.GetSprite(InputIconAssetName.GetInputTypeName(LRInputType.DialogueSkip, true));
    }

    private string ReplaceTags(string text)
    {
      text = text.Replace(model.LeftColorOpenTag, leftColorTag);

      text = text.Replace(model.RightColorOpenTag, rightColorTag);

      text = text.Replace(model.DoctorColorOpenTag, docColorTag);

      text = text.Replace(model.ColorCloseTagBefore, model.ColorCloseTagAfter);

      return text;
    }

    private async UniTask NextIndexAsync()
    {
      if (index == MaxIndex + 1)
      {
        subscribeHandle.Unsubscribe();
        model.onComplete?.Invoke();

        return;
      }

      if(index == BGMIndex - 1)
      {
        await model.bgmController.PlayLobbyBGMAsync();
        model.bgmController.UpdateVolume(1.0f);        
      }

      var targetCanvasGroup = view.CanvasGroups[index];
      var previewCanvasGroup = index >= 0 ? view.CanvasGroups[index - 1] : null;
      var animatorHash = AnimatorHash.Epilogue.GetPlayHash(index);
      view.Animator.Play(animatorHash);

      view.NextInputImage.SetAlpha(model.uiSO.Epilogue.InputDisableAlpha);
      isTalking = true;
      var duration = 0.0f;
      while(duration < model.uiSO.Epilogue.IllustFadeDuration)
      {
        var t = duration / model.uiSO.Epilogue.IllustFadeDuration;
        targetCanvasGroup.alpha = t;
        if(previewCanvasGroup != null)
          previewCanvasGroup.alpha = 1.0f - t;

        duration += Time.deltaTime;
        await UniTask.Yield();
      }

      targetCanvasGroup.alpha = 1.0f;
      if (previewCanvasGroup != null)
        previewCanvasGroup.alpha = 0.0f;     

      var stringKey = string.Format(textFormat, index);
      SetLocalizeKeyAsync(stringKey, default).Forget();

      illustFlag = !illustFlag;
      index++;
    }

    private void OnNext()
    {
      if (isTalking)
        return;

      NextIndexAsync().Forget();
    }
  }
}
