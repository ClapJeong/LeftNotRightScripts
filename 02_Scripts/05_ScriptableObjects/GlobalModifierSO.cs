using UnityEngine;

[CreateAssetMenu(fileName = "GlobalModifierSO", menuName = "SO/GlobalModifier")]

public class GlobalModifierSO : ScriptableObject
{
  [field: SerializeField] public float StageEnergyOffset { get; private set; }
}
