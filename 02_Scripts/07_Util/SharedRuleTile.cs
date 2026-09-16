using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Shared Rule Tile")]
public class SharedRuleTile : RuleTile
{
  [Header("같은 것으로 취급할 타일들")]
  public TileBase[] connectableTiles;

  public override bool RuleMatch(int neighbor, TileBase tile)
  {
    switch (neighbor)
    {
      case TilingRuleOutput.Neighbor.This:
        return IsSameGroup(tile);

      case TilingRuleOutput.Neighbor.NotThis:
        return !IsSameGroup(tile);
    }

    return base.RuleMatch(neighbor, tile);
  }

  private bool IsSameGroup(TileBase tile)
  {
    if (tile == this)
      return true;

    if (connectableTiles != null)
    {
      foreach (var t in connectableTiles)
      {
        if (tile == t)
          return true;
      }
    }

    return false;
  }
}