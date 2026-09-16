using LR.Stage.Player.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace LR.Stage.Player
{
  public interface IPlayerEffectController
  {
    public void PlayEffect(PlayerEffect effect);

    public void StopEffect(PlayerEffect effect);

    public void UpdateMoveDirection(Vector2 direction);

    public void StopAllEffects(params PlayerEffect[] except);
  }
}
