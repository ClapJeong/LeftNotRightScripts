using UnityEngine;

namespace LR.Table.Dialogue
{
  [System.Serializable]
  public class BackgroundShakeData
  {
    [field: SerializeField] public float EmissionMin { get; private set; }
    [field: SerializeField] public float EmissionMax { get; private set; }
    [field: SerializeField] public float CreateHeightSpace { get; private set; }
    [field: SerializeField] public float CreateWidthSpace { get; private set; }
    [field: SerializeField] public float SpeedMin { get; private set; }
    [field: SerializeField] public float SpeedMax { get; private set; }
    [field: SerializeField] public float SpeedModifyRange { get; private set; }
    [field: SerializeField] public float DurationMin { get; private set; }
    [field: SerializeField] public float DurationMax {  get; private set; }
    [field: SerializeField] public float SizeMin { get; private set; }
    [field: SerializeField] public float SizeMax { get; private set; }
    [field: SerializeField] public float RotateMin {  get; private set; }
    [field: SerializeField] public float RotateMax { get; private set; }
    [field: SerializeField] public float BeginAlphaMin { get; private set; }
    [field: SerializeField] public float AudioFadeInDuration { get; private set; }
    [field: SerializeField] public float AudioFadeOutDuration { get; private set; }
  }
}
