using System;
using System.Collections.Generic;

public static class DirectionUtil
{
  public static List<Direction> GetShuffledDirections()
  {
    var values = (Direction[])Enum.GetValues(typeof(Direction));
    var list = new List<Direction>(values);

    var rng = new Random();

    for (int i = list.Count - 1; i > 0; i--)
    {
      int j = rng.Next(i + 1);
      (list[i], list[j]) = (list[j], list[i]);
    }

    return list;
  }
}
