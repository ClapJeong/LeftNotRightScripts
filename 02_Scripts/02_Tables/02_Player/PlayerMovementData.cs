using LR.Table.Input;
using UnityEngine;

namespace LR.Table.Player
{
  [System.Serializable]
  public class PlayerMovementData
  {
    [field: SerializeField] public Vector3 UpVector { get; private set; }
    [field: SerializeField] public Vector3 RightVector { get; private set; }
    [field: SerializeField] public Vector3 DownVector { get; private set; }
    [field: SerializeField] public Vector3 LeftVector { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public float Acceleration { get; private set; }
    [field: SerializeField] public float Decceleration { get; private set; }
    [field: SerializeField] public float MaxSpeed { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public float ElectricModifier {  get; private set; }
  }
}
