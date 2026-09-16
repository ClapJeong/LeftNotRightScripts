using UniRx;

namespace LR.Stage.Player
{
  public class PlayerStatus
  {
    public bool IsWallStuck;

    public bool IsInputting;

    public bool IsTeleported;

    public BoolReactiveProperty IsElectric = new(false);

    public float DeltaMoveLength;
  }
}
