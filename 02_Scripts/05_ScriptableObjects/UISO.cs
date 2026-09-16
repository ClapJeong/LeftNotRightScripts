using UnityEngine;

[CreateAssetMenu(fileName = "UISO", menuName = "SO/UI")]
public class UISO : ScriptableObject
{
  [System.Serializable]
  public class IndicatorData
  {
    [field: SerializeField] public float RectTransformMoveDuration { get; private set; }
    [field: SerializeField] public float RectTransformDecreaseNormalized { get; private set; }
    [field: SerializeField] public float RectTransformIncreaseNormalized { get; private set; }
    [field: SerializeField] public Vector3 RectTransformDecreasedScale { get; private set; }
    [field: SerializeField] public float GuideMoveDuration { get; private set; }
    [field: SerializeField] public float LeftGuideDefaultSpace { get; private set; }
    [field: SerializeField] public float LeftGuideMoveLength { get; private set; }
    [field: SerializeField] public float RightGuideMoveValue { get; private set; }

    [field: Space(5)]
    [field: SerializeField] public float SubmitPingpongDuration {  get; private set; }
    [field: SerializeField] public float RightSubmitPingpongLength {  get; private set; }
    [field: SerializeField] public float UISubmitScaleValue { get; private set; }
    [field: SerializeField] public float SubmitIdleWiggleRange { get; private set; }
    [field: SerializeField] public float SubmitIdleWiggleSpeed { get; private set; }
    [field: SerializeField] public float ScrollSpeed { get; private set; }

    [field: SerializeField] public Vector3 SelectEuler {  get; private set; }
    [field: SerializeField] public int SelectEulerCount { get; private set; }
    [field: SerializeField] public AnimationCurve SelectEulerCurve { get; private set; }
    [field: SerializeField] public float SelectRotateDuration {  get; private set; }
  }
  [field: Space(5)]
  [field: SerializeField] public IndicatorData Indicator { get; private set; } 

  [field: Space(5)]
  [field: SerializeField] public float LoadingFadeDuration { get; private set; }

  [System.Serializable]
  public class VolumeControlData
  {
    [field: SerializeField] public float DisableAlpha { get; private set; }
    [field: SerializeField] public float DisableScale {  get; private set; }
    [field: SerializeField] public float ScaleDuration { get; private set; }
    [field: SerializeField] public float VolumeUnit { get; private set; }
  }
  [field: SerializeField] public VolumeControlData VolumeControl { get; private set; }

  [System.Serializable]
  public class LobbyData
  {
    [field: Header("[ Logos ]")]   
    [field: SerializeField] public float LogoStateChangeDuration { get; private set; }
    [field: SerializeField] public Vector2 LogoIconHidePos { get; private set; }
    [field: SerializeField] public Vector2 LogoLeftStagePos { get; private set; }
    [field: SerializeField] public Vector2 LogoRightStagePos { get; private set; }
    [field: SerializeField] public Vector2 LogoLeftHidePos { get; private set; }
    [field: SerializeField] public Vector2 LogoRightHidePos { get; private set; }
    [field: SerializeField] public Vector2 LogoDoctorHidePos { get; private set; }
    [field: SerializeField] public float GlassMoveLength { get; private set; }
    [field: SerializeField] public float SpeedRunPortraitScaleValue { get; private set; }
    [field: SerializeField] public float SpeedRunPortraitScaleYoyoInterval { get; private set; }
    [field: SerializeField] public float StagePortraitScaleValue { get; private set; }
    [field: SerializeField] public float StagePortraitScaleDuration { get; private set; }

    [field: Header("[ Portrait ]")]
    [field: SerializeField] public float PortraitYoyoLength { get; private set; }
    [field: SerializeField] public float PortraitYoyoEuler { get; private set; }
    [field: SerializeField] public float PortraitYoyoDuration { get; private set; }
    [field: SerializeField] public float PortraitInputScaleDuration { get; private set; }
    [field: SerializeField] public float PortraitInputScaleValue { get; private set; }

    [field: Header("[ Background ]")]
    [field: SerializeField] public float BackgroundLeftOffsetSpeed { get; private set; }
    [field: SerializeField] public float BackgroundRightOffsetDuration { get; private set; }
    [field: SerializeField] public float BackgroundRightOffsetLength { get; private set; }

