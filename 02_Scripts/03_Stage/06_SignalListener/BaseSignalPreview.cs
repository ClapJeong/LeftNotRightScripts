using UnityEngine;

namespace LR.Stage.SignalListener
{
  public abstract class BaseSignalPreview : MonoBehaviour
  {
    public abstract void Initialize(Vector3 worldPosition, Color color);

    public abstract void Activate();

    public abstract void Deactivate();

    public abstract void Restart();
  }
}
