using UnityEngine;

namespace LR.Table.Player
{
  [System.Serializable]
  public class PlayerStunData
  {
    [field: SerializeField] public float StunDuration { get; private set; }
    [field: SerializeField] public float ShearRange { get; private set; }
    [field: SerializeField] public float ShearInterval { get; private set; }
  }
}