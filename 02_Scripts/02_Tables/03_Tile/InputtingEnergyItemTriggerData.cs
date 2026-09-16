using LR.Table.Input;
using UnityEngine;

namespace LR.Table.TriggerTile
{
  [System.Serializable]
  public class InputtingEnergyItemTriggerData
  {
    [field: SerializeField] public float RestoreValue { get; private set; }
    [field: SerializeField] public InputQTEData QTEData { get; private set; }
    [field: SerializeField] public InputProgressData InputProgressData { get; private set; }
  }
}
