using Cysharp.Threading.Tasks;
using LR.Table.Input;
using LR.UI.GameScene.InputQTE;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using LR.Manager.Stage;
using LR.Manager.Sound;
using LR.Manager.Input;
using Zenject;

public class InputQTEService : IInputQTEService
{
  private enum QTEResultType
  {
    Success,
    Fail,
    SequenceTimeout,
  }

  private enum PerformedType
  {
    None,
    Target,
    Wrong,
    TimeOut,
  }

  private class DurationData
  {
    public class Duration
    {
      public float max;
      public float current;

      public Duration(float max)
      {
        this.max = max;
        this.current = max;
      }

      public void Reset()
        => current = max;
    }

    public Duration sequence;
    public Duration qte;

    public DurationData(float sequenceMaxDuration, float qteMaxDuration)
    {
      sequence = new(sequenceMaxDuration);
      qte = new(qteMaxDuration);      
    }

    public void ResetQTEDuration()
      => qte.Reset();
  }

  [Inject] private readonly IInputActionSubscriber inputActionSubscriber = null;
  [Inject] private readonly IInputQTEUIService uiService = null;
  [Inject] private readonly ISFXController sfxController = null;
  [Inject] private readonly IStageStateProvider stageStateProvider = null;

  private readonly int ignoreFrame = 80;
  private readonly CTSContainer cts = new(); 
  private bool isPlaying = false;
  private PerformedType performedType = PerformedType.None;
  private InputQTEData currentData;
  private Direction currentDirection;
  private int beginFrame;
  private ISignalGimmickSwapable uiPresenter;
  private bool isSwapped = false;

  public async void Play(
    InputQTEData data, 
    Transform followTarget,
    UnityAction onSuccess,
    UnityAction onFail)
  {
    if (isPlaying)
      return;

    cts.Dispose();
    cts.Create();

    currentData = data;
    var presenter = await uiService.GetPrsenterAsync(data.UIType, followTarget);
    isPlaying = true;
    PlayAsync(presenter, onSuccess, onFail, cts.token).Forget();
  }

  public void Stop()
  {
    if (!isPlaying)
      return;

    cts.Cancel();
  }

  public void OnSwapped(bool isSwap)
  {
    if (isPlaying)
    {
      UnregisterInputAction();      
    }

    isSwapped = isSwap;
    uiPresenter?.OnSwapped(isSwap);

    if (isPlaying)
    {
      RegisterInputAction(currentDirection);      
    }
  }

  private async UniTask PlayAsync(
    IUIInputQTEPresenter presenter, 
    UnityAction onSuccess,
    UnityAction onFail,
    CancellationToken token)
  {
    var isSuccess = false;
    beginFrame = Time.frameCount;
    try
    {
      this.uiPresenter = presenter;
      presenter.OnSwapped(isSwapped);
      await presenter.ActivateAsync();
      presenter.OnSequenceBegin();

      var targetCount = currentData.Count;
      var currentCount = 0;
      var durationData = new DurationData(currentData.SequenceDuration, currentData.QTEDuration);
      var playQTE = true;
      while (playQTE)
      {
        var targetDirection = currentData.GetRandomDirection();
        RegisterInputAction(targetDirection);

        token.ThrowIfCancellationRequested();
        durationData.ResetQTEDuration();
        presenter.OnQTEBegin(targetDirection);
        var qteResult = await PlayQTEAsync(presenter, durationData, token);        
        
        switch (qteResult)
        {
          case QTEResultType.Success:
            {
              currentCount++;

              if(currentCount == targetCount)
              {
                playQTE = false;
                isSuccess = true;
              }
              else
              {
                sfxController.PlayOnce(AudioSourceType.Left,
                currentCount switch
                {
                  1 => SFX.CaptchaQTEFirst,
                  2 => SFX.CaptchaQTESecond,
                  _ => SFX.CaptchaQTESecond,
                });
              }

              presenter.OnQTECountChanged(currentCount);
            }
            break;

          case QTEResultType.Fail:
            {
              sfxController.PlayOnce(AudioSourceType.Left, SFX.CaptchaFail);
              switch (currentData.QTEFailType)
              {
                case InputQTEEnum.QTEFaiResultType.None:
                  {

                  }
                  break;

                case InputQTEEnum.QTEFaiResultType.DecreaseOnlyCount:
                  {
                    currentCount = Mathf.Max(0, currentCount - 1);
                    presenter.OnQTECountChanged(currentCount);
                  }
                  break;

                case InputQTEEnum.QTEFaiResultType.DecreaseCountWithFail:
                  {
                    currentCount = Mathf.Max(0, currentCount - 1);
                    presenter.OnQTECountChanged(currentCount);
                    if(currentCount == 0)
                    {
                      playQTE = false;
                      isSuccess = false;
                    }
                  }
                  break;

                case InputQTEEnum.QTEFaiResultType.FailSequence:
                  {
                    playQTE = false;
                    isSuccess = false;
                  }
                  break;                
              }
            }
            break;

          case QTEResultType.SequenceTimeout:
            {
              playQTE = false;
              isSuccess = false;
            }
            break;
        }

        UnregisterInputAction();
      }
    }
    catch (OperationCanceledException)
    {
      UnregisterInputAction();
    }
    finally
    {
      this.uiPresenter = null;
      isPlaying = false;

      if (isSuccess)
        onSuccess?.Invoke();
      else
        onFail?.Invoke();
      presenter.OnSequenceResult(isSuccess);
      presenter.DeactivateAsync().Forget();
    }
  }

