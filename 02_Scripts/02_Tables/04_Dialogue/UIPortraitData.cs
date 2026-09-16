using UnityEngine;

namespace LR.Table.Dialogue
{
  [System.Serializable]
  public class UIPortraitData
  {
    [field: Header("ChangeType")]
    [field: SerializeField] public float ChangeDuration { get; private set; }
    [field: SerializeField] public float MoveLength {  get; private set; }
   
    [field: Space(10)]
    [field: Header("AlphaType")]
    [field: SerializeField] public float AlphaMax { get; private set; }
    [field: SerializeField] public float Alpha75 { get; private set; }
    [field: SerializeField] public float Alpha50 { get; private set; }
    [field: SerializeField] public float AlphaMin { get; private set; }

    public float GetAlphaValue(DialogueDataEnum.Portrait.AlphaType type)
      => type switch
      {
        DialogueDataEnum.Portrait.AlphaType.Max => AlphaMax,
        DialogueDataEnum.Portrait.AlphaType.Ahlpha75 => Alpha75,
        DialogueDataEnum.Portrait.AlphaType.Ahlpha50 => Alpha50,
        DialogueDataEnum.Portrait.AlphaType.Min => AlphaMin,        
        _ => throw new System.NotImplementedException()
      };
  }
}