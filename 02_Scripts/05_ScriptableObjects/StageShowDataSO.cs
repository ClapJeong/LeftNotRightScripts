using UnityEngine;

[CreateAssetMenu(fileName = "StageShowDataSO", menuName = "SO/StageShowData")]
public class StageShowDataSO : ScriptableObject
{
  [field: SerializeField] public float ZoomMinSize { get; private set; }
  [field: SerializeField] public float ZoomAdditionalSpace { get; private set; }
  [field: SerializeField] public float ZoomBeginDuration { get; private set; }
  [field: SerializeField] public float MoveBeginDuration { get; private set; }

  [field: Space(5)]
  [field: SerializeField] public float ClearTurnOnDelay { get; private set; }
  [field: SerializeField] public float LightTurnOnDelay { get; private set; }
  [field: SerializeField] public float ZoomOutDelay { get; private set; }

  [field: Space(5)]
  [field: SerializeField] public float ZoomEndDuration { get; private set; }
  [field: SerializeField] public float MoveEndDuration { get; private set; }
}
