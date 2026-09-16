using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Table.Dialogue;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LR.UI.GameScene.Dialogue.Root
{
  public class DialogueInputActionController : IDisposable
  {
    public class InputActionModel
    {
      public readonly UnityAction onLeftPerformed;
      public readonly UnityAction onLeftCanceled;
      public readonly UnityAction onRightPerformed;
      public readonly UnityAction onRightCanceled;

      public readonly UnityAction onNextInput;

      public readonly UnityAction onSkipPerformed;
      public readonly UnityAction<float> onSkipProgress;
      public readonly UnityAction onSkipCanceled;
      public readonly UnityAction onSkip;

      public InputActionModel(UnityAction onLeftPerformed, UnityAction onLeftCanceled, UnityAction onRightPerformed, UnityAction onRightCanceled, UnityAction onNextInput, UnityAction onSkipPerformed, UnityAction<float> onSkipProgress, UnityAction onSkipCanceled, UnityAction onSkip)
      {
        this.onLeftPerformed = onLeftPerformed;
        this.onLeftCanceled = onLeftCanceled;
        this.onRightPerformed = onRightPerformed;
        this.onRightCanceled = onRightCanceled;
        this.onNextInput = onNextInput;
        this.onSkipPerformed = onSkipPerformed;
        this.onSkipProgress = onSkipProgress;
        this.onSkipCanceled = onSkipCanceled;
        this.onSkip = onSkip;
      }
    }
    private readonly IInputActionSubscriber inputActionSubscriber;
    private readonly UITextPresentationData textPresentationData;
    private readonly InputActionModel model;

    private readonly CTSContainer skipCTS = new();

    private bool isSubscribed = false;

    public DialogueInputActionController(
      IInputActionSubscriber inputActionSubscriber,
      DialogueUIDataSO dialogueUIDataSO, 
      InputActionModel model)
    {
      this.inputActionSubscriber = inputActionSubscriber;
      this.textPresentationData = dialogueUIDataSO.TextPresentationData;
      this.model = model;
    }

    public void Dispose()
    {
      if(isSubscribed)
        UnsubscribeInputActions();
    }

    public void SubscribeInputActions()
    {
      if (isSubscribed)
        return;

      inputActionSubscriber.SubscribePhase(LRInputType.LeftAny, OnLeftPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.LeftAny, OnLeftCanceled, InputPhase.Canceled);
      inputActionSubscriber.SubscribePhase(LRInputType.RightAny, OnRightPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.RightAny, OnRightCanceled, InputPhase.Canceled);
      inputActionSubscriber.SubscribePhase(LRInputType.DialogueSkip, OnSkipPerformed, InputPhase.Performed);
      inputActionSubscriber.SubscribePhase(LRInputType.DialogueSkip, OnSkipCanceled, InputPhase.Canceled);      

      isSubscribed = true;
    }

    public void UnsubscribeInputActions()
    {
      if (isSubscribed == false)
        return;

      inputActionSubscriber.UnsubscribePhase(LRInputType.LeftAny, OnLeftPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.LeftAny, OnLeftCanceled, InputPhase.Canceled);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightAny, OnRightPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.RightAny, OnRightCanceled, InputPhase.Canceled);
      inputActionSubscriber.UnsubscribePhase(LRInputType.DialogueSkip, OnSkipPerformed, InputPhase.Performed);
      inputActionSubscriber.UnsubscribePhase(LRInputType.DialogueSkip, OnSkipCanceled, InputPhase.Canceled);

      isSubscribed = false;
    }

    private void OnLeftPerformed()
    {
      model.onLeftPerformed?.Invoke();
      model.onNextInput?.Invoke();
    }

    private void OnLeftCanceled()
    {
      model.onLeftCanceled?.Invoke();
    }

    private void OnRightPerformed()
    {
      model.onRightPerformed?.Invoke();
      model.onNextInput?.Invoke();
    }

    private void OnRightCanceled()
    {
      model.onRightCanceled?.Invoke();
    }

    private void OnSkipPerformed()
    {
      model.onSkipPerformed?.Invoke();
      skipCTS.Dispose();
      skipCTS.Create();
      SkipAsync(skipCTS.token).Forget();
    }

    private void OnSkipCanceled()
    {
      model.onSkipCanceled?.Invoke();
      skipCTS.Cancel();
    }

    private async UniTask SkipAsync(CancellationToken token)
    {
      var duration = 0.0f;
      try
      {
        while(duration < textPresentationData.SkipInputDuration)
        {
          token.ThrowIfCancellationRequested();
          model.onSkipProgress?.Invoke(duration / textPresentationData.SkipInputDuration);

          duration += Time.deltaTime;
          await UniTask.Yield();
        }
        model.onSkipProgress?.Invoke(1.0f);
        model.onSkip?.Invoke();
      }
      catch (OperationCanceledException)
      {
        model.onSkipProgress?.Invoke(0.0f);
      }
    }
  }
}
