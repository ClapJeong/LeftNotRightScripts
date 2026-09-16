using LR.Manager.GameDataManager;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Stage.StageDataContainer;
using LR.Table.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.Manager.Stage
{
  public class StageDataRecordService : 
    IStageRecorderService,
    IStageFailDataSubscriber,
    IStageFailDataProvider
  {

    [Inject] private readonly IGameDataProvider gameDataProvider = null;
    [Inject] private readonly IDifficultyService difficultyService = null;
    [Inject] private readonly PlayerEnergyDataSO playerEnergyDataSO = null;

    private readonly Dictionary<DamageType, List<float>> leftResult = new();
    private readonly Dictionary<DamageType, List<float>> rightResult = new();
    private readonly Dictionary<IStageFailDataSubscriber.DataType, UnityEvent<float>> onDataChangeds = new();

    private float stageEnergy;
    private BothPlayerEnergyContainer energyContainer;

    private IStageRecorderService.SaveResultT saveResult;
    public int FailCount => failCount;
    private int restartCount = 0;
    private int failCount = 0;

    private float runningTime = 0.0f;

    public void InitializeStage(StageDataContainer stageDataContainer, BothPlayerEnergyContainer energyContainer)
    {
      this.stageEnergy = stageDataContainer.GetTotalEnergy(difficultyService.CurrentDifficulty);
      this.energyContainer = energyContainer;
    }

    public bool IsAnyDamaged(PlayerType playerType)
      => playerType switch
      {
        PlayerType.Left => leftResult.Count > 0,
        PlayerType.Right => rightResult.Count > 0,
        _ => throw new NotImplementedException(),
      };

    public void DebugResult()
    {
      var chapter = gameDataProvider.GetSelectedChapter();
      var stage = gameDataProvider.GetSelectedStage();

      var stb = new StringBuilder($"[ {chapter}-{stage} ({(chapter - 1) * StageConst.StageUnit + stage}): {energyContainer.TotalEnergy:f2} / {stageEnergy} -> {(stageEnergy - energyContainer.TotalEnergy):f2} ]");

      if (leftResult.Count > 0)
        stb.Append("(");
      foreach (var leftPair in leftResult)
        stb.Append($" Left {leftPair.Key}: {leftPair.Value.Sum():f2}");
      if (leftResult.Count > 0)
        stb.Append(")");

      if (rightResult.Count > 0)
        stb.Append("(");
      foreach (var rightPair in rightResult)
        stb.Append($" Right {rightPair.Key}: {rightPair.Value.Sum():f2}");
      if (rightResult.Count > 0)
        stb.Append(")");

      Debug.Log(stb.ToString());
    }

    public void SaveRecord()
    {
      var leftSum = 0.0f;
      foreach (var leftValues in leftResult.Values)
        leftSum += leftValues.Sum();
      var rightSum = 0.0f;
      foreach (var rightValues in rightResult.Values)
        rightSum += rightValues.Sum();

      saveResult = new()
      {
        LeftSum = leftSum,
        RightSum = rightSum,
        Running = runningTime,
        TotalSum = leftSum + rightSum + runningTime,
      };
    }

    public IStageRecorderService.SaveResultT GetSumResult()
    {
      return saveResult;
    }

    public IStageRecorderService.DamageResultT GetPlayerDamageResult(PlayerType playerType)
    {
      var targetResult = playerType switch
      {
        PlayerType.Left => leftResult,
        PlayerType.Right => rightResult,
        _ => throw new NotImplementedException(),
      };

      var t = new IStageRecorderService.DamageResultT
      {
        results = new()
      };
      foreach (var pair in targetResult)
        t.results.Add(pair.Key, pair.Value);

      return t;
    }

    public void OnPlayerDamaged(PlayerType playerType, DamageType damageType, float value)
    {
      var targetResult = playerType switch
      {
        PlayerType.Left => leftResult,
        PlayerType.Right => rightResult,
        _ => throw new NotImplementedException(),
      };

      if (targetResult.ContainsKey(damageType))
        targetResult[damageType].Add(value);
      else
        targetResult[damageType] = new() { value };
    }

    public void ResetRecords()
    {
      leftResult.Clear();
      rightResult.Clear();
      runningTime = 0.0f;
    }

    public void AddRestartCount()
    {
      restartCount++;
      onDataChangeds.TryInvoke(IStageFailDataSubscriber.DataType.Restart, restartCount);
    }

    public void AddFailCount()
    {
      failCount++;
      onDataChangeds.TryInvoke(IStageFailDataSubscriber.DataType.Failure, failCount);

      var bonusTime = (failCount / playerEnergyDataSO.DeathStep) * playerEnergyDataSO.DeathBonusValue;
      onDataChangeds.TryInvoke(IStageFailDataSubscriber.DataType.BonusTime, bonusTime);
    }

    public void ResetFailCount()
    {
      failCount = 0;

      onDataChangeds.TryInvoke(IStageFailDataSubscriber.DataType.Restart, restartCount);
      onDataChangeds.TryInvoke(IStageFailDataSubscriber.DataType.Failure, failCount);

      var bonusTime = (failCount / playerEnergyDataSO.DeathStep) * playerEnergyDataSO.DeathBonusValue;
      onDataChangeds.TryInvoke(IStageFailDataSubscriber.DataType.BonusTime, bonusTime);
    }

    public bool IsThisSuccessIsPerfect()
      => leftResult.Count == 0 && rightResult.Count == 0;

    public void SubscribeOnChanged(IStageFailDataSubscriber.DataType dataType, UnityAction<float> unityAction)
      => onDataChangeds.AddEvent(dataType, unityAction);

    public void UnsubscribeOnChanged(IStageFailDataSubscriber.DataType dataType, UnityAction<float> unityAction)
      => onDataChangeds.RemoveEvent(dataType, unityAction);

    public void AddRunningTimer(float value)
      => runningTime += value;

    public float GetRunningTime()
      => runningTime;

    public float GetSum()
    {
      var leftSum = 0.0f;
      foreach (var value in leftResult.Values)
        leftSum += value.Sum();
      var running = this.runningTime;
      var rightSum = 0.0f;
      foreach(var value in rightResult.Values)
        rightSum += value.Sum();

      return leftSum + running + rightSum;
    }
  }
}
