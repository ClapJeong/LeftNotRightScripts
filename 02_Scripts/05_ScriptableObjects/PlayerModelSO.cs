using UnityEngine;
using LR.Table.Player;
using LR.Stage.Player.Enum;

[CreateAssetMenu(fileName = "PlayerModelSO", menuName ="SO/PlayerModel")]
public class PlayerModelSO : ScriptableObject
{
  [field: SerializeField] public PlayerMovementData Movement {  get; private set; }

  [field: Space(10)]
  [field: SerializeField] public PlayerStunData Stun { get; private set; }
  [field: Space(10)]
  [field: SerializeField] public LayerMask ObstacleLayer { get; private set; }
  [field: SerializeField] public PlayerCollisionData LeftCollisionData {  get; private set; }
  [field: SerializeField] public PlayerCollisionData RightCollisionData { get; private set; }
  public PlayerCollisionData GetCollisionData(PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => LeftCollisionData,
      PlayerType.Right => RightCollisionData,
      _ => throw new System.NotImplementedException(),
    };
  [field: Space(10)]
  [field: SerializeField] public PlayerBlinkData BlinkData { get; private set; }

  [field: Space(10)]
  [field: SerializeField] public PlayerWallHitSizeData WallHitSizeData { get; private set; }
}
