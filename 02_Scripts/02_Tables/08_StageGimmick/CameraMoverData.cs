using UnityEngine;

namespace LR.Table.StageGimmick
{
  [System.Serializable]
  public class CameraMoverData
  {
    [field: SerializeField] public int ValueUnit;
    [field: SerializeField] public float MinScaleRatio { get; private set; }
    [field: SerializeField] public float ScaleChangeDuration { get; private set; }
    [field: SerializeField] public float MaxLerpSpeed { get; private set; }
  }
}
