using UnityEngine;

namespace LR.Table.TriggerTile
{
  [System.Serializable]
  public class PortalTriggerData
  {
    [field: SerializeField] public float GlowMin { get; private set; }
    [field: SerializeField] public float GlowMax { get; private set; }
    [field: SerializeField] public float GlowDuration {  get; private set; }
    [field: SerializeField] public float MoveEffectMoveDuration { get; private set; }
  }
}
