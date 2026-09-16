using LR.Stage.TriggerTile.Enum;
using LR.Manager.Stage.Signal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace LR.Manager.Stage
{
  public class SignalService :
  ISignalKeyRegister,
  ISignalConsumer,
  ISignalSubscriber,
  ISignalIDLifeProvider
  {
    private class Signal
    {
      public readonly UnityEvent<bool> activateEvent = new();
      public readonly UnityEvent<bool> deactivateEvent = new();
      public readonly UnityEvent<int> idActivateEvent = new();
      public readonly UnityEvent<int> idDeactivateEvent = new();

      private readonly Dictionary<int, bool> keys = new();
      public Dictionary<int, SignalLife> IDLifes { get; private set; } = new();

      public Signal(int id, SignalLife signalLife)
      {
        AddProvider(id, signalLife);
      }

      public void AddProvider(int id, SignalLife signalLife)
      {
        keys[id] = false;
        IDLifes[id] = signalLife;
      }

      public void Acquire(int id, out bool isActivated)
      {
        if (keys[id] == true)
        {
          isActivated = false;
          return;
        }          

        keys[id] = true;
        idActivateEvent?.Invoke(id);

        isActivated = IsActivated();
        if (isActivated)
        {
          var isACDC = IDLifes.Any(pair => pair.Value == SignalLife.ActivateAndDeactivate);
          activateEvent?.Invoke(isACDC);
        }          
      }

      public void Release(int id)
      {
        if (keys[id] == false)
          return;
        idDeactivateEvent?.Invoke(id);

        if (IsActivated())
        {
          var isACDC = IDLifes.Any(pair => pair.Value == SignalLife.ActivateAndDeactivate);
          deactivateEvent?.Invoke(isACDC);
        }          

        keys[id] = false;
      }

      public void ResetActiveCount()
      {
        var originKeys = keys.Keys.ToList();
        foreach (var key in originKeys)
          keys[key] = false;
      }

      private bool IsActivated()
      {
        foreach (var value in keys.Values)
          if (!value)
            return false;

        return true;
      }
    }

    private readonly Dictionary<int, Signal> signals = new();

    #region ISignalKeyRegister
    public void RegisterKey(int key, int id, SignalLife signalLife)
    {
      if (signals.TryGetValue(key, out var set))
        set.AddProvider(id, signalLife);
      else
        signals[key] = new Signal(id, signalLife);
    }
    #endregion

    #region ISignalConsumer
    public void AcquireSignal(int key, int id, out bool isFinalSignal)
    {
      signals[key].Acquire(id, out var isActivated);
      isFinalSignal = isActivated;
    }

    public void ReleaseSignal(int key, int id)
    {
      signals[key].Release(id);
    }

    public void ResetAllSignal()
    {
      foreach (var eventSet in signals.Values)
        eventSet.ResetActiveCount();
    }
    #endregion

    #region ISignalSubscriber
    public void SubscribeSignalActivate(int key, UnityAction<bool> activate)
    {
      signals[key]
        .activateEvent
        .AddListener(activate);
    }

    public void UnsubscribeSignalActivate(int key, UnityAction<bool> activate)
    {
      signals[key]
        .activateEvent.
        RemoveListener(activate);
    }

    public void SubscribeSignalDeactivate(int key, UnityAction<bool> deactivate)
    {
      signals[key]
        .deactivateEvent
        .AddListener(deactivate);
    }

    public void UnsubscribeSignalDeactivate(int key, UnityAction<bool> deactivate)
    {
      signals[key]
        .deactivateEvent
        .RemoveListener(deactivate);
    }

    public void SubscribeIDActivate(int key, int id, UnityAction<int> activate)
    {
      signals[key]
        .idActivateEvent
        .AddListener(activate);
    }

    public void UnsubscribeIDActivate(int key, int id, UnityAction<int> activate)
    {
      signals[key]
        .idActivateEvent
        .RemoveListener(activate);
    }

    public void SubscribeIDDeactivate(int key, int id, UnityAction<int> deactivate)
    {
      signals[key]
        .idDeactivateEvent
        .AddListener(deactivate);
    }

    public void UnsubscribeIDDeactivate(int key, int id, UnityAction<int> deactivate)
    {
      signals[key]
        .idDeactivateEvent
        .RemoveListener(deactivate);
    }

    #endregion

    #region ISignalLifesProvider
    public Dictionary<int, SignalLife> GetSignalIDLifes(int key)
      => signals[key]
        .IDLifes;
    #endregion
  }
}