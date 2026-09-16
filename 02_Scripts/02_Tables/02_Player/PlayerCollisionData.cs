using UnityEngine;

namespace LR.Table.Player
{
  [System.Serializable]
  public class PlayerCollisionData
  {
    [field: SerializeField] public float WallBumpVelocityNormalized { get; private set; }
    [field: SerializeField] public float WallBumpMinDamage { get; private set; }
    [field: SerializeField] public float WallBumpMaxDamge {  get; private set; }
    [field: SerializeField] public float WallBumpMinDuration { get; private set; }
    [field: SerializeField] public float WallBumpMaxDuration {  get; private set; }
    [field: SerializeField] public float WallBumpAnimationDuration {  get; private set; }
    [field: SerializeField] public float WallBumpVelocityDecreaseValue { get; private set; }

    [field: SerializeField] public PhysicsMaterial2D WallMaterial { get; private set; }

    [field: SerializeField] public PhysicsMaterial2D DefaultMaterial { get; private set; }
  }
}
