using UnityEngine;

namespace LR.Table.StageGimmick
{
  [System.Serializable]
  public class SwapData
  {
    [field: SerializeField] public float SwapCooldownDuration {  get; private set; }
    [field: SerializeField] public float SwapDuration { get; private set; }
    [field: SerializeField] public float ClockInterval { get; private set; }
    [field: SerializeField] public float ClockCount { get; private set; }
    [field: SerializeField] public float FastClockCount { get; private set; }
    [field: SerializeField] public float TimeScaleValue {  get; private set; }
    [field: SerializeField] public float TimeSlowDuration { get; private set; }
    [field: SerializeField] public float TimeSlowDelay { get; private set; }
  }
}
