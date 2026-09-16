using UnityEngine;

namespace LR.Table.StageGimmick
{
  [System.Serializable]
  public class QTEBombData
  {
    [field: SerializeField] public int SumMaxCount { get; private set; }
    [field: SerializeField] public float CompleteDuration {  get; private set; }
    [field: SerializeField] public float CompleteWaitDuration { get; private set; }
    [field: SerializeField] public float InputAdditionalDuration { get; private set; }
    [field: SerializeField] public float ConveyorCrossingDurationRatio { get; private set; }
  }
}
