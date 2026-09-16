using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

namespace ScriptableEvent
{
  [CreateAssetMenu(fileName = "ScriptableEventSO", menuName = "SO/ScriptableEvent")]
  public class ScriptableEventSO : ScriptableObject
  {
    private class StageListnerSet
    {
      public static implicit operator bool(StageListnerSet s) => s != null;

      public readonly StageEventType type;
      public readonly List<ScriptableStageEventListener> listeners;

      public StageListnerSet(StageEventType type,ScriptableStageEventListener listener)
      {
        this.type = type;
        this.listeners = new() { listener };
      }

      public void Add(ScriptableStageEventListener listener)
        => listeners.Add(listener);

      public void Remove(ScriptableStageEventListener listener)
        =>listeners.Remove(listener);

      public void Raise()
      {
        for(int i=0;i<listeners.Count; i++)
          listeners[i].Raise();
      }
    }
    private class GameDataListnerSet
    {
      public static implicit operator bool(GameDataListnerSet s) => s != null;

      public readonly GameDataEventType type;
      public readonly List<ScriptableGameDataEventListener> listeners;

      public GameDataListnerSet(GameDataEventType type, ScriptableGameDataEventListener listener)
      {
        this.type = type;
        this.listeners = new() { listener };
      }

      public void Add(ScriptableGameDataEventListener listener)
        => listeners.Add(listener);

      public void Remove(ScriptableGameDataEventListener listener)
        => listeners.Remove(listener);

      public void Raise()
      {
        for (int i = 0; i < listeners.Count; i++)
          listeners[i].Raise();
      }
    }

    private readonly List<ScriptableLocaleEventListener> setLocaleListeners = new();
    private readonly List<StageListnerSet> stageListeners = new();
    private readonly List<GameDataListnerSet> gameDataListnerSets = new();
    private readonly List<ScriptableEnergyEventListener> leftEnergyListners = new();
    private readonly List<ScriptableEnergyEventListener> rightEnergyListners = new();

    public static ScriptableEventSO instance;

    private void OnEnable()
    {
      if (instance == null)
        instance = this;
    }

    #region Locale
    public void OnLocaleChanged(Locale locale)
    {
      for (int i = 0; i < setLocaleListeners.Count; i++)
        setLocaleListeners[i].Raise(locale);
    }

    public void RegisterSetLocaleEvent(ScriptableLocaleEventListener listener)
      => setLocaleListeners.Add(listener);

    public void UnregisterSetLocaleEvent(ScriptableLocaleEventListener listener)
      => setLocaleListeners.Remove(listener);
    #endregion

    #region Satge
    public void OnStageEvent(StageEventType stageEventType)
    {
      var set = stageListeners.FirstOrDefault(s => s.type == stageEventType);
      set?.Raise();
    }

    public void RegisterStageEvent(StageEventType stageEventType, ScriptableStageEventListener listener)
    {
      var set = stageListeners.FirstOrDefault(s => s.type == stageEventType);

      if (set)
        set.Add(listener);
      else
        stageListeners.Add(new StageListnerSet(stageEventType, listener));
    }

    public void UnregisterStageEvent(StageEventType stageEventType, ScriptableStageEventListener listener)
    {
      var set = stageListeners.FirstOrDefault(s => s.type == stageEventType);
      set?.Remove(listener);
    }
    #endregion

    #region GameData
    public void OnGameDataEvent(GameDataEventType gameDataEventType)
    {
      var set = gameDataListnerSets.FirstOrDefault(s => s.type == gameDataEventType);
      set?.Raise();
    }

    public void RegisterGameDataEvent(GameDataEventType gameDataEventType, ScriptableGameDataEventListener listener)
    {
      var set = gameDataListnerSets.FirstOrDefault(s => s.type == gameDataEventType);

      if (set)
        set.Add(listener);
      else
        gameDataListnerSets.Add(new GameDataListnerSet(gameDataEventType, listener));
    }

    public void UnregisterGameDataEvent(GameDataEventType gameDataEventType, ScriptableGameDataEventListener listener)
    {
      var set = gameDataListnerSets.FirstOrDefault(s => s.type == gameDataEventType);
      set?.Remove(listener);
    }

    public void SetDialogueCondition(string key, int left, int right)
    {
      
    }
    #endregion

    #region Energy
    public void OnLeftEnergyChanged(float value)
    {
      for (int i = 0; i < leftEnergyListners.Count; i++)
        leftEnergyListners[i].Raise(value);
    }

    public void RegisterLeftEnergyEvent(ScriptableEnergyEventListener listener)
      => leftEnergyListners.Add(listener);

    public void UnregisterLeftEnergyEvent(ScriptableEnergyEventListener listener)
      => leftEnergyListners.Remove(listener);

    public void OnRightEnergyChanged(float value)
    {
      for (int i = 0; i < rightEnergyListners.Count; i++)
        rightEnergyListners[i].Raise(value);
    }

    public void RegisterRightEnergyEvent(ScriptableEnergyEventListener listener)
      => rightEnergyListners.Add(listener);

    public void UnregisterRightEnergyEvent(ScriptableEnergyEventListener listener)
      => rightEnergyListners.Remove(listener);
    #endregion
  }
}