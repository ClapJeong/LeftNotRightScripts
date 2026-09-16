using UnityEngine.Events;
using LR.Stage.Player.Enum;
using LR.Stage.TriggerTile.Enum;

namespace LR.Manager.Stage.StageObject
{
  public interface ITriggerTileEventSubscriber
  {
    public void SubscribeOnEnter(PlayerType playerType, TriggerTileType type, UnityAction onEnter);

    public void SubscribeOnExit(PlayerType playerType, TriggerTileType type, UnityAction onExit);

    public void UnsubscribeOnEnter(PlayerType playerType, TriggerTileType type, UnityAction onEnter);

    public void UnsubscribeOnExit(PlayerType playerType, TriggerTileType type, UnityAction onExit);
  }
}