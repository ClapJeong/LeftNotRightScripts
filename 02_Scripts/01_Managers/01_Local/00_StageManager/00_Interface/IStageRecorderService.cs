using LR.Stage.Player.Enum;
using System.Collections.Generic;

namespace LR.Manager.Stage
{
  public interface IStageRecorderService
  {
    public struct DamageResultT
    {
      public Dictionary<DamageType, List<float>> results; 
    }

    public struct SaveResultT
    {
      public float LeftSum;
      public float RightSum;
      public float Running;
      public float TotalSum;
    }

    public bool IsAnyDamaged(PlayerType playerType);
    public void DebugResult();

    public void OnPlayerDamaged(PlayerType playerType, DamageType damageType, float value);

    public DamageResultT GetPlayerDamageResult(PlayerType playerType);

    public void ResetRecords();

    public void AddRestartCount();

    public void AddFailCount();

    public void ResetFailCount();

    public bool IsThisSuccessIsPerfect();

    public void AddRunningTimer(float value);

    public void SaveRecord();

    public SaveResultT GetSumResult();
  }
}