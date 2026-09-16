using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Table.Input;
using LR.UI.GameScene.InputProgress;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class InputProgressService : IInputProgressService
{
  [Inject] private readonly IInputActionSubscriber inputActionSubscriber = null;
  [Inject] private readonly IInputProgressUIService uiService = null;
  [Inject] private readonly ISFXController sfxController = null;
  [Inject] private readonly IStageStateProvider stageStateProvider = null;

  private readonly CTSContainer cts = new();

  private bool isPlaying = false;
  private InputProgressData currentData;
  private float value;
  private AudioLoopHandle audioLoopHandle;
  private bool isSwapped = false;
  private ISignalGimmickSwapable uiPresenter;

  public async void Play(
    InputProgressData data,
    Transform followTarget,
    UnityAction<float> onProgress, 
    UnityAction onComplete,
    UnityAction onFail)
  {
    if (isPlaying)
      return;

    cts.Dispose();
    cts.Create();

    audioLoopHandle = sfxController.CreateLoopSource(CharacterPositionType.Right, SFX.CaptchaProgress, data.BeginValue);
    value = data.BeginValue;
    currentData = data;
    var presenter = await uiService.GetPresenterAsync(data.UIType, followTarget);
    presenter.OnSwapped(isSwapped);
    PlayAsync(presenter, onProgress, onComplete, onFail, cts.token).Forget();
  }

  public void Stop()
  {
    audioLoopHandle?.Dispose();
    cts.Cancel();
  }

  public void OnSwapped(bool isSwap)
  {
    if (isPlaying)
    {
      UnsubscribeInputActions();
    }      

    isSwapped = isSwap;
    uiPresenter?.OnSwapped(isSwap);

    if (isPlaying)
      SubscribeInputActions();
  }


  private async UniTask PlayAsync(
    IUIInputProgressPresenter uiPresenter,
    UnityAction<float> onProgress, 
    UnityAction onComplete,
    UnityAction onFail,
    CancellationToken token)
  {
    await uiPresenter.ActivateAsync();

    try
    {
      this.uiPresenter = uiPresenter;
      isPlaying = true;
      SubscribeInputActions();
      while (true)
      {
        if ((currentData.Failable && value <= 0.0f) || value >= 1.0f)
          break;
        token.ThrowIfCancellationRequested();

        if (stageStateProvider.GetState() == StageEnum.State.Pause)
        {
          await UniTask.Yield();
          continue;
        }

        value = Mathf.Max(0.0f, value - currentData.DecreaseValuePerSecond * Time.deltaTime);
        audioLoopHandle.Pitch = value;
        onProgress?.Invoke(value);
        uiPresenter.OnProgress(value);
        await UniTask.Yield();
      }

      if (value >= 1.0f)
      {
        value = 1.0f;
        onProgress?.Invoke(value);
        onComplete?.Invoke();

        uiPresenter.OnProgress(value);
        uiPresenter.OnComplete();
      }        
      else if (value <= 0.0f)
      {
        value = 0.0f;
        onProgress?.Invoke(value);        
        onFail?.Invoke();
        sfxController.PlayOnce(AudioSourceType.Right, SFX.CaptchaFail);

        uiPresenter.OnProgress(value);
        uiPresenter.OnComplete();
      }      
    }
    catch (OperationCanceledException)
    {
      onFail?.Invoke();
    }
    finally
    {
      this.uiPresenter = null;
      audioLoopHandle?.Dispose();
      isPlaying = false;
      UnsubscribeInputActions();
      uiPresenter.DeactivateAsync().Forget();
    }
  }
  
  private void OnPerformed(InputPhase phase)
  {
    if (phase != InputPhase.Performed ||
        !stageStateProvider.IsPlayingState)
      return;

    value += currentData.IncreaseValueOnInput;
  }

  private void SubscribeInputActions()
  {
    var inputTypes = isSwapped ? LRInputTypeUtil.GetLefts() : LRInputTypeUtil.GetRights();
    foreach(var inputType in inputTypes)
      inputActionSubscriber.Subscribe(inputType, OnPerformed);
  }

  private void UnsubscribeInputActions()
  {
    var inputTypes = isSwapped ? LRInputTypeUtil.GetLefts() : LRInputTypeUtil.GetRights();
    foreach (var inputType in inputTypes)
      inputActionSubscriber.Unsubscribe(inputType, OnPerformed);
  }
}
