using UnityEngine;

namespace LR.Table.Player
{
  [System.Serializable]
  public class PlayerBlinkData
  {
    [field: SerializeField] public float WallHitBlinkDuration {  get; set; }
    [field: SerializeField] public float TeleportBlinkDuration {  get; set; }
  }
}
