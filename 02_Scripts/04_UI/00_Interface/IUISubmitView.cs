using UnityEngine;
using UnityEngine.Events;

namespace LR.UI
{
  public interface IUISubmitView
  {
    public RectTransform RectTransform { get; }

    public void Enable(bool isEnable);

    public bool IsEnable();

    public void Perform();
    public void Perform(Direction direction);    

    public void Subscribe(UnityAction onPerformed);

    public void Unsubscribe(UnityAction onPerformed);

    public void UnsubscribeAll();

    public void OnSelect(Direction previousDirection);

    public void OnSelect();

    public void OnExit();
  }
}