using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Stage.Player.GimmickGuide;
using LR.Table.StageGimmick;
using LR.UI;
using LR.UI.GameScene.StageGimmick;
using System;
using System.Collections.Generic;
using System.Threading;
using Zenject;

namespace LR.Manager.Stage.Gimmick
{
  public class QTEBomb : IStageGimmick
  {
    private class CountSet
    {
      public bool IsComplete => current == target;

      public LRInputType inputType;
      public int target;
      public int current = 0;

      public CountSet(LRInputType inputType, int target)
      {
        this.inputType = inputType;
        this.target = target;
      }

      public bool TryAdd()
      {
        if(current < target)
        {
          current++;
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    [Inject] private readonly ICameraEffectService cameraEffectService = null;
    [Inject] private readonly IStageStateProvider stageStateProvider = null;    
    [Inject] private readonly QTEBombData data = null;
    [Inject] private readonly ISFXController sfxController = null;

    private readonly InputActionSet inputActionSet;
    private readonly CTSContainer bombCTS = new();
    private bool isRunning = false;
    private float completeDuration = 0.0f;
    private bool isLeftQTE = true;
    private readonly List<CountSet> countSets = new();

    private readonly QTEGuideView leftGuideView;
    private readonly QTEGuideView rightGuideView;
    private readonly IPlayerMoveController leftMoveController;
    private readonly IPlayerMoveController rightMoveController;
    private readonly IPlayerReactionController leftReactionController;
    private readonly IPlayerReactionController rightReactionController;

    private UIQTEBombPresenter presenter;    
    private AudioLoopHandle selectingAudioHandle;

    public QTEBomb(
      IPlayerGetter playerGetter,
      IInputActionSubscriber inputActionSubscriber)
    {
      this.leftReactionController = playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Left).GetReactionController();
      this.rightReactionController = playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Right).GetReactionController();

      var leftPlayer = playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Left);
      var rightPlayer = playerGetter.GetPlayer(LR.Stage.Player.Enum.PlayerType.Right);
      leftGuideView = leftPlayer.GetQTEGuideView();
      rightGuideView = rightPlayer.GetQTEGuideView();
      leftMoveController = leftPlayer.GetMoveController();
      rightMoveController = rightPlayer.GetMoveController();

      isLeftQTE = UnityEngine.Random.Range(0, 2) == 0;

      inputActionSet = new(
        inputActionSubscriber,
        OnLeftPerformed,
        OnRightPerformed);
    }

    private QTEGuideView GetQTEGuideView(PlayerType playerType)
      => playerType switch
      {
        PlayerType.Left => leftGuideView,
        PlayerType.Right => rightGuideView,
        _ => throw new NotImplementedException(),
      };

    public void Begin()
    {
      inputActionSet.Enable(true);
      presenter.PlayRandomInput(isLeftQTE);
      bombCTS.Cancel();
      bombCTS.Create();
      var token = bombCTS.token;
      UpdateBombAsync(token).Forget();
    }

    public void Complete()
    {
      selectingAudioHandle?.Stop();
      selectingAudioHandle = null;
      inputActionSet.Enable(false);
      bombCTS.Cancel();
    }

    public void Dispose()
    {
      selectingAudioHandle?.Dispose();
      bombCTS.Dispose();
      inputActionSet.Dispose();
    }

    public void InjectUI(IUIPresenter presenter)
    {
      this.presenter = presenter as UIQTEBombPresenter;
      this.presenter.PlayRandomInput(isLeftQTE);
    }

    public void Pause()
    {
      inputActionSet.Enable(false);      
    }

    public void Restart()
    {
      isLeftQTE = UnityEngine.Random.Range(0, 2) == 0;
      inputActionSet.Enable(false);
      presenter.UpdateDuration(0.0f);
      presenter.PlayRandomInput(isLeftQTE);
      countSets.Clear();
      leftGuideView.ClearIcons();
      rightGuideView.ClearIcons();
      bombCTS.Cancel();
    }

    public void Resume()
    {
      inputActionSet.Enable(true);
    }

    private async UniTask UpdateBombAsync(CancellationToken token)
    {
      try
      {        
        await WaitRegenAsync(token);
        ResetInpuData();

        while (true)
        {          
          presenter.BeginInput();

          isRunning = true;
          while (completeDuration > 0.0f && !IsAllQTEComplete())
          {
            token.ThrowIfCancellationRequested();

            if(!stageStateProvider.IsPlayingState)
            {
              await UniTask.Yield();
              continue;
            }

            var targetMoveController = isLeftQTE ? leftMoveController : rightMoveController;
            var decreaseDuration = UnityEngine.Time.deltaTime * (targetMoveController.IsConveyorCrossing() ? data.ConveyorCrossingDurationRatio : 1.0f);

            completeDuration -= decreaseDuration;

            var t = completeDuration / data.CompleteDuration;
            presenter.UpdateDuration(t);
            leftGuideView.UpdateDuration(t);
            rightGuideView.UpdateDuration(t);

            await UniTask.Yield();
          }
          isRunning = false;

          if (completeDuration < 0.0f)
          {
            FailSequence();
          }
          else
          {
            SuccessSequence();
          }

          isLeftQTE = !isLeftQTE;
          leftGuideView.ClearIcons();
          rightGuideView.ClearIcons();
          selectingAudioHandle?.Stop();
          selectingAudioHandle = null;
          presenter.PlayRandomInput(isLeftQTE);
          await WaitRegenAsync(token);          

          ResetInpuData();
        }
      }
      catch (OperationCanceledException) { }
    }

