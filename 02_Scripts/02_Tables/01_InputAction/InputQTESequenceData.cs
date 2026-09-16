using System.Collections.Generic;
using UnityEngine;

namespace LR.Table.Input
{
  [System.Serializable]
  public class InputQTEData
  {
    [field: SerializeField] public InputQTEEnum.UIType UIType { get; private set;  }
    [field: SerializeField] public InputQTEEnum.QTEFaiResultType QTEFailType { get; private set; }
    [field: SerializeField] public int Count {  get; private set; }
    [field: SerializeField] public float SequenceDuration {  get; private set; }
    [field: SerializeField] public float QTEDuration { get; private set; }
    [field: SerializeField]
    public List<Direction> Directions { get; private set; } = new()
    {
      Direction.Up,
      Direction.Right,
      Direction.Down,
      Direction.Left,
    };

    public Direction GetRandomDirection()
      => Directions[Random.Range(0, Directions.Count)];
  }
}