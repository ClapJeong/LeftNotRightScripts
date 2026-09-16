using LR.Manager.Input;
using LR.Stage.Player.Enum;
using UnityEngine;

public static class DirectionExtension
{
  public static Direction ParseOpposite(this Direction direction)
    => direction switch
    {
      Direction.Up => Direction.Down,
      Direction.Down => Direction.Up,
      Direction.Left => Direction.Right,
      Direction.Right => Direction.Left,
      _ => throw new System.NotImplementedException(),
    };

  public static Vector2 ParseVector2(this Direction direction)
    => direction switch
    {
      Direction.Up => Vector2.up,
      Direction.Right => Vector2.right,
      Direction.Down => Vector2.down,
      Direction.Left => Vector2.left,
      _ => throw new System.NotImplementedException(),
    };

  public static Vector3 ParseVector3(this Direction direction)
    => direction switch
    {
      Direction.Up => Vector3.up,
      Direction.Right => Vector3.right,
      Direction.Down => Vector3.down,
      Direction.Left => Vector3.left,
      _ => throw new System.NotImplementedException(),
    };

  public static LRInputType ParseToLeftInputActionType(this Direction direction)
    => direction switch
    {
      Direction.Up => LRInputType.LeftUp,
      Direction.Right => LRInputType.LeftRight,
      Direction.Down => LRInputType.LeftDown,
      Direction.Left => LRInputType.LeftLeft,
      _ => throw new System.NotImplementedException(),
    };

  public static LRInputType ParseToRightInputActionType(this Direction direction)
    => direction switch
    {
      Direction.Up => LRInputType.RightUp,
      Direction.Right => LRInputType.RightRight,
      Direction.Down => LRInputType.RightDown,
      Direction.Left => LRInputType.RightLeft,
      _ => throw new System.NotImplementedException(),
    };

  public static LRInputType ParseToLRInputType(this Direction direction, bool isLeft)
    => isLeft ? direction.ParseToLeftInputActionType() : direction.ParseToRightInputActionType();

  public static LRInputType ParseToLRInputType(this Direction direction, PlayerType playerType)
    => ParseToLRInputType(direction, isLeft: playerType switch
    {
      PlayerType.Left => true,
      PlayerType.Right => false,
      _ => throw new System.NotImplementedException(),
    });
}