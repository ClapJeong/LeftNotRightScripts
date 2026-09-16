using LR.Table.Input;
using UnityEngine;

namespace LR.Table.TriggerTile
{
  [System.Serializable]
  public class SignalTriggerData
  {
    [field: SerializeField] public InputQTEData QTEData { get; private set; }
    [field:Space(5)]
    [field: SerializeField] public InputProgressData ProgressData { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public BounceData FailBounceData { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public float GlowInterval { get; private set; }
    [field: SerializeField] public float IdleIntensity { get; private set; }
    [field: SerializeField] public float ActivateIntensity { get; private set; }
  }
}
