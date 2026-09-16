using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

public static class SelectableUtil
{
  public static void SetNavigation(this Selectable selectable, Selectable up, Selectable right, Selectable down, Selectable left)
  {
    var navigation = new Navigation
    {
      mode = Navigation.Mode.Explicit,
      selectOnUp = up,
      selectOnRight = right,
      selectOnDown = down,
      selectOnLeft = left
    };

    selectable.navigation = navigation;
  }

  public static void AddNavigation(this Selectable selectable, Direction direction, Selectable target)
  {
    var navigation = selectable.navigation;
    navigation.mode = Navigation.Mode.Explicit;
    switch (direction)
    {
      case Direction.Up:
        navigation.selectOnUp = target;
        break;

      case Direction.Down:
        navigation.selectOnDown = target;
        break;

      case Direction.Left:
        navigation.selectOnLeft = target;
        break;

      case Direction.Right:
        navigation.selectOnRight = target;
        break;

      default: throw new System.NotImplementedException();
    }

    selectable.navigation = navigation;
  }

}
