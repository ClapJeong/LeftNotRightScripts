using Cysharp.Threading.Tasks;
using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Manager.Stage.Gimmick.SwapGimmick;
using LR.Stage.Player.GimmickGuide;
using LR.Table.StageGimmick;
using LR.UI;
using LR.UI.GameScene.StageGimmick;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.Manager.Stage.Gimmick
{
  public class Swap : IStageGimmick
  {
    private enum State
    {
      Wait,
      Swap,
    }

    private readonly SwapData data;
    private readonly IStageStateProvider stageStateProvider;
    private readonly IPlayerGetter playerGetter;
    [Inject] private readonly ISFXController sfxController = null;
    [Inject] private readonly ICameraEffectService cameraEffectService = null;
    [Inject] private readonly IInputQTEService inputQTEService = null;
    [Inject] private readonly IInputProgressService inputProgressService = null;

    private readonly CTSContainer CTS = new();
    private readonly TimeScaler timeScaler;
    private readonly List<SwapGuideView> guideViews = new();
    private State state = State.Wait;
    private float duration;
    private int fastClockCount = 0;
    private UISwapPresenter presenter;    

    public Swap(
      SwapData data,
      IStageStateProvider stageStateProvider,
      IPlayerGetter playerGetter)
    {
      this.data = data;
      this.stageStateProvider = stageStateProvider;
      this.playerGetter = playerGetter;
      this.timeScaler = new(data, stageStateProvider);

      guideViews.Add(playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Left).GetSwapGuideView());
      guideViews.Add(playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Right).GetSwapGuideView());
    }

    public void Begin()
    {
      timeScaler.StopImmedieatley();
      duration = 0.0f;
      SetState(State.Wait, true);

      CTS.Cancel();
      CTS.Create();
      var token = CTS.token;
      UpdateAsync(token).Forget();
    }

    public void Complete()
    {
      timeScaler.StopImmedieatley();
    }

    public void Dispose()
    {
      timeScaler.Dispose();
      CTS.Dispose();
    }

    public void InjectUI(IUIPresenter presenter)
    {
      this.presenter = presenter as UISwapPresenter;
    }

    public void Pause()
    {
      timeScaler.OnPause();
    }

    public void Restart()
    {
      timeScaler.StopImmedieatley();
      duration = 0.0f;
      presenter.UpdateFill(1.0f);
      SetState(State.Wait, true);
    }

    public void Resume()
    {
      timeScaler.OnResume();
    }

    private async UniTask UpdateAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();
          if(!stageStateProvider.IsPlayingState)
          {
            await UniTask.Yield();
            continue;
          }

          switch (state)
          {
            case State.Wait: OnWait(); break;

            case State.Swap: OnSwap(); break;
          }

          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException)
      {

      }
    }

    private void OnWait()
    {
      duration += Time.deltaTime;
      var normalized = 1.0f - (duration / data.SwapCooldownDuration);
      presenter.UpdateFill(normalized);
      foreach (var guideView in guideViews)
        guideView.OnDuration(normalized);

      if (duration >= data.SwapCooldownDuration)
      {
        duration = data.SwapDuration;
        SetState(State.Swap);
      }
      else
      {
        var targetDuration = data.SwapCooldownDuration
            - data.ClockInterval * (data.ClockCount * 2.0f + data.FastClockCount - fastClockCount) * 0.5f;
        if (duration >= targetDuration)
        {
          var isFastCount = fastClockCount > data.ClockCount;
          sfxController.PlayOnce(AudioSourceType.Center, isFastCount ? SFX.Clock_1 : SFX.Clock_0);

          fastClockCount += isFastCount ? 1
                                        : 2;

          foreach (var guideView in guideViews)
            guideView.PumpAsync().Forget();
        }
      }
    }

    private void OnSwap()
    {
      duration -= Time.deltaTime;
      var normalized = duration / data.SwapDuration;
      presenter.UpdateFill(normalized);
      foreach (var guideView in guideViews)
        guideView.OnDuration(normalized);

      if (duration <= 0.0f)
      {
        duration = 0.0f;
        SetState(State.Wait);
      }
      else
      {
        var targetDuration = data.ClockInterval * (data.ClockCount * 2.0f + data.FastClockCount - fastClockCount) * 0.5f;
        if (duration <= targetDuration)
        {
          var isFastCount = fastClockCount > data.ClockCount;
          sfxController.PlayOnce(AudioSourceType.Center, isFastCount ? SFX.Clock_0 : SFX.Clock_1);

          fastClockCount += isFastCount ? 1
                                        : 2;

          foreach (var guideView in guideViews)
            guideView.PumpAsync().Forget();
        }
      }
    }

    private void SetState(State state, bool isImmedieately = false)
    {
      if (this.state == state)
        return;

      fastClockCount = 0;
      this.state = state;

      var isSwapped = state == State.Swap;
      foreach (var guideView in guideViews)
        guideView.Swap(isSwapped, isImmedieately);
      presenter.Swap(isSwapped, isImmedieately);
      inputQTEService.OnSwapped(isSwapped);
      inputProgressService.OnSwapped(isSwapped);
      SwapInputs();

      if (!isImmedieately)
      {
        timeScaler.PlaySlow();
        sfxController.PlayOnce(AudioSourceType.CenterUI,
          state switch
          {
            State.Wait => SFX.SwapReverted,
            State.Swap => SFX.SwapChanged,
            _ => throw new NotImplementedException(),
          });
        cameraEffectService.PlaySwapChromaticAsync().Forget();
      }
    }

    private void SwapInputs()
    {
      var leftPresenter = playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Left);
      var rightPresenter = playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Right);

      var leftInputController = leftPresenter.GetInputActionController();
      var rightInputController = rightPresenter.GetInputActionController();

      leftInputController.RebindToOpposite();
      rightInputController.RebindToOpposite();
    }
  }
}
