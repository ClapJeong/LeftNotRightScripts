using UnityEngine;

[CreateAssetMenu(fileName = "StageCompleteDataSO", menuName = "SO/StageCompleteData")]
public class StageCompleteDataSO : ScriptableObject
{
  [field: Header("[ Exhaust ]")]
  [field: SerializeField] public float ExhaustTimeScale { get; private set; }
  [field: SerializeField] public float ExhaustInDelay { get; private set; }
  [field: SerializeField] public float ExhaustInDuration { get; private set; }
  [field: SerializeField] public float ExhaustWaitDelay { get; private set; }
  [field: SerializeField] public float ExhaustOutDuration { get; private set; }

  [field: Header("[ Complete ]")]
  [field: SerializeField] public float ZoomPositionRatio { get; private set; }
  [field: SerializeField] public float FirstDelay { get; private set; }
  [field: SerializeField] public float DelayAfterSignalWork { get; private set; }
  [field: SerializeField] public float DoorOpenDuration { get; private set; }
  [field: SerializeField] public float DelayAfterExitOpen { get; private set; }
  [field: SerializeField] public float DoctorWalkSpeed { get; private set; }
  [field: SerializeField] public float DoctorWalkHideDuration { get; private set; }
  [field: SerializeField] public float DoctorRunChargingDuration { get; private set; }
  [field: SerializeField] public float DoctorRunSpeed { get; private set; }
  [field: SerializeField] public float DoctorRunHideDuration { get; private set; }
  [field: SerializeField] public float DelayAfterDoctorWork { get; private set; }
  [field: SerializeField] public AnimationCurve ZoomCurve { get; private set; }
  [field: SerializeField] public float ZoomInRatio { get; private set; }
  [field: SerializeField] public float ZoomInDuration { get; private set; }  
  [field: SerializeField] public float ZoomOutDuration { get; private set; }
  [field: SerializeField] public float DocMoveDelayRatio { get; private set; }
  [field: SerializeField] public float EpilogueUIShowDelay {  get; private set; }
  [field: SerializeField] public float EpiloguecompleteCreditDelay { get; private set; }
}
