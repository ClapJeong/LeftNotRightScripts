using LR.Stage.Player.Enum;
using UnityEngine.Events;

namespace LR.Stage.Player
{
  public interface IPlayerEnergySubscriber
  {
    public enum StateEvent
    {
      OnRestoreFull,
      OnExhausted,
    }

    public enum ValueEvent
    {
      LeftDamaged,
      RightDamaged,
      AnyDamaged,
    }

    public enum Threshhold
    {
      Above,
      Below,
    }

    public void SubscribeStateEvent(StateEvent stateEvent, UnityAction action);

    public void UnsubscribeStateEvent(StateEvent stateEvent, UnityAction action);

    public void SubscribeThreshhold(Threshhold threshhold, float normalizedValue, UnityAction action);

    public void UnsubscribeThreshhold(Threshhold threshhold, float normalizedValue, UnityAction action);

    public void SubscribeValueEvent(ValueEvent valueEvent, UnityAction<float> action);

    public void UnsubscribeValueEvent(ValueEvent valueEvent, UnityAction<float> action);

    public void SubscribeOnHit(UnityAction<PlayerType, DamageType> onHit);

    public void UnsubscribeOnHit(UnityAction<PlayerType, DamageType> onHit);

    public void SubscribeOnDamageValue(UnityAction<PlayerType, float> onDamaged);

    public void UnsbscribeOnDamageValue(UnityAction<PlayerType, float> onDamaged);
  }
}