  private async UniTask<QTEResultType> PlayQTEAsync(
    IUIInputQTEPresenter presenter,
    DurationData durationData,
    CancellationToken token)
  {
    performedType = PerformedType.None;

    var hasSequence = durationData.sequence.max > 0f;
    var hasQTE = durationData.qte.max > 0f;
    var qteTimeout = false;
    try
    {
      while (performedType == PerformedType.None &&
             (hasQTE && !qteTimeout))
      {
        token.ThrowIfCancellationRequested();
        if (stageStateProvider.GetState() == StageEnum.State.Pause)
        {
          await UniTask.Yield();
          continue;
        }

        float delta = Time.deltaTime;

        if (hasSequence)
        {
          durationData.sequence.current -= delta;
          durationData.sequence.current = Mathf.Max(durationData.sequence.current, 0f);
        }

        if (hasQTE)
        {
          durationData.qte.current -= delta;
          durationData.qte.current = Mathf.Max(durationData.qte.current, 0f);          

          presenter.OnQTEProgress(durationData.qte.current / durationData.qte.max);

          qteTimeout = durationData.qte.current <= 0f;
        }

        await UniTask.Yield(PlayerLoopTiming.Update);
      }

      return CompleteQTE(presenter);
    }
    catch (OperationCanceledException)
    {
      presenter.OnQTEResult(false);
      return QTEResultType.Fail;
    }
  }

  private QTEResultType CompleteQTE(IUIInputQTEPresenter presenter)
  {
    QTEResultType result;

    if (performedType == PerformedType.Target)
      result = QTEResultType.Success;
    else
      result = QTEResultType.Fail;

    presenter.OnQTEResult(result == QTEResultType.Success);

    return result;
  }

  private void RegisterInputAction(Direction targetDirection)
  {
    foreach(Direction direction in System.Enum.GetValues(typeof(Direction)))
    {
      var targetLRInputType = isSwapped ? direction.ParseToRightInputActionType() : direction.ParseToLeftInputActionType();
      if(direction == targetDirection)
        inputActionSubscriber.SubscribePhase(targetLRInputType, OnTargetPerformed, InputPhase.Performed);
      else
        inputActionSubscriber.SubscribePhase(targetLRInputType, OnWrongPerformed, InputPhase.Performed);
    }    
    currentDirection = targetDirection;
  }

  private void UnregisterInputAction()
  {
    foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
    {
      var targetLRInputType = isSwapped ? direction.ParseToRightInputActionType() : direction.ParseToLeftInputActionType();

      if (direction == currentDirection)
        inputActionSubscriber.UnsubscribePhase(targetLRInputType, OnTargetPerformed, InputPhase.Performed);
      else
        inputActionSubscriber.UnsubscribePhase(targetLRInputType, OnWrongPerformed, InputPhase.Performed);
    }

    performedType = PerformedType.None;
  }

  private void OnTargetPerformed()
  {
    if (!isPlaying || !stageStateProvider.IsPlayingState)
      return;

    performedType = PerformedType.Target;
  }

  private void OnWrongPerformed()
  {
    if (!isPlaying || !stageStateProvider.IsPlayingState)
      return;

    var currentFrame = Time.frameCount;
    if (currentFrame - beginFrame < ignoreFrame)
      return;

    performedType = PerformedType.Wrong;
  }
}
