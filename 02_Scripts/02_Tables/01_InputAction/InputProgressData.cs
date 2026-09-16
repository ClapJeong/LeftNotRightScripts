using UnityEngine;

namespace LR.Table.Input
{
  [System.Serializable]
  public class InputProgressData
  {
    [field: SerializeField] public InputProgressEnum.UIType UIType { get; private set; }
    [field: SerializeField] public bool Failable {  get; private set; }
    [field: SerializeField][field: Range(0.0f, 1.0f)] public float BeginValue { get; private set; } = 0.5f;
    [field: SerializeField] public float DecreaseValuePerSecond { get; private set; }
    [field: SerializeField] public float IncreaseValueOnInput { get; private set; }
  }
}