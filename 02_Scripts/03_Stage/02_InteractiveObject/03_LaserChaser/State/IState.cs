using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.InteractiveObject.LaserChaserState
{
  public interface IState
  {
    public void OnEnter();

    public void OnUpdate(UnityAction onComplete);

    public void OnExit();

    public void OnPuase();

    public void UpdateTarget(Transform target);
  }
}
