using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Table.Dialogue;
using System;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.GameScene.Dialogue.Root
{
  public class TalkingController : IDisposable
  {
    public class Model
    {
      public GameObject rootView;
      public UIDialogueBackgroundView backgroundView;
      public UITalkingCharacterView leftView;
      public UITalkingCharacterView centerView;
      public UITalkingCharacterView rightView;
      public UITalkingInputsView inputView;
      public List<Image> inputEnableImages;
      public UnityAction onSkip;
      public UnityAction onNextTalk;
      public int dialogueIndex;
      public bool enableLobbyBackground;

      public Model(
        GameObject rootView,
        UIDialogueBackgroundView backgroundView,
        UITalkingCharacterView leftView, 
        UITalkingCharacterView centerView, 
        UITalkingCharacterView rightView, 
        UITalkingInputsView inputView, 
        List<Image> inputEnableImages, 
        UnityAction onSkip, 
        UnityAction onNextTalk, 
        int dialogueIndex,
        bool enableLobbyBackground)
      {
        this.rootView = rootView;
        this.backgroundView = backgroundView;
        this.leftView = leftView;
        this.centerView = centerView;
        this.rightView = rightView;
        this.inputView = inputView;
        this.inputEnableImages = inputEnableImages;
        this.onSkip = onSkip;
        this.onNextTalk = onNextTalk;
        this.dialogueIndex = dialogueIndex;
        this.enableLobbyBackground = enableLobbyBackground;
      }
    }

    private readonly UISO uiSO;
    private readonly Model model;

    private readonly UIDialogueBackgroundPresenter dialogueBackgroundPresenter;
    private readonly UITalkingCharacterPresenter leftCharacterPresenter;
    private readonly UITalkingCharacterPresenter centerCharacterPresenter;
    private readonly UITalkingCharacterPresenter rightCharacterPresenter;
    private readonly UITalkingInputsPresenter inputPresenter;
    private readonly DialogueInputActionController dialogueInputActionController;

    private readonly CompositeDisposable disposables = new();
    private readonly CTSContainer dialogueCTS = new();
    private readonly CTSContainer inputMoveCTS = new();
    private bool isTalkling = false;

    public TalkingController(
      DiContainer diContainer,
      LocalManager localManager,
      UISO uISO,
      DialogueUIDataSO dialogueUISO,
      Model model)
    {
      this.uiSO = uISO;
      this.model = model;

      var attachTarget = localManager.gameObject;

      var backgroundModel = diContainer.Instantiate<UIDialogueBackgroundPresenter.Model>(new object[] { this.model.dialogueIndex, this.model.enableLobbyBackground });
      this.dialogueBackgroundPresenter = new(backgroundModel, model.backgroundView);
      dialogueBackgroundPresenter.AttachOnDestroy(attachTarget);
      dialogueBackgroundPresenter.AddTo(disposables);

      var leftModel = diContainer.Instantiate<UITalkingCharacterPresenter.Model>(new object[] {CharacterPositionType.Left, dialogueUISO.PortraitData, dialogueUISO.TextPresentationData });
      this.leftCharacterPresenter = new (leftModel, model.leftView);
      leftCharacterPresenter.AttachOnDestroy(attachTarget);
      leftCharacterPresenter.AddTo(disposables);

      var centerModel = diContainer.Instantiate<UITalkingCharacterPresenter.Model>(new object[] { CharacterPositionType.Center, dialogueUISO.PortraitData, dialogueUISO.TextPresentationData });
      this.centerCharacterPresenter = new (centerModel, model.centerView);
      centerCharacterPresenter.AttachOnDestroy(attachTarget);
      centerCharacterPresenter.AddTo(disposables);

      var rightModel = diContainer.Instantiate<UITalkingCharacterPresenter.Model>(new object[] { CharacterPositionType.Right, dialogueUISO.PortraitData, dialogueUISO.TextPresentationData });
      this.rightCharacterPresenter = new (rightModel, model.rightView);
      rightCharacterPresenter.AttachOnDestroy(attachTarget);
      rightCharacterPresenter.AddTo(disposables);

      var inputModel = diContainer.Instantiate<UITalkingInputsPresenter.Model>();
      this.inputPresenter = new (inputModel, model.inputView);
      inputPresenter.AttachOnDestroy(attachTarget);
      inputPresenter.AddTo(disposables);

      var inputActionModel = new DialogueInputActionController.InputActionModel(
        onLeftPerformed: inputPresenter.ActivateLeftInput,
        onLeftCanceled: inputPresenter.DeactivateLeftInput,
        onRightPerformed: inputPresenter.ActivateRightInput,
        onRightCanceled: inputPresenter.DeactivateRightInput,
        onNextInput: OnNextInput,
        onSkipPerformed: inputPresenter.OnSkipPerformed,
        onSkipProgress: inputPresenter.SkipProgress,
        onSkipCanceled: inputPresenter.OnSkipCanceled,
        onSkip: model.onSkip);
      dialogueInputActionController = diContainer.Instantiate<DialogueInputActionController>(new object[] { inputActionModel });
      dialogueInputActionController.AddTo(disposables);
    }

    public async UniTask ActivateAsync(bool isImmediately, CancellationToken token)
    {
      ClearViews();

      await UniTask.WhenAll(
        leftCharacterPresenter.ActivateAsync(isImmediately, token),
      centerCharacterPresenter.ActivateAsync(isImmediately, token),
      rightCharacterPresenter.ActivateAsync(isImmediately, token),
      inputPresenter.ActivateAsync(isImmediately, token));     
    }

    public async UniTask DeactivateAsync(bool isImmediately, CancellationToken token)
    {
      await UniTask.WhenAll(
        DisalbeTalkingInputsAsync(),
        leftCharacterPresenter.DeactivateAsync(isImmediately, token),
        centerCharacterPresenter.DeactivateAsync(isImmediately, token),
        rightCharacterPresenter.DeactivateAsync(isImmediately, token),
        inputPresenter.DeactivateAsync(isImmediately, token));
    }

    public void ResetInputs()
    {
      inputPresenter.DeactivateLeftInput();
      inputPresenter.DeactivateRightInput();
    }

    public void ClearViews()
    {
      leftCharacterPresenter.ClearView();
      centerCharacterPresenter.ClearView();
      rightCharacterPresenter.ClearView();
    }

    public async UniTask PlayCharacterDataAsync(DialogueTalkingData talkingData, bool isFirstTalk = false)
    {
      inputMoveCTS.Cancel();

      dialogueCTS.Cancel();
      dialogueCTS.Create();
      var token = dialogueCTS.token;
      try
      {
        foreach (var image in model.inputEnableImages)
        {
          image.rectTransform.anchoredPosition = Vector3.zero;
          image.SetAlpha(uiSO.Dialogue.InputWaitingAlpha);
        }          

        isTalkling = true;
        await UniTask.WhenAll(
          leftCharacterPresenter.PlayCharacterDataAsync(talkingData.left, isFirstTalk),
          centerCharacterPresenter.PlayCharacterDataAsync(talkingData.center, isFirstTalk),
          rightCharacterPresenter.PlayCharacterDataAsync(talkingData.right, isFirstTalk))
          .AttachExternalCancellation(token);
              
        token.ThrowIfCancellationRequested();
      }
      catch (OperationCanceledException)
      {
        leftCharacterPresenter.CompleteDialogueImmedieately();
        centerCharacterPresenter.CompleteDialogueImmedieately();
        rightCharacterPresenter.CompleteDialogueImmedieately();
      }
      finally
      {
        inputMoveCTS.Create();
        var inputMoveToken = inputMoveCTS.token;
        foreach (var image in model.inputEnableImages)
        {
          image.SetAlpha(1.0f);
          DOTween
              .Sequence()
              .Join(image.rectTransform.DOAnchorPosY(uiSO.Dialogue.InputGuideMoveLength, uiSO.Dialogue.InputGuideMoveDuration))
              .Append(image.rectTransform.DOAnchorPosY(0.0f, uiSO.Dialogue.InputGuideMoveDuration))
              .SetLoops(-1)
              .ToUniTask(TweenCancelBehaviour.Kill, inputMoveToken).Forget();
        }
        isTalkling = false;
      }
    }

    public void EnableTalkingInputs()
    {
      dialogueInputActionController.SubscribeInputActions();
      inputPresenter.ActivateAsync(true).Forget();
    }

    public async UniTask DisalbeTalkingInputsAsync()
    {
      dialogueInputActionController.UnsubscribeInputActions();
      await inputPresenter.DeactivateAsync();
    }

    public void ClearTexts()
    {
      leftCharacterPresenter.ClearText();
      centerCharacterPresenter.ClearText();
      rightCharacterPresenter.ClearText();
    }

    public void UpdateShadow(DialogueDataEnum.Background.Shadow shadowType)
      => dialogueBackgroundPresenter.UpdateShadow(shadowType);

    public void UpdateShake(bool isShake)
      => dialogueBackgroundPresenter.UpdateShake(isShake);

    private void OnNextInput()
    {
      if (isTalkling)
      {
        dialogueCTS.Cancel();
      }
      else
      {
        model.onNextTalk();
      }
    }

    public void Dispose()
    {
      disposables.Dispose();
      dialogueCTS.Dispose();
      inputMoveCTS.Dispose();
    }
  }
}
