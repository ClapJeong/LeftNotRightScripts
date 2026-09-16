using UnityEngine;

[CreateAssetMenu(fileName = "EffectTableSO", menuName = "SO/EffectTable")]

public class EffectTableSO : ScriptableObject
{
  [field: SerializeField] public int PoolingCount {  get; private set; }
  [field: SerializeField] public AnimEffectTable AnimEffectTable { get; private set; }
}
