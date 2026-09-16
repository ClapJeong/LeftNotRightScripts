using UnityEngine;
using UnityEngine.Events;

namespace ScriptableEvent
{
  public enum StageEventType
  {
    Complete,
    LeftFail,
    RightFail,
    Restart,
  }

  public class ScriptableStageEventListener : MonoBehaviour
  {
    [SerializeField] private StageEventType type;
    [SerializeField] private UnityEvent stageEvent;

    private void OnEnable()
      => ScriptableEventSO.instance?.RegisterStageEvent(type, this);

    private void OnDisable()
      => ScriptableEventSO.instance?.UnregisterStageEvent(type, this);

    public void Raise()
      => stageEvent?.Invoke();
  }
}
