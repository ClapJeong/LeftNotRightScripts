using UnityEngine;
using UnityEngine.Events;

namespace ScriptableEvent
{
  public enum GameDataEventType
  {
    AddClearedStage,
    MinusClearedStage,
    ClearAll,
    Max,
  }
  public class ScriptableGameDataEventListener : MonoBehaviour
  {
    [SerializeField] private GameDataEventType type;
    [SerializeField] private UnityEvent listenrEvent;

    private void OnEnable()
    {
      ScriptableEventSO.instance?.RegisterGameDataEvent(type,this);
    }

    private void OnDisable()
    {
      ScriptableEventSO.instance?.RegisterGameDataEvent(type, this);
    }

    public void Raise()
      => listenrEvent?.Invoke();
  }
}