    [field: Header("[ Panels ]")]
    [field: SerializeField] public float PanelShowDelay { get; private set; }
    [field: SerializeField] public float PanelShowDuration { get; private set; }
    [field: SerializeField] public float PanelHideDuration { get; private set; }    
    [field: Space(5)]
    [field: SerializeField] public float MainPanelSubmitMoveDelay { get; private set; }
    [field: SerializeField] public float MainPanelButtonHideScale {  get; private set; }
    [field: SerializeField] public float MainPanelButtonScaleDuration { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public float StageButtonShowDuration { get; private set; }
    [field: SerializeField] public float StageButtonShowInterval { get; private set; }
    [field: SerializeField] public float StageButtonShowHideLength { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public float PreviewWiggleRange { get; private set; }
    [field: SerializeField] public float PreviewWiggleSpeed { get; private set; }
    [field: SerializeField] public float StageBeginDelay { get; private set; }

    [System.Serializable]
    public class StageButtonData
    {
      [field: SerializeField] public Vector3 ShowScale { get; private set; }
      [field: SerializeField] public Vector3 HideScale { get; private set; }
      [field: SerializeField] public float MoveDuration { get; private set; }
      [field: SerializeField] public float ButtonSetUnselectAlpha { get; private set; }
    }
    [field: SerializeField] public StageButtonData StageButton {  get; private set; }

    [field: Header("[ Dialogue Shake ]")]
    [field: SerializeField] public float DialogueShakeDuration { get; private set; }
    [field: SerializeField] public float DialogueShakeStrength { get; private set; }
    [field: SerializeField] public int DialogueShakeVibrato { get; private set; }
    [field: SerializeField] public float DialogueShakeRandom { get; private set; }

    [field: Header("[ Dialogue Replay ]")]
    [field: SerializeField] public float DialogueReplayHideLength { get; private set; }
    [field: SerializeField] public float DialogueReplayShowDuration { get; private set; }
    [field: SerializeField] public float DialogueReplayShowInterval { get; private set; }

    [field: Header("[ Data Reset ]")]
    [field: SerializeField] public float DataResetPanelChangeDuration { get; private set; }
  }
  [field: SerializeField] public LobbyData Lobby { get; private set;  }  

  [System.Serializable]
  public class StageData
  {
    [field: SerializeField] public float UIMoveDefaultDuration { get; private set; }
    [field: Header("[ Begin ]")]
    [field: SerializeField] public float BeginFadeDuration { get; private set; }

    [field: Header("[ Fail ]")]
    [field: SerializeField] public float FailUIQuitThrottle { get; private set; }

    [field: Header("[ Restart ]")]
    [field: SerializeField] public float RestartDelay { get; private set; }
    [field: SerializeField] public float RestartMatDuration {  get; private set; }
    [field: SerializeField] public float RestartMatSize { get; private set; }
    [field: SerializeField] public float RestartTextMoveLength {  get; private set; }
    [field: SerializeField] public float RestartIconMoveLength { get; private set; }
    [field: SerializeField] public float RestartUIMoveDuration {  get; private set; }
    [field: SerializeField] public float RestartUIFadeDuration { get; private set; }
    [field: Header("[ Speedrun ]")]
    [field: SerializeField] public float SpeedrunPortraitHidePositon { get; private set; }
    [field: SerializeField] public float SpeedrunPortraitShowDuration { get; private set; }
    [field: SerializeField] public float SpeedrunExitButtonDelay { get; private set; }
    [field: SerializeField] public float SpeedrunExitButtonHidePositon { get; private set; }
    [field: SerializeField] public float SpeedrunExitButtonShowDuration { get; private set; }
    [field: Header("[ Fail ]")]
    [field: SerializeField] public float FailViewDelay { get; private set; }
    [field: Header("[ Practice ]")]
    [field: SerializeField] public float PracticeIconScaleValue { get; private set; }
    [field: SerializeField] public float PracticeIconScaleDuration { get; private set; }

    [field: Header("[ Perfect ]")]
    [field: SerializeField] public float PerfectIconFadeDuration { get; private set; }
    [field: SerializeField] public float PerfectFailTextMoveDuration { get; private set; }
    [field: SerializeField] public float PerfectFailTextMoveLength { get; private set; }
    [field: SerializeField] public float PerfectFailTextFadeBeginDurationRatio { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public float PerfectLogoScaleDelay { get; private set; }
    [field: SerializeField] public float PerfectLogoScaleValue { get; private set; }
    [field: SerializeField] public float PerfectLogoScaleDuration { get; private set; }
    [field: Header("[ Perfect ]")]
    [field: SerializeField] public float StarBeginScale { get; private set; }
    [field: SerializeField] public float StarBeginAlpha { get; private set; }
    [field: SerializeField] public float StarBeginDelay { get; private set; }
    [field: SerializeField] public float StarSclaeDuration { get; private set; }
    [field: SerializeField] public float StarPumpScale { get; private set; }
    [field: SerializeField] public float StarPumpDuration { get; private set; }
    [field: Header("[ World Record ]")]
    [field: SerializeField] public float NewScorePumpDuration { get; private set; }
    [field: SerializeField] public float NewScoreScaleValue { get; private set; }
    [field: SerializeField] public float LeaderboardScrollDuration { get; private set; }
  }
  [field: SerializeField] public StageData Stage { get; private set; }

  [System.Serializable]
  public class StageGimmickData
  {
    [field: SerializeField] public float HideLength {  get; private set; }
    [field: SerializeField] public float FadeDuration {  get; private set; }
    [field: SerializeField] public float MoveDuration { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public float GimmickFailShakeDuration { get; private set; }
    [field: SerializeField] public float GimmickFailShakeStrength { get; private set; }
    [field: SerializeField] public int GimmickFailShakeVibrato { get; private set; }

    [field: Header("[ CameraMover ]")]
    [field: SerializeField] public float CameraMoverUpdateDuration { get; private set; }
    [field: SerializeField] public float CameraMoverMaxEuler { get; private set; }

    [field: Header("[ QTE Bomb ]")]
    [field: SerializeField] public float QTEBombRandomShowInterval { get; private set; }
    [field: SerializeField] public float QTEBombPunchScale { get; private set; }
    [field: SerializeField] public float QTEBombPunchDuration { get; private set; }
    [field: SerializeField] public float QTEBombIconDisableAlpha { get; private set; }

    [field: Header("[ Input Require ]")]
    [field: SerializeField] public float InputRequireShakeDuration { get; private set; }
    [field: SerializeField] public float InputRequireShakeStrenghMin { get; private set; }
    [field: SerializeField] public float InputRequireShakeStrenghMax { get; private set; }
    [field: SerializeField] public int InputRequireShakeVibratoMin { get; private set; }
    [field: SerializeField] public int InputRequireShakeVibratoMax { get; private set; }
    [field: SerializeField] public int InputRequireDecreaseSpeed { get; private set; }
    [field: SerializeField] public int InputRequireIncreaseSpeed { get; private set; }

    [field: Header("[ Input Require ]")]
    [field: SerializeField] public float SwapDuration { get; private set; }
    [field: SerializeField] public float SwapScale { get; private set; }
  }
  [field: SerializeField] public StageGimmickData StageGimmick { get; private set; }

  [System.Serializable]
  public class PlayerData
  {
    [field: SerializeField] public float FadeDuration { get; private set; }
    [field: SerializeField] public float MoveDuration { get; private set; }

    [field: SerializeField][field: Range(0.0f, 1.0f)] public float PortraitLowEnergy { get; private set; }
    [field: SerializeField] public int EnergyChangedBlinkCount { get; private set; }
    [field: SerializeField] public float EnergyChangedUIDuration { get; private set; }

    [field: Header("[ Damaged ]")]
    [field: SerializeField] public float DamagedScaleValue { get; private set; }
    [field: SerializeField] public float DamagedScaleDuration { get; private set; }
    [field: SerializeField] public float DamagedPortraitShakeDuration { get; private set; }
    [field: SerializeField] public float DamagedPortraitShakeStrengh { get; private set; }
    [field: SerializeField] public int DamagedPortraitShakeVibrato { get; private set; }

    [field: Header("[ Stun ]")]
    [field: SerializeField] public float StunBlinkDuration {  get; private set; }
    [field: SerializeField] public float StunBlinkAlpha { get; private set; }

    [field: Header("[ Vignette ]")]
    [field: SerializeField] public float WallBumpVignetteAlpha { get; private set; }
    [field: SerializeField] public float DamagedVignetteAlpha { get; private set; }

    [field: Header("[ Exhausted ]")]
    [field: SerializeField] public Vector2 ExhaustedHidePosition { get; private set; }
    [field: SerializeField] public float ExhaustedMoveDuration { get; private set; }
    [field: SerializeField] public AnimationCurve ExhaustedMoveCurve { get; private set; }

    [field: Header("[ Electric ]")]
    [field: SerializeField] public float ElectricShakeInterval { get; private set; }
    [field: SerializeField] public float ElectricShakeRange { get; private set; }

    [field: Header("[ Clear ]")]
    [field: SerializeField] public float ClearEnterLength { get; private set; }
    [field: SerializeField] public float ClearEnterUpduration { get; private set; }
    [field: SerializeField] public float ClearEnterDownDuration { get; private set; }

    [field: Header("[ Input ]")]
    [field: SerializeField] public float InputIconScale {  get; private set; }
    [field: Space(5)]
    [field: SerializeField] public float InputFadeDuration { get; private set; }
    [field: SerializeField] public float InputMoveLength { get; private set; }
    [field: SerializeField] public float InputMoveDuration {  get; private set; }
    [field: SerializeField] public float InputMoveDelay {  get; private set; }

    [field: Space(5)]
    [field: SerializeField] public float InputFailDelay {  get; private set; }
    [field: SerializeField] public float InputFailMoveDuration { get; private set; }
    [field: SerializeField] public float InputFailMoveLength { get; private set; }
    [field: SerializeField] public float InputFailFadeDuration { get; private set; }

    [field: Header("[ Speedrun ]")]
    [field: SerializeField] public float SpeedrunHidePosition { get; private set; }
    [field: SerializeField] public float SpeedrunHideDuration { get; private set; }

    [field: Header("[ Damage Log ]")]
    [field: SerializeField] public DamageLogData DamageLog { get; private set; }

    [System.Serializable]
    public class ShaderData
    {
      [field: SerializeField] public float AlphaMin { get; private set; }
      [field: SerializeField] public float AlphaMax { get; private set; }
      [field: SerializeField] public float FadeDurationMin { get; private set; }
      [field: SerializeField] public float FadeDurationMax { get; private set; }

      [field: SerializeField] public float DefaultValueChangeDuration { get; private set; }
      [field: Header("[ Idle ]")]
      [field: SerializeField] public float IdleScanStrength {  get; private set; }
      [field: SerializeField] public float IdleRGB { get; private set; }
      [field: SerializeField] public float IdleGlitchStrength { get; private set; }
      [field: SerializeField] public float IdleEdgeNoiseStrength { get; private set; }
      [field: SerializeField] public float IdleEdgeNoiseWidth { get; private set; }
      [field: SerializeField] public float IdleFlicker { get; private set; }

      [field: Header("[ SoftHit ]")]
      [field: SerializeField] public float SoftHitScanStrength { get; private set; }
      [field: SerializeField] public float SoftHitRGBRange { get; private set; }
      [field: SerializeField] public float SoftHitRGBSpeed { get; private set; }
      [field: SerializeField] public float SoftHitGlitchStrength { get; private set; }
      [field: SerializeField] public float SoftHitEdgeNoiseStrength { get; private set; }
      [field: SerializeField] public float SoftHitEdgeNoiseWidth { get; private set; }

      [field: Header("[ StrongHit ]")]
      [field: SerializeField] public float StrongHitScanStrength { get; private set; }
      [field: SerializeField] public float StrongHitRGBRange { get; private set; }
      [field: SerializeField] public float StrongHitRGBSpeed { get; private set; }
      [field: SerializeField] public float StrongHitGlitchStrength { get; private set; }
      [field: SerializeField] public float StrongHitEdgeNoiseStrength { get; private set; }
      [field: SerializeField] public float StrongHitEdgeNoiseWidth { get; private set; }

      [field: Header("[ Electric ]")]
      [field: SerializeField] public float ElectricGlitchStrength { get; private set; }

      [field: Header("[ Complete ]")]
      [field: SerializeField] public float CompleteScanStrength {  get; private set; }

      [field: Header("[ Stun ]")]
      [field: SerializeField] public float StunRGBRange {  get; private set; }
      [field: SerializeField] public float StunRGBIntervalSpikeDuration { get; private set; }
      [field: SerializeField] public float StunRGBIntervalMin { get; private set; }
      [field: SerializeField] public float StunRGBIntervalMax { get; private set; }
      [field: SerializeField] public float StunFlicker { get; private set; }
      [field: SerializeField] public float StunEdgeNoiseStrength { get; private set; }
      [field: SerializeField] public float StunEdgeNoiseWidth { get; private set; }

    }

    [System.Serializable]
    public class DamageLogData
    {
      [field: SerializeField] public float RandomXRange { get; private set; }
      [field: SerializeField] public float RandomYRange { get; private set; }
      [field: SerializeField] public float LengthPerUnit { get; private set; }
      [field: SerializeField] public float SecondPerUnit { get; private set; }
      [field: SerializeField] public float DamageUnit { get; private set; }
      [field: SerializeField] public float FadeBeginDurationRatio { get; private set; }
      [field: SerializeField] public float Scale { get; private set; }
    }

    [field: Header("[ Shader ]")]
    [field: SerializeField] public ShaderData Shader { get; private set; }
  }
  [field: SerializeField] public PlayerData Player { get; private set; }

  [System.Serializable]
  public class DialogueData
  {
    [field: SerializeField] public float BackgroundChangeDuration { get; private set; }
    [field: Header("[ Background ]")]
    [field: SerializeField] public float LetterBoxDuration { get; private set; }
    [field: SerializeField] public float BackgroundFadeDuration { get; private set; }
    [field: SerializeField] public float BackgroundFadeDelay { get; private set; }
    [field: SerializeField] public float BackgroundShowDelay {  get; private set; }

    [field: Header("[ Input ]")]
    [field: SerializeField] public float InputWaitingAlpha {  get; private set; }
    [field: SerializeField] public float InputGuideMoveLength { get; private set; }
    [field: SerializeField] public float InputGuideMoveDuration { get; private set; }

    [field: Header("[ Box ]")]
    [field: SerializeField] public float OverlayFadeDuration { get; private set; }

    [field: Header("[ Box ]")]    
    [field: SerializeField] public float BoxJumpLength { get; private set; }
    [field: SerializeField] public float BoxJumpDuration { get; private set; }
    [field: SerializeField] public float BoxRotateDelay { get; private set; }
    [field: SerializeField] public float LeftBoxRotateValue { get; private set; }
    [field: SerializeField] public float RightBoxRotateValue { get; private set; }
    [field: SerializeField] public float BoxRotateDuration { get; private set; }
  }
  [field: SerializeField] public DialogueData Dialogue { get; private set; }

  [System.Serializable]
  public class QTEData
  {
    [field: SerializeField] public float CaptchaCrossFadeDuration {  get; private set; }
    [field: SerializeField] public float HideDelay {  get; private set; }
    [field: SerializeField] public float HideDuration { get; private set; }
    [field: SerializeField] public float MoveBeginLength { get; private set; }
    [field: SerializeField] public float MoveEndLength { get; private set; }
    [field: SerializeField] public float IconMoveDuration { get; private set; }
  }
  [field: SerializeField] public QTEData QTE {  get; private set; }

  [System.Serializable]
  public class ProgressData
  {
    [field: SerializeField] public float MinScale { get; private set; }
    [field: SerializeField] public float MaxScale { get; private set; }
    [field: SerializeField] public float InputBlinkDuration { get; private set; }
    [field: SerializeField] public float InputMoveRange {  get; private set; }
    [field: SerializeField] public float ProgressRotation { get; private set; }
    [field: SerializeField] public float HideDelay { get; private set; }
    [field: SerializeField] public float HideDuration { get; private set; }
  }
  [field: SerializeField] public ProgressData Progress { get; private set; }

  [System.Serializable]
  public class EpilogueData
  {
    [field: SerializeField] public float FirstHideDuration { get; private set; }
    [field: SerializeField] public float FirstShowDuration { get; private set; }    
    [field: SerializeField] public float IllustFadeDuration { get; private set; }
    [field: SerializeField] public float InputDisableAlpha { get; private set; }
  }
  [field: SerializeField] public EpilogueData Epilogue { get; private set; }

  [System.Serializable ]
  public class CreditData
  {
    [field: SerializeField] public float ShowHideDuration {  get; private set; }
  }
  [field: SerializeField] public CreditData Credit { get; private set; }
}