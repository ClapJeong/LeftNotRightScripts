using LR.Stage.Player.Enum;
using UnityEngine;

namespace LR.Manager.Stage.Marking
{
  public interface IMarkPlacer
  {
    public void MarkWallHitPaint(PlayerType playerType, Vector2 worldPosition);

    public void MarkDeadPaint(PlayerType playerType, Vector2 worldPosition);
  }
}
