using UnityEngine;

namespace LR.Table.Player
{
  [System.Serializable]
  public class PlayerWallHitSizeData
  {
    [field: SerializeField] public float ScaleValue { get; private set; }
    [field: SerializeField] public float Duration { get; private set; }
  }
}
