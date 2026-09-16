using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

namespace ScriptableEvent
{
  public enum LocaleEventType
  {
    SetLocale,
  }
  public class ScriptableLocaleEventListener : MonoBehaviour
  {    
    [SerializeField] private LocaleEventType type;
    [SerializeField] private UnityEvent<Locale> setLocaleEvent;

    private void OnEnable()
    {
      switch (type)
      {
        case LocaleEventType.SetLocale:
          ScriptableEventSO.instance?.RegisterSetLocaleEvent(this);
          break;

        default: throw new System.NotImplementedException();
      }
    }

    private void OnDisable()
    {
      switch (type)
      {
        case LocaleEventType.SetLocale:
          ScriptableEventSO.instance?.UnregisterSetLocaleEvent(this);
          break;

        default: throw new System.NotImplementedException();
      }
    }

    public void Raise(Locale locale)
      => setLocaleEvent?.Invoke(locale);
  }
}
