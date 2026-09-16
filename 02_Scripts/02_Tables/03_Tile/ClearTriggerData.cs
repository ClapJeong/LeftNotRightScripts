using UnityEngine;

namespace LR.Table.TriggerTile
{
  [System.Serializable]
  public class ClearTriggerData
  {
    [field: SerializeField] public InstanceEffectType EffectType { get; private set; }
    [field: SerializeField] public float IdleGlowIntensityMin { get; private set; }
    [field: SerializeField] public float IdleGlowIntensityMax { get; private set; }    
    [field: SerializeField] public float ActivateGlowIntensityMin { get; private set; }
    [field: SerializeField] public float ActivateGlowIntensityMax { get; private set; }
    [field: SerializeField] public float GlowBlinkDuration { get; private set; }
    [field: SerializeField] public int IdleBlinkCount {  get; private set; }
    [field: SerializeField] public int ActivateBlinkCount { get; private set; }
  }
}