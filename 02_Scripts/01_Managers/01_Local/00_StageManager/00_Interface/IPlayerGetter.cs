using LR.Stage.Player;
using LR.Stage.Player.Enum;

namespace LR.Manager.Stage
{
  public interface IPlayerGetter
  {
    public IPlayerPresenter GetPlayer(PlayerType playerType);

    public bool IsAllPlayerExist();
  }
}