using UnityEngine;

namespace LR.Table.Camera
{
  [System.Serializable]
  public class RendererVolumeData
  {
    [field: Header("[ Chromatic - GimmickStun ]")]
    [field: SerializeField] public float GimmickStunChromaticMaxIntensity { get; private set; }
    [field: SerializeField] public float GimmickStunChromaticDuration { get; private set; }

    [field: Header("[ Chromatic - Swap ]")]
    [field: SerializeField] public float SwapChromaticMaxIntensity { get; private set; }
    [field: SerializeField] public float SwapChromaticDuration { get; private set; }

    [field: Header("[ Grain ]")]
    [field: SerializeField] public float GrainPlayduration {  get; private set; }
    [field: SerializeField] public float GrainMaxValue {  get; private set; }
  }
}
