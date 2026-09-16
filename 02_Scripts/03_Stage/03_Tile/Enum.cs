namespace LR.Stage.TriggerTile.Enum
{
  public enum TriggerTileType
  {
    LeftClear,
    RightClear,
    Spike,
    DefaultSignal,
    InputSignal,
    DefaultEnergy,
    InputtingEnergy,
    Portal,
    Conveyor,
  }

  public enum SignalInput
  {
    QTE,
    Progress,
  }

  public enum SignalInputFail
  {
    Bounce,
    BounceAndStun,
  }

  public enum SignalLife
  {
    OnlyActivate,
    ActivateAndDeactivate,
  }
}
