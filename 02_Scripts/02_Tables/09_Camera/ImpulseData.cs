using UnityEngine;

namespace LR.Table.Camera
{
  [System.Serializable]
  public class ImpulseData
  {
    [field: Header("[ Wall ]")]
    [field: SerializeField] public float WallBumpHorizontalValue { get; private set; }
    [field: SerializeField] public float WallBumpVerticalRange {  get; private set; }

    [field: Header("[ Damaged ]")]
    [field: SerializeField] public float HitHorizontalValue { get; private set; }
    [field: SerializeField] public float HitVerticalRange { get; private set; }

    [field: Header("[ GimmickStun ]")]
    [field: SerializeField] public float GimmickStunForce {  get; private set; }

    [field: Header("[ ExitOpen ]")]
    [field: SerializeField] public float ExitOpenForce { get; private set; }

    [field: Header("[ ExhaustNoise ]")]
    [field: SerializeField] public float ExhaustNoiseChangeDuration { get; private set; }
  }
}
