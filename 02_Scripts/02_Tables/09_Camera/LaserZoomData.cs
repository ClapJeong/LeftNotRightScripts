using UnityEngine;

namespace LR.Table.Camera
{
  [System.Serializable]
  public class LaserZoomData
  {
    [field: SerializeField] public float ZoomOutValue { get; private set; }
    [field: SerializeField] public float ZoomOutDuration { get; private set; }
    [field: SerializeField] public float RevertDuration { get; private set; }
  }
}
