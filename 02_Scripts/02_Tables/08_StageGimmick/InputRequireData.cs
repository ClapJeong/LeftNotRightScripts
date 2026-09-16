using UnityEngine;

namespace LR.Table.StageGimmick
{
  [System.Serializable]
  public class InputRequireData
  {
    [field: SerializeField] public float MaxValue { get; private set; }
    [field: SerializeField] public float BeginValue {  get; private set; }
    [field: SerializeField] public float IncreaseValue {  get; private set; }
    [field: SerializeField] public float DecreaseValue { get; private set; }
    [field: SerializeField] public float RegenWaitDuration { get; private set; }
    [field: SerializeField] public float RegenDuration {  get; private set; }
    [field: SerializeField] public float MoveAcceptDeltaLength {  get; private set; }
    [field: SerializeField] public float MoveAcceptSpeedNormalized { get; private set; }
    [field: SerializeField] public float ClockIntervalMin {  get; private set; }
    [field: SerializeField] public float ClockIntervalMax { get; private set; }
    [field: SerializeField] public float ObjectRegenAlpha { get; private set; }
    [field: SerializeField] public float ClockSoundMin { get; private set; }
  }
}