    private bool IsAllQTEComplete()
    {
      foreach (var countSet in countSets)
        if (!countSet.IsComplete)
          return false;

      return true;
    }

    private async UniTask WaitRegenAsync(CancellationToken token)
    {
      var waitDuration = 0.0f;
      while (waitDuration < data.CompleteWaitDuration)
      {
        token.ThrowIfCancellationRequested();
        if (!stageStateProvider.IsPlayingState)
        {
          await UniTask.Yield();
          continue;
        }

        waitDuration += UnityEngine.Time.deltaTime;
        presenter.UpdateDuration(waitDuration / data.CompleteWaitDuration);
        await UniTask.Yield();
      }
      presenter.UpdateDuration(1.0f);
    }

    private void OnLeftPerformed(Direction direction)
    {
      if (!isRunning)
        return;

      switch (direction)
      {
        case Direction.Up:
          OnInputPerfomed(LRInputType.LeftUp);
          break;

        case Direction.Right:
          OnInputPerfomed(LRInputType.LeftRight);
          break;

        case Direction.Down:
          OnInputPerfomed(LRInputType.LeftDown);
          break;

        case Direction.Left:
          OnInputPerfomed(LRInputType.LeftLeft);
          break;
      }
    }

    private void OnRightPerformed(Direction direction) 
    {
      if (!isRunning)
        return;

      switch (direction)
      {
        case Direction.Up:
          OnInputPerfomed(LRInputType.RightUp);
          break;

        case Direction.Right:
          OnInputPerfomed(LRInputType.RightRight);
          break;

        case Direction.Down:
          OnInputPerfomed(LRInputType.RightDown);
          break;

        case Direction.Left:
          OnInputPerfomed(LRInputType.RightLeft);
          break;
      }
    }

    private void OnInputPerfomed(LRInputType inputDirection)
    {
      foreach (var countSet in countSets)
      {
        if(countSet.inputType == inputDirection)
        {          
          if (countSet.TryAdd())
          {
            completeDuration += data.InputAdditionalDuration;

            var index = countSet.current - 1;
            presenter.OnSuccessSingleQTE(inputDirection.ParseToDirection(), index);
            GetQTEGuideView(inputDirection.IsLeft() ? PlayerType.Left : PlayerType.Right).OnSuccessSingleQTE(inputDirection, index);

            var isLeft = inputDirection.IsLeft();
            sfxController.PlayOnce(
              isLeft ? AudioSourceType.Left : AudioSourceType.Right,
              isLeft ? SFX.QTEBombLeftSelected : SFX.QTEBombRightSelected);
          }
          return;
        }
      }
    }

    private void ResetInpuData()
    {
      var count = UnityEngine.Random.Range(2, 5);
      var targetInputTypes = isLeftQTE ? LRInputTypeUtil.GetRandomLefts(count)
                                       : LRInputTypeUtil.GetRandomRights(count);
      var targetInputCounts = Split(data.SumMaxCount, count);
      completeDuration = data.CompleteDuration;

      presenter.UpdateDuration(1.0f);

      sfxController.PlayOnce(
        isLeftQTE ? AudioSourceType.Left : AudioSourceType.Right,
        isLeftQTE ? SFX.QTEBombLeftSelected : SFX.QTEBombRightSelected);
      
      presenter.ClearIcons();
      countSets.Clear();
      leftGuideView.ClearIcons();
      rightGuideView.ClearIcons();
      var targetGuideView = GetQTEGuideView(isLeftQTE ? PlayerType.Left : PlayerType.Right);

      for(int i = 0; i < count; i++)
      {
        var targetCount = targetInputCounts[i];
        var inputType = targetInputTypes[i];
        countSets.Add(new(inputType, targetCount));

        presenter.UpdateTargetInput(inputType, targetCount);
        targetGuideView.UpdateIconCount(inputType, targetCount);
      }
      
      presenter.UpdateLayout();      
    }

    public static List<int> Split(int n, int k)
    {
      var result = new List<int>();

      if (n < k || k <= 0)
        return result; // 불가능

      // 컷 k-1개 생성
      var cuts = new List<int>();
      for (int i = 0; i < k - 1; i++)
      {
        cuts.Add(UnityEngine.Random.Range(1, n)); // [1, n-1]
      }

      // 정렬
      cuts.Sort();

      int prev = 0;

      // 구간 분해
      foreach (var cut in cuts)
      {
        result.Add(cut - prev);
        prev = cut;
      }

      // 마지막 구간
      result.Add(n - prev);

      return result;
    }

    private void SuccessSequence()
    {      
      presenter.OnSuccessQTE();      
    }

    private void FailSequence()
    {
      presenter.OnFail();
      leftReactionController.Stun();
      rightReactionController.Stun();

      cameraEffectService.GenerateImpulse(ICameraEffectService.ImpulseType.GimmickStun);
      cameraEffectService.PlayGimmickStunChromaticAsync();

      sfxController.PlayOnce(AudioSourceType.Center, SFX.GimmickExplosion);
    }
  }
}
