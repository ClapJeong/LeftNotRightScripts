using LR.Table.Camera;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraDataSO", menuName = "SO/CameraData")]
public class CameraDataSO : ScriptableObject
{
  [field: SerializeField] public ImpulseData ImpulseData { get; private set; }

  [field: SerializeField] public RendererVolumeData rendererVolumeData { get; private set; }

  [field: SerializeField] public LaserZoomData LaserZoomData { get; private set; }
}
