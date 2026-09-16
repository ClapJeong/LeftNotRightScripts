using System.Collections;
using UnityEngine;

namespace LR.Stage.InteractiveObject.AutoMover
{
  public abstract class BaseMoveController
  {
    public abstract void UpdateDuration(float duration);

    public abstract void OnFixedUpdate(out Vector2 moveDirection);

    public abstract void Reset();
  }
}