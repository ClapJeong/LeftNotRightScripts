using UnityEngine;

namespace LR.Table.StageGimmick
{
  [System.Serializable]
  public class CameraBalanceData
  {
    [field: SerializeField] public float MaxRotation { get; private set; }
    [field: SerializeField] public float RotateSpeed {  get; private set; }
    [field: SerializeField] public float RevertDuration {  get; private set; }
    [field: SerializeField] public float LerpValue {  get; private set; }
  }
}
