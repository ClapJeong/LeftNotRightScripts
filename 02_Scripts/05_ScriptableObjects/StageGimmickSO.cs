using LR.Table.StageGimmick;
using UnityEngine;

[CreateAssetMenu(fileName = "StageGimmickSO", menuName = "SO/StageGimmick")]

public class StageGimmickSO : ScriptableObject
{
  [field: SerializeField] public CameraBalanceData CameraBalanceData { get; private set; }
  [field: SerializeField] public CameraMoverData CameraMoverData { get; private set; }
  [field: SerializeField] public QTEBombData QTEBombData { get; private set; }
  [field: SerializeField] public InputRequireData InputRequireData { get; private set; }
  [field: SerializeField] public SwapData SwapData { get; private set; }
}
