namespace LR.Stage.Player.Enum
{
  public enum DamageType
  {
    Strong,
    WallBump,
  }

  public enum PlayerState
  {
    None,

    Idle,
    Move,
    Stun,
    Inputting,

    Clear,
    Exhausted,
  }

  public enum PlayerType
  {
    Left,
    Right,
  }

  public enum PlayerEffect
  {
    Move,
    Inputing,
    Stun,
    Exhaust,
    Run,
    Electric,
  }
}
