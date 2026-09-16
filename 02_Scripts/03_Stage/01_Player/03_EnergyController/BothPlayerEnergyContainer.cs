using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.Player.Enum;
using LR.Table.Player;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using static LR.Stage.Player.IPlayerEnergySubscriber;

namespace LR.Stage.Player
{
  public class BothPlayerEnergyContainer:
    IPlayerEnergyController,
    IPlayerEnergySubscriber,
    IPlayerEnergyProvider,
    IDisposable
  {
    [Inject] private readonly IStageResultHandler stageResultHandler = null;
    [Inject] private readonly IStageStateProvider stageStateProvider = null;
    [Inject] private readonly PlayerEnergyDataSO playerEnergyData = null;
    [Inject] private readonly IBGMController bgmController = null;
    [Inject] private readonly IDifficultyService difficultyService = null;
    [Inject] private readonly IStageFailDataProvider deathCountProvider = null;
    [Inject] private readonly GlobalModifierSO globalModifier = null;
    [Inject] private readonly IPlayerGetter playerGetter = null;
    [Inject] private readonly IStageRecorderService stageRecorder = null;
    [Inject] private readonly IPracticeService practiceService = null;
    [Inject] private readonly IGameDataProvider gameDataProvider = null;
    private readonly float initializedStageMaxEnergy;

    private readonly Dictionary<StateEvent, UnityEvent> stateEvents = new();
    private readonly Dictionary<ValueEvent, UnityEvent<float>> valueEvents = new();
    private readonly UnityEvent<PlayerType, DamageType> onPlayerHit = new();
    private readonly UnityEvent<PlayerType, float> onPlayerDamaged = new();
    private readonly Dictionary<float, UnityEvent> aboveEvents = new();
    private readonly Dictionary<float, UnityEvent> belowEvents = new();

    private IPlayerStateProvider leftStateProvider;
    private IPlayerStateProvider rightStateProvider;

    private readonly IDisposable updateDisposable;

    public float StageMaxEnergy
    {
      get
      {
        var initialize = initializedStageMaxEnergy;
        var failBonus = (deathCountProvider.FailCount / playerEnergyData.DeathStep) * playerEnergyData.DeathBonusValue;
        var modifier = globalModifier.StageEnergyOffset;

        var chapter = gameDataProvider.GetSelectedChapter();
        var stage = gameDataProvider.GetSelectedStage();
        var isClearStage = gameDataProvider.IsClearStage(chapter, stage, out var _);

        var maxEnergy = initialize + modifier;
        if (!isClearStage)
          maxEnergy += failBonus;

        return maxEnergy;
      }
    }
    private float leftEnergy;
    private float rightEnergy;
    private float prevTotalEnergy;
    private float currentTotalEnergy;

    public BothPlayerEnergyContainer(
      LocalManager localManager,
      float initializedStageMaxEnergy)
    {
      this.initializedStageMaxEnergy = initializedStageMaxEnergy;

      prevTotalEnergy = initializedStageMaxEnergy;
      currentTotalEnergy = initializedStageMaxEnergy;
      leftEnergy = initializedStageMaxEnergy * 0.5f;
      rightEnergy = initializedStageMaxEnergy * 0.5f;

      updateDisposable =
        localManager
        .gameObject
        .UpdateAsObservable()
        .Subscribe(_ => UpdateEnergy(Time.deltaTime));
    }

    public void InitializePlayers()
    {
      leftStateProvider = playerGetter.GetPlayer(PlayerType.Left).GetStateProvider();
      rightStateProvider = playerGetter.GetPlayer(PlayerType.Right).GetStateProvider();
    }

    public void Dispose()
    {
      updateDisposable?.Dispose();
    }

    #region IPlayEnergyProvider
    private bool IsEnergyWorking => stageStateProvider.IsPlayingState;

    public bool IsDead => TotalEnergy <= 0.0f;

    public bool IsFull => TotalEnergy >= StageMaxEnergy;

    public float TotalEnergy => Mathf.Max(0.0f, leftEnergy + rightEnergy);

    public float LeftEnergy => leftEnergy;

    public float RightEnergy => rightEnergy;

    public float TotalNormalized => TotalEnergy / StageMaxEnergy;

    public float LeftEnergyNormalized => leftEnergy / StageMaxEnergy;

    public float RightEnergyNormalized => rightEnergy / StageMaxEnergy;
    #endregion


    #region IPlayerEnergyController
    public void Damage(PlayerType hitPlayer, float value, DamageType damageType, bool ignoreInvincible = false)
    {
      if (practiceService.IsPractice)
        return;
      if (!IsEnergyWorking)
        return;
      if (IsDead)
        return;

      stageRecorder.OnPlayerDamaged(hitPlayer, damageType, value);

      var beforeNormalized = TotalNormalized;

      switch (hitPlayer)
      {
        case PlayerType.Left: 
          leftEnergy -= value;
          valueEvents.TryInvoke(ValueEvent.LeftDamaged, beforeNormalized - TotalNormalized);
          break;

        case PlayerType.Right: 
          rightEnergy -= value;
          valueEvents.TryInvoke(ValueEvent.RightDamaged, beforeNormalized - TotalNormalized);
          break;
      }

      valueEvents.TryInvoke(ValueEvent.AnyDamaged, beforeNormalized - TotalNormalized);

      onPlayerHit?.Invoke(hitPlayer, damageType);
      onPlayerDamaged?.Invoke(hitPlayer, value);

      if (IsDead)
        OnExhauset();
    }

    public void Restart()
    {
      prevTotalEnergy = StageMaxEnergy;
      currentTotalEnergy = prevTotalEnergy;
      leftEnergy = currentTotalEnergy * 0.5f;
      rightEnergy = currentTotalEnergy * 0.5f;
    }
#endregion

    #region IPlayerEnergyUpdater
    public void UpdateEnergy(float deltaTime)
    {
      if (IsDead || !IsEnergyWorking || practiceService.IsPractice)
        return;

      prevTotalEnergy = TotalEnergy;
      var decreaseValue = difficultyService.CurrentDifficulty == IDifficultyService.Difficulty.Normal ? playerEnergyData.NormalDecreasingValue
                                                                                                      : playerEnergyData.EasyDecreasingValue;
      decreaseValue *= deltaTime * 0.5f;
      var leftDecrease = leftStateProvider.GetCurrentState() == PlayerState.Move ? decreaseValue
                                                                                 : decreaseValue * playerEnergyData.StopModifier;
      var rightDecrease = rightStateProvider.GetCurrentState() == PlayerState.Move ? decreaseValue
                                                                                   : decreaseValue * playerEnergyData.StopModifier;

      leftEnergy -= leftDecrease;
      rightEnergy -= rightDecrease;

      stageRecorder.AddRunningTimer(leftDecrease + rightDecrease);

      var currentEnergy = Mathf.Max(0.0f, TotalEnergy);

      var prevNormalized = prevTotalEnergy / currentTotalEnergy;
      var currentNormalized = currentEnergy / currentTotalEnergy;
      if (currentNormalized < prevNormalized)
      {
        foreach (var pair in belowEvents)
          if (IsCrossedDown(prevNormalized, currentNormalized, pair.Key))
            pair.Value?.Invoke();
      }
      else if (currentNormalized > prevNormalized)
      {
        foreach (var pair in aboveEvents)
          if (IsCrossedUp(prevNormalized, currentNormalized, pair.Key))
            pair.Value?.Invoke();
      }

      bgmController.UpdateStero(0.5f + (LeftEnergyNormalized - RightEnergyNormalized));

      if (IsDead)
        OnExhauset();
    }

    #endregion

    #region IPlayerEnergySubscriber
    public void SubscribeStateEvent(IPlayerEnergySubscriber.StateEvent type, UnityAction action)
      => stateEvents.AddEvent(type, action);

    public void UnsubscribeStateEvent(IPlayerEnergySubscriber.StateEvent type, UnityAction action)
      => stateEvents.RemoveEvent(type, action);

    public void SubscribeThreshhold(Threshhold type, float normalizedValue, UnityAction action)
    {
      switch (type)
      {
        case Threshhold.Above:
          aboveEvents.AddEvent(normalizedValue, action);
          break;

        case Threshhold.Below:
          belowEvents.AddEvent(normalizedValue, action);
          break;
      }
    }

    public void UnsubscribeThreshhold(Threshhold type, float normalizedValue, UnityAction action)
    {
      switch (type)
      {
        case Threshhold.Above:
          aboveEvents.RemoveEvent(normalizedValue, action);
          break;

        case Threshhold.Below:
          belowEvents.RemoveEvent(normalizedValue, action);
          break;
      }
    }

    private static bool IsCrossedDown(float prev, float curr, float normalized)
       => prev > normalized && curr <= normalized;

    private static bool IsCrossedUp(float prev, float curr, float normalized)
      => prev < normalized && curr >= normalized;

    public void SubscribeValueEvent(ValueEvent valueEvent, UnityAction<float> action)
      => valueEvents.AddEvent(valueEvent, action);

    public void UnsubscribeValueEvent(ValueEvent valueEvent, UnityAction<float> action)
      => valueEvents.RemoveEvent(valueEvent, action);

    public void SubscribeOnHit(UnityAction<PlayerType, DamageType> onHit)
      => onPlayerHit.AddListener(onHit);

    public void UnsubscribeOnHit(UnityAction<PlayerType, DamageType> onHit)
      => onPlayerHit.RemoveListener(onHit);

    public void SubscribeOnDamageValue(UnityAction<PlayerType, float> onDamaged)
      => onPlayerDamaged.AddListener(onDamaged);

    public void UnsbscribeOnDamageValue(UnityAction<PlayerType, float> onDamaged)
      => onPlayerDamaged.RemoveListener(onDamaged);
    #endregion

    private void OnExhauset()
    {
      stageResultHandler.Exhausted();
      stateEvents.TryInvoke(IPlayerEnergySubscriber.StateEvent.OnExhausted);
    }    

    public void DebugEnergy()
    {
    }
  }
}
