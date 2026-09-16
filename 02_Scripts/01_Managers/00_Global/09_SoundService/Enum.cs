
namespace LR.Manager.Sound
{
  public enum AudioSourceType
  {
    Left,
    Center,
    Right,
    LeftUI,
    CenterUI,
    RightUI,
  }

  public enum VolumeType
  {
    Master,
    BGM,
    SFX,
  }

  public enum SFX
  {
    IndicatorMove,
    UIGoodSubmit,
    UIBadSubmit,

    __DialogueLeftTalking,
    __DialogueCenterTalking,
    __DialogueRightTalking,

    __StageBeginFirstSelect,
    __StageBeginLastSelect,
    StageRestart,
    StageFail,
    StageComplete,

    LMove,
    LWallHit,
    LBulletHit,
    LElectric,
    RMove,
    RWallHit,
    RBulletHit,
    RElectric,

    SignalTriggerEnter,
    __SignalTriggerACDCWorking,
    SignalTriggerACDCExit,
    SignalListener,

    CaptchaQTEFirst,
    CaptchaQTESecond,
    CaptchaProgress,
    
    CaptchaFail,

    LaserCharge,
    LaserShoot,    

    GimmickExplosion,
    __InputRequireFuse,
    QTEBombRandom,
    QTEBombLeftSelected,
    QTEBombRightSelected,
    __QTEBombInputting,
    SwapChanged,
    SwapReverted,

    EpilogueExplosion,
    ClearTurnOn,
    StageTurnOn,
    DoctorRun,
    DoctorDoor,

    FirstClear,
    LastClear,

    Earthquake,

    Clock_0,
    Clock_1,

    LaserPointing,

    FirstCutsceneBell,   
    FirstCutsceneRain,
    FirstCutsceneThunder,
    FirstCutsceneShovel,
    FirstLever,
    FirstLRActivate,
    FirstMachineOn,

    Portal,

    ExitDoorOpen,
    FirstElectric,
    FirstElectricStick,
    FirstAlert,
    FirstNightAnimal,

    LShooter,
    RShooter,
    
    SpeedRunComplete,

    PerfectSuccess,
    PerfectFail,
  }

  public enum BGM
  {
    HazardJoy,
    BGM1,
    BGM2,
    BGM3,
  }
}
