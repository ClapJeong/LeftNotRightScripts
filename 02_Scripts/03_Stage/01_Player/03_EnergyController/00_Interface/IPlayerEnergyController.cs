using LR.Stage.Player.Enum;

namespace LR.Stage.Player
{
  public interface IPlayerEnergyController
  {
    public void Damage(PlayerType hitPlayer, float value, DamageType damageType, bool ignoreInvincible = false);
  }
}