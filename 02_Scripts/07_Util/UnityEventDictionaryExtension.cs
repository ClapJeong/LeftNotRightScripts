using System.Collections.Generic;
using UnityEngine.Events;

public static class UnityEventDictionaryExtension
{
  public static bool TryInvoke<T>(this Dictionary<T, UnityEvent> dictionary, T key)
  {
    if (dictionary.TryGetValue(key, out var unityEvent))
    {
      unityEvent.Invoke();
      return true;
    }
    else
      return false;
  }

  public static bool TryInvoke<T, U>(this Dictionary<T, UnityEvent<U>> dictionary, T key, U factor)
  {
    if (dictionary.TryGetValue(key, out var unityEvent))
    {
      unityEvent.Invoke(factor);
      return true;
    }
    else
      return false;
  }

  public static void AddEvent<T>(this Dictionary<T, UnityEvent> dictionary, T key, UnityAction unityAction)
  {
    if (dictionary.TryGetValue(key, out var existEvent))
      existEvent.AddListener(unityAction);
    else
    {
      dictionary[key] = new UnityEvent();
      dictionary[key].AddListener(unityAction);
    }
  }

  public static void AddEvent<T,U>(this Dictionary<T, UnityEvent<U>> dictionary, T key, UnityAction<U> unityAction)
  {
    if (dictionary.TryGetValue(key, out var existEvent))
      existEvent.AddListener(unityAction);
    else
    {
      dictionary[key] = new UnityEvent<U>();
      dictionary[key].AddListener(unityAction);
    }
  }

  public static void RemoveEvent<T>(this Dictionary<T, UnityEvent> dictionary, T key, UnityAction unityAction)
  {
    if(dictionary.TryGetValue(key,out var existEvent))
      existEvent.RemoveListener(unityAction);
  }

  public static void RemoveEvent<T,U>(this Dictionary<T, UnityEvent<U>> dictionary, T key, UnityAction<U> unityAction)
  {
    if (dictionary.TryGetValue(key, out var existEvent))
      existEvent.RemoveListener(unityAction);
  }
}