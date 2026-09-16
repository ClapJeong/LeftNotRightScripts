using Cysharp.Threading.Tasks;
using LR.Table.Dialogue;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.UI.GameScene.Dialogue.Root
{
  public class SequenceController : IDisposable
  {
    public class Model
    {
      public GameObject attachTarget;
      public UIDialogueBackgroundView backgroundView;
      public UITalkingCharacterView leftTalkingView;
      public UITalkingCharacterView centerTalkingView;
      public UITalkingCharacterView rightTalkingView;
      public UITalkingInputsView talkingInputView;
      public UnityAction onComplete;
      public int dialogueIndex;
      public bool enableLobbyBackground;

      public Model(
        GameObject attachTarget,
        UIDialogueBackgroundView backgroundView,
        UITalkingCharacterView leftTalkingView, 
        UITalkingCharacterView centerTalkingView, 
        UITalkingCharacterView rightTalkingView, 
        UITalkingInputsView talkingInputView, 
        UnityAction onComplete, 
        int dialogueIndex,
        bool enableLobbyBackground)
      {
        this.attachTarget = attachTarget;
        this.backgroundView = backgroundView;
        this.leftTalkingView = leftTalkingView;
        this.centerTalkingView = centerTalkingView;
        this.rightTalkingView = rightTalkingView;
        this.talkingInputView = talkingInputView;
        this.onComplete = onComplete;
        this.dialogueIndex = dialogueIndex;
        this.enableLobbyBackground = enableLobbyBackground;
      }
    }

    private readonly TalkingController talkingController;
    private readonly UnityAction onComplete;

    private readonly CTSContainer selectionCTS = new();
    private DialogueData currentDialogueData;
    private int sequenceIndex = 0;

    public SequenceController(
      DiContainer diContainer,
      Model model)
    {
      this.onComplete = model.onComplete;

      var inputActionModel = new TalkingController.Model(
        model.attachTarget,
        model.backgroundView,
        model.leftTalkingView,
        model.centerTalkingView,
        model.rightTalkingView,
        model.talkingInputView,
        new() { model.talkingInputView.Left.Idle, model.talkingInputView.Right.Idle },
        OnSkip,
        OnNextTalk,
        model.dialogueIndex,
        model.enableLobbyBackground);
      talkingController = diContainer.Instantiate<TalkingController>(new object[] { inputActionModel });
      talkingController.DeactivateAsync(true, default).Forget();
      talkingController.ClearTexts();
    }

    #region Dialogue Events&Subscribe
    private void OnNextTalk()
    {
      talkingController.ResetInputs();

      var isLastTalking = currentDialogueData.TalkingDatas.Count == sequenceIndex;
      if (isLastTalking == false)
        PlayTalkingData(currentDialogueData.TalkingDatas[sequenceIndex]);
      else
        OnCompleteDialogue();
    }

    private void OnSkip()
    {
      selectionCTS.Cancel();

      OnCompleteDialogue();
    }

    private void OnCompleteDialogue()
    {
      talkingController.DeactivateAsync(false, default).Forget();
      onComplete?.Invoke();
    }
    #endregion

    public async UniTask PlayFirstTalkingDataAsync(DialogueData dialogueData)
    {
      currentDialogueData = dialogueData;

      talkingController.EnableTalkingInputs();
      await talkingController.ActivateAsync(false, default);

      PlayTalkingData(dialogueData.TalkingDatas.First(), isFirstTalk : true);
    }

    private void PlayTalkingData(DialogueTalkingData talkingData, bool isFirstTalk = false)
    {
      talkingController.UpdateShadow(talkingData.Shadow);
      talkingController.UpdateShake(talkingData.BackgroundShake);
      talkingController.PlayCharacterDataAsync(talkingData, isFirstTalk).Forget();

      sequenceIndex++;
    }

    public void Dispose()
    {
      talkingController.Dispose();
    }
  }
}
