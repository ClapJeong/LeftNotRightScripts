using Cysharp.Threading.Tasks;
using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Manager.Stage.Gimmick.InputRequires;
using LR.Stage.Player;
using LR.Stage.Player.GimmickGuide;
using LR.Table.StageGimmick;
using LR.UI;
using LR.UI.GameScene.StageGimmick;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.Manager.Stage.Gimmick
{
  public class InputRequire : IStageGimmick
  {
    public class ValueSet
    {
      public enum State
      {
        Moved,
        UnMoved,
        Clear,
      }

      public State CurrentState
      {
        get
        {
          var currentState = playerStateProvider.GetCurrentState();

          if (currentState == LR.Stage.Player.Enum.PlayerState.Clear)
            return State.Clear;

          if (currentState == LR.Stage.Player.Enum.PlayerState.Move &&
              playerMoveController.GetCurrentInputVelocityNormalized() > data.MoveAcceptSpeedNormalized)// playerStatus.DeltaMoveLength > data.MoveAcceptDeltaLength) 컨베이어에서도요지랄인데
            return State.Moved;

          return State.UnMoved;
        }
      }

      public float NormalizedValue => value / data.MaxValue;
      public float value;
      public bool isRegen = false;

      private readonly IPlayerStateProvider playerStateProvider;
      private readonly IPlayerMoveController playerMoveController;
      private readonly PlayerStatus playerStatus;
      private readonly InputRequireData data;      

      public ValueSet(
        IPlayerStateProvider playerStateProvider,
        IPlayerMoveController playerMoveController,
        PlayerStatus playerStatus,
        InputRequireData data)
      {
        this.playerStateProvider = playerStateProvider;
        this.playerMoveController = playerMoveController;
        this.playerStatus = playerStatus;
        this.data = data;
      }
    }

    [Inject] private readonly ICameraEffectService cameraEffectService = null;
    [Inject] private readonly IStageStateProvider stageStateProvider = null;
    [Inject] private readonly ISFXController sfxController = null;
    private readonly IPlayerReactionController leftReactionController;
    private readonly IPlayerStateProvider leftStateProvider;
    private readonly IPlayerReactionController rightReactionController;
    private readonly IPlayerStateProvider rightStateProvider;    
    private readonly InputRequireData data;

    private readonly InputRequireSFXPlayer inputRequireSFXPlayer;
    private readonly InputRequireGuideView leftGuideView;
    private readonly InputRequireGuideView rightGuideView;

    private readonly CTSContainer cts = new();

    private UIInputRequirePresenter presenter;

    private readonly ValueSet leftValueSet;
    private readonly ValueSet rightValueSet;
    private bool isExhaustCalled = false;
    private AudioLoopHandle fuseSFXHandle;

    public InputRequire(
      DiContainer diContainer,
      IPlayerGetter playerGetter,
      InputRequireData data)
    {
      var leftPlayer = playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Left);
      var leftStatus = leftPlayer.GetPlayerStatus();
      this.leftReactionController = leftPlayer.GetReactionController();      
      this.leftStateProvider = leftPlayer.GetStateProvider();
      this.leftGuideView = leftPlayer.GetInputRequireGuideView();

      var rightPlayer = playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Right);
      var rightStatus = rightPlayer.GetPlayerStatus();
      this.rightReactionController = rightPlayer.GetReactionController();
      this.rightStateProvider = rightPlayer.GetStateProvider();
      this.rightGuideView = rightPlayer.GetInputRequireGuideView();

      this.data = data;

      leftValueSet = new(leftStateProvider, leftPlayer.GetMoveController(), leftStatus, data);
      rightValueSet = new(rightStateProvider, rightPlayer.GetMoveController(), rightStatus, data);

      leftValueSet.value = data.BeginValue;
      rightValueSet.value = data.BeginValue;

      this.inputRequireSFXPlayer = diContainer.Instantiate<InputRequireSFXPlayer>(new object[] { leftValueSet, rightValueSet, data });
    }

    public void Begin()
    {
      leftValueSet.value = data.BeginValue;
      rightValueSet.value = data.BeginValue;
      cts.Cancel();
      cts.Create();
      var token = cts.token;
      UpdateValueSetAsync(
        leftValueSet, 
        leftReactionController,
        OnLeftUpdate,
        OnExhausted,
        OnLeftRegenBegin,
        OnLeftRegenComplete,
        token).Forget();
      UpdateValueSetAsync(
        rightValueSet, 
        rightReactionController,
        OnRightUpdate,
        OnExhausted,
        OnRightRegenBegin,
        OnRightRegenComplete,
        token).Forget();

      UpdateFuseVolumeAsync(token).Forget();
      inputRequireSFXPlayer.PlayAsync().Forget();
    }

    private void OnLeftUpdate(float value)
    {
      presenter.OnUpdateLeftFill(value);
      leftGuideView?.UpdateFillAmount(value);
    }

    private void OnLeftRegenBegin()
    {
      presenter.LeftRegenBegin();
      leftGuideView?.UpdateAlpha(data.ObjectRegenAlpha);
    }

    private void OnLeftRegenComplete()
    {
      presenter.LeftRegenComplete();
      leftGuideView?.UpdateAlpha(1.0f);
    }

    private void OnRightUpdate(float value)
    {
      presenter.OnUpdateRightFill(value);
      rightGuideView?.UpdateFillAmount(value);
    }

    private void OnRightRegenBegin()
    {
      presenter.RightRegenBegin();
      rightGuideView?.UpdateAlpha(data.ObjectRegenAlpha);
    }

    private void OnRightRegenComplete()
    {
      presenter.RightRegenComplete();
      rightGuideView?.UpdateAlpha(1.0f);
    }

    public void Complete()
    {
      fuseSFXHandle?.Dispose();
      fuseSFXHandle = null;
      cts.Cancel();
    }

    public void Dispose()
    {
      inputRequireSFXPlayer.Dispose();
      fuseSFXHandle?.Dispose();
      cts.Dispose();
      presenter.Dispose();
    }

    public void InjectUI(IUIPresenter presenter)
    {
      this.presenter = presenter as UIInputRequirePresenter;

      this.presenter.OnUpdateLeftFill(leftValueSet.value / data.MaxValue);
      this.presenter.OnUpdateRightFill(rightValueSet.value / data.MaxValue);
    }

    public void Pause()
    {
    }

    public void Restart()
    {
      inputRequireSFXPlayer.Dispose();
      cts.Cancel();
      leftValueSet.value = data.BeginValue;
      rightValueSet.value = data.BeginValue;

      leftGuideView?.UpdateAlpha(1.0f);
      rightGuideView?.UpdateAlpha(1.0f);
      leftGuideView.UpdateFillAmount(1.0f);
      rightGuideView.UpdateFillAmount(1.0f);

      presenter.LeftRegenComplete();
      presenter.RightRegenComplete();
      presenter.OnUpdateLeftFill(leftValueSet.value / data.MaxValue);
      presenter.OnUpdateRightFill(rightValueSet.value / data.MaxValue);
    }

    public void Resume()
    {

    }

    private void OnExhausted()
    {
      if (isExhaustCalled)
        return;

      fuseSFXHandle?.Dispose();
      fuseSFXHandle = null;
      sfxController.PlayOnce(AudioSourceType.Center, SFX.GimmickExplosion);

      isExhaustCalled = true;

      leftValueSet.value = 0.0f;
      rightValueSet.value = 0.0f;

      presenter.OnRightExhaust();
      presenter.OnLeftExhaust();
      presenter.ShakeUI();

      cameraEffectService.GenerateImpulse(ICameraEffectService.ImpulseType.GimmickStun);
      cameraEffectService.PlayGimmickStunChromaticAsync();
    }

    private async UniTask UpdateFuseVolumeAsync(CancellationToken token)
    {
      try
      {
        while (!token.IsCancellationRequested)
        {
          token.ThrowIfCancellationRequested();

          if (fuseSFXHandle != null)
            fuseSFXHandle.Volume = 1.0f - Mathf.Max(leftValueSet.value, rightValueSet.value);
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException)
      {

      }
    }

    private async UniTask UpdateValueSetAsync(
      ValueSet valueSet, 
      IPlayerReactionController reactionController,
      UnityAction<float> onUpdateValue,
      UnityAction onExhaust,
      UnityAction onRegenBegin,
      UnityAction onRegenComplete,
      CancellationToken token)
    {
      try
      {        
        while (true)
        {
          isExhaustCalled = false;
          valueSet.isRegen = false;

          while (valueSet.value > 0.0f)
          {
            token.ThrowIfCancellationRequested();
            if (!IsRunning())
            {
              await UniTask.Yield();
              continue;
            }
            
            onUpdateValue?.Invoke(valueSet.value / data.MaxValue);

            switch (valueSet.CurrentState)
            {
              case ValueSet.State.Moved:
                valueSet.value = Mathf.Min(data.MaxValue, valueSet.value + data.IncreaseValue * Time.deltaTime);
                break;

              case ValueSet.State.UnMoved:
                valueSet.value -= data.DecreaseValue * Time.deltaTime;
                break;

              case ValueSet.State.Clear:
                break;
            }             

            await UniTask.Yield();
          }

          onExhaust?.Invoke();
          onUpdateValue?.Invoke(0.0f);
          reactionController.Stun();

          var waitDuration = 0.0f;
          while (waitDuration < data.RegenWaitDuration)
          {
            token.ThrowIfCancellationRequested();
            if (!stageStateProvider.IsPlayingState)
            {
              await UniTask.Yield();
              continue;
            }

            waitDuration += Time.deltaTime;
            await UniTask.Yield();
          }

          onRegenBegin?.Invoke();
          var regenDuration = 0.0f;
          valueSet.isRegen = true;
          while (regenDuration < data.RegenDuration)
          {
            token.ThrowIfCancellationRequested();
            if (!stageStateProvider.IsPlayingState)
            {
              await UniTask.Yield();
              continue;
            }
            valueSet.value = Mathf.Lerp(0.0f, data.BeginValue, regenDuration / data.RegenDuration);
            onUpdateValue?.Invoke(valueSet.value / data.MaxValue);

            regenDuration += Time.deltaTime;
            await UniTask.Yield();
          }          
          onRegenComplete?.Invoke();
        }
      }
      catch (OperationCanceledException)
      {
      }
    }

    private bool IsRunning()
      => stageStateProvider.IsPlayingState;
  }
}
