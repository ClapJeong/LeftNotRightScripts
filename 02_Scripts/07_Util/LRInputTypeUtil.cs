using LR.Manager.Input;
using LR.UI.Enum;
using System;
using System.Collections.Generic;

public static class LRInputTypeUtil
{
  public static List<LRInputType> GetLefts()
    => new()
    {
      LRInputType.LeftUp,
      LRInputType.LeftRight,
      LRInputType.LeftDown,
      LRInputType.LeftLeft,
    };

  public static List<LRInputType> GetRights()
    => new()
    {
      LRInputType.RightUp,
      LRInputType.RightRight,
      LRInputType.RightDown,
      LRInputType.RightLeft,
    };

  public static Direction ParseToDirection(this LRInputType inputDirection)
  => inputDirection switch
  {
    LRInputType.LeftUp => Direction.Up,
    LRInputType.LeftRight => Direction.Right,
    LRInputType.LeftDown => Direction.Down,
    LRInputType.LeftLeft => Direction.Left,

    LRInputType.RightUp => Direction.Up,
    LRInputType.RightRight => Direction.Right,
    LRInputType.RightDown => Direction.Down,
    LRInputType.RightLeft => Direction.Left,

    _ => throw new System.NotImplementedException(),
  };

  public static LRInputType ParseToOpposite(this LRInputType inputDirection)
  => inputDirection switch
  {
    LRInputType.LeftUp => LRInputType.RightUp,
    LRInputType.LeftRight => LRInputType.RightRight,
    LRInputType.LeftDown => LRInputType.RightDown,
    LRInputType.LeftLeft => LRInputType.RightLeft,

    LRInputType.RightUp => LRInputType.LeftUp,
    LRInputType.RightRight => LRInputType.LeftRight,
    LRInputType.RightDown => LRInputType.LeftDown,
    LRInputType.RightLeft => LRInputType.LeftLeft,

    _ => throw new System.NotImplementedException(),
  };

  public static bool IsQTEAble(this LRInputType inputDirection)
  => inputDirection switch
  {
    LRInputType.LeftUp => true,
    LRInputType.LeftRight => true,
    LRInputType.LeftDown => true,
    LRInputType.LeftLeft => true,

    LRInputType.RightUp => true,
    LRInputType.RightRight => true,
    LRInputType.RightDown => true,
    LRInputType.RightLeft => true,

    _ => false,
  };

  public static LRInputType GetRandom()
  {
    var list = new List<LRInputType>();
    list.AddRange(GetLefts());
    list.AddRange(GetRights());
    return list[UnityEngine.Random.Range(0, list.Count)];
  }

  public static LRInputType GetRandomLeft()
  {
    var lefts = GetLefts();
    return lefts[UnityEngine.Random.Range(0, lefts.Count)];
  }

  public static LRInputType GetRandomRight()
  {
    var rights = GetRights();
    return rights[UnityEngine.Random.Range(0, rights.Count)];
  }

  public static List<LRInputType> GetRandomLefts(int count)
  {
    var lefts = new List<LRInputType>(GetLefts());

    if (count > lefts.Count)
      throw new ArgumentException("count is larger than available Left elements");

    Shuffle(lefts);

    return lefts.GetRange(0, count);
  }

  public static List<LRInputType> GetRandomRights(int count)
  {
    var rights = new List<LRInputType>(GetRights());

    if (count > rights.Count)
      throw new ArgumentException("count is larger than available Right elements");

    Shuffle(rights);

    return rights.GetRange(0, count);
  }

  private static void Shuffle<T>(List<T> list)
  {
    for (int i = list.Count - 1; i > 0; i--)
    {
      int j = UnityEngine.Random.Range(0, i + 1);
      (list[i], list[j]) = (list[j], list[i]);
    }
  }

  public static bool IsLeft(this LRInputType inputDirection)
    => inputDirection == LRInputType.LeftUp ||
    inputDirection == LRInputType.LeftRight ||
    inputDirection == LRInputType.LeftDown ||
    inputDirection == LRInputType.LeftLeft;

  public static bool IsRight(this LRInputType inputDirection)
      => inputDirection == LRInputType.RightUp ||
      inputDirection == LRInputType.RightRight ||
      inputDirection == LRInputType.RightDown ||
      inputDirection == LRInputType.RightLeft;
}
