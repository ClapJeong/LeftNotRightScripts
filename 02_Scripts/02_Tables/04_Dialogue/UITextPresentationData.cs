using System;
using UnityEngine;

namespace LR.Table.Dialogue
{
  [System.Serializable]
  public class UITextPresentationData
  {
    [field: SerializeField] public float SkipInputDuration { get; private set; }
    [field: Header("[ Color ]")]
    [field: SerializeField] public Color LeftColor {  get; private set; }
    [field: SerializeField] public Color DoctorColor { get; private set; }
    [field: SerializeField] public Color RightColor { get; private set; }
  }
}
