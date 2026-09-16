using UnityEngine;

[CreateAssetMenu(fileName = "WallHitObjectSO", menuName = "SO/WallHitObject")]

public class WallHitObjectSO : ScriptableObject
{
  [field: SerializeField] public float CreateRange { get; private set; }
  [field: SerializeField] public float CreateHeight { get; private set; }
  [field: SerializeField] public float EmmisionMin { get; private set; }
  [field: SerializeField] public float EmmisionMax { get; private set; }
  [field: SerializeField] public float EmmisionRange { get; private set; }
  [System.Serializable]
  public struct SizeRange
  {
    [field: SerializeField] public float Min { get; private set; }
    [field: SerializeField] public float Max { get; private set; }
  }
  [field: SerializeField] public SizeRange SizeMin { get; private set; }
  [field: SerializeField] public SizeRange SizeMax { get; private set; }
}
