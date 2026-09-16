using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using LR.UI.Enum;
using Zenject;

namespace LR.UI
{
  [RequireComponent(typeof(RectTransform))]
  public abstract class BaseUIView : MonoBehaviour, IUIView
  {
    private RectTransform rectTransform;
    public RectTransform RectTransform
    {
      get
      {
        if(rectTransform == null)
          rectTransform = GetComponent<RectTransform>();
        return rectTransform;
      }
    }

    protected VisibleState visibleState = VisibleState.None;

    protected Dictionary<ViewEvent, UnityEvent> events = new();

    private UISO uiSO;
    protected UISO UISO
    {
      get
      {
        if (uiSO == null)
          uiSO = GlobalManager.instance.Table.UISO;

        return uiSO;
      }
    }

    public VisibleState GetVisibleState()
      => visibleState;

    public abstract UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default);

    public abstract UniTask HideAsync(bool isImmediately = false, CancellationToken token = default);

    public void SubscribeEvent(ViewEvent eventType, UnityAction action)
    {
      if(events.TryGetValue(eventType, out var unityEvent))
      {
        unityEvent.AddListener(action);
      }
      else
      {
        events[eventType] = new UnityEvent();
        events[eventType].AddListener(action);
      }
    }

    public void UnsubscribeEvent(ViewEvent eventType, UnityAction action)
    {
      if(events.TryGetValue(eventType, out var unityEvent))
        unityEvent.RemoveListener(action);
    }

    public void DestroySelf()
      => Destroy(gameObject);
  }
}