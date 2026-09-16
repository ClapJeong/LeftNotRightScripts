using UnityEngine;
using UnityEngine.Events;
using LR.Stage.Player.Enum;

namespace ScriptableEvent
{
  public enum EnergyEventType
  {
    Damage,
    Restore
  }
  public class ScriptableEnergyEventListener : MonoBehaviour
  {
    [SerializeField] private PlayerType playerType;
    [SerializeField] private EnergyEventType type;
    [SerializeField] private UnityEvent<float> action;

    private void OnEnable()
    {
      if (this == null || ScriptableEventSO.instance == null)
        return;

      switch (playerType)
      {
        case PlayerType.Left:
          ScriptableEventSO.instance.RegisterLeftEnergyEvent(this);
          break;

        case PlayerType.Right:
          ScriptableEventSO.instance.RegisterRightEnergyEvent(this);
          break;
      }      
    }

    private void OnDisable()
    {
      if (this == null || ScriptableEventSO.instance == null)
        return;

      switch (playerType)
      {
        case PlayerType.Left:
          ScriptableEventSO.instance?.UnregisterLeftEnergyEvent(this);
          break;

        case PlayerType.Right:
          ScriptableEventSO.instance?.UnregisterRightEnergyEvent(this);
          break;
      }
    }

    public void Raise(float value)
    {
      switch (type)
      {
        case EnergyEventType.Damage:
          if (value < 0) 
            action?.Invoke(Mathf.Abs(value));
          break;

        case EnergyEventType.Restore:
          if(value > 0)
            action?.Invoke(value);
          break;

        default: throw new System.NotImplementedException();
      }
    }
  }
}