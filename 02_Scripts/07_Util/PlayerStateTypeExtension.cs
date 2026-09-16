using LR.Stage.Player.Enum;

public static class PlayerStateTypeExtenstion
{
  public static bool IsCharging(this PlayerState playerStateType)
  {
    if (playerStateType == PlayerState.Inputting)
      return true;
    else
      return false;
  }
}