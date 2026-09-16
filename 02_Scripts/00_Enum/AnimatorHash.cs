using LR.Stage.StageDataContainer;
using UnityEngine;

public static class AnimatorHash
{
  public static class LocalRecordCharacter
  {
    public readonly static int Hit = Animator.StringToHash(nameof(Hit));
    public readonly static int Perfect = Animator.StringToHash(nameof(Perfect));
  }

  public static class PerfectNotice
  {
    public readonly static int Idle = Animator.StringToHash(nameof(Idle));
    public readonly static int Fail = Animator.StringToHash(nameof(Fail));
    public readonly static int Success = Animator.StringToHash(nameof(Success));
  }

  public static class Epilogue
  {
    private readonly static string PlayFormat = "Play_{0}";
    public static int GetPlayHash(int index) => Animator.StringToHash(string.Format(PlayFormat, index));
  }

  public static class WallHitFallObject
  {
    public readonly static int Left = Animator.StringToHash(nameof(Left));
    public readonly static int Right = Animator.StringToHash(nameof(Right));
  }

  public static class PortalEffect
  {
    public readonly static int Idle = Animator.StringToHash(nameof(Idle));
    public readonly static int Inner = Animator.StringToHash(nameof(Inner));
    public readonly static int Outter = Animator.StringToHash(nameof(Outter));
  }

  public static class LaserEffect
  {
    public readonly static int Idle = Animator.StringToHash(nameof(Idle));
    public readonly static int Charge = Animator.StringToHash(nameof(Charge));
    public readonly static int Shoot = Animator.StringToHash(nameof(Shoot));
  }

  public static class Shooter
  {
    public readonly static int UpIdle = Animator.StringToHash(nameof(UpIdle));
    public readonly static int UpShoot = Animator.StringToHash(nameof(UpShoot));

    public readonly static int RightIdle = Animator.StringToHash(nameof(RightIdle));
    public readonly static int RightShoot = Animator.StringToHash(nameof(RightShoot));

    public readonly static int DownIdle = Animator.StringToHash(nameof(DownIdle));
    public readonly static int DownShoot = Animator.StringToHash(nameof(DownShoot));

    public readonly static int LeftIdle = Animator.StringToHash(nameof(LeftIdle));
    public readonly static int LeftShoot = Animator.StringToHash(nameof(LeftShoot));
  }

  public static class Signal
  {
    public readonly static int Activate = Animator.StringToHash(nameof(Activate));
    public readonly static int Deactivate = Animator.StringToHash(nameof(Deactivate));
  }

  public static class StageCompleteDoctor
  {
    public readonly static int Idle = Animator.StringToHash(nameof(Idle));
    public readonly static int Fail = Animator.StringToHash(nameof(Fail));

    public readonly static int Shaking = Animator.StringToHash(nameof(Shaking));
    public readonly static int Running = Animator.StringToHash(nameof(Running));

    public readonly static int None = Animator.StringToHash(nameof(None));

    public readonly static int PlayerHit = Animator.StringToHash(nameof(PlayerHit));
  }

  public static class StageCompleteExit
  {
    public readonly static int Idle = Animator.StringToHash(nameof(Idle));
    public readonly static int Activate = Animator.StringToHash(nameof(Activate));
  }

  public static class StageCompleteSignal
  {
    public readonly static int Idle = Animator.StringToHash(nameof(Idle));
    public readonly static int LeftActivate = Animator.StringToHash(nameof(LeftActivate));
    public readonly static int RightActivate = Animator.StringToHash(nameof(RightActivate));
  }

  public static class GimmickPreviewIcon
  {
    public readonly static int Idle = Animator.StringToHash(nameof(Idle));
    public readonly static int CameraMover = Animator.StringToHash(nameof(CameraMover));
    public readonly static int CameraRotator = Animator.StringToHash(nameof(CameraRotator));
    public readonly static int QTEBomb = Animator.StringToHash(nameof(QTEBomb));
    public readonly static int InputRequire = Animator.StringToHash(nameof(InputRequire));
    public readonly static int Swap = Animator.StringToHash(nameof(Swap));

    public static int GetHash(StageGimmick gimmick)
      => gimmick switch
      {
        StageGimmick.None => Idle,
        StageGimmick.CameraRotator => CameraRotator,
        StageGimmick.CameraMover => CameraMover,
        StageGimmick.QTEBomb => QTEBomb,
        StageGimmick.InputRequire => InputRequire,
        StageGimmick.Swap =>Swap,
        _ => throw new System.NotImplementedException(),
      };
  }

  public static class QTECaptcha
  {
    public readonly static int Activate = Animator.StringToHash(nameof(Activate));
    public readonly static int Deactivate = Animator.StringToHash(nameof(Deactivate));
  }

  public static class CaptchaObject
  {
    public readonly static int SetLeftSprite = Animator.StringToHash(nameof(SetLeftSprite));
    public readonly static int SetRightSprite = Animator.StringToHash(nameof(SetRightSprite));
    public readonly static int Idle = Animator.StringToHash(nameof(Idle));
    public readonly static int Resolving = Animator.StringToHash(nameof(Resolving));
    public readonly static int Resolved = Animator.StringToHash(nameof(Resolved));
    public readonly static int Reanimated = Animator.StringToHash(nameof(Reanimated));
  }

  public static class Dialogue
  {
    public static class Portrait
    {
      public readonly static int Idle = Animator.StringToHash(nameof(Idle));
      public readonly static int Surprised = Animator.StringToHash(nameof(Surprised));
      public readonly static int JumpOnce = Animator.StringToHash(nameof(JumpOnce));
      public readonly static int JumpLoop = Animator.StringToHash(nameof(JumpLoop));
      public readonly static int ShakeOnce = Animator.StringToHash(nameof(ShakeOnce));
      public readonly static int ShakeLoop = Animator.StringToHash(nameof(ShakeLoop));
      public readonly static int Run = Animator.StringToHash(nameof(Run));
      public readonly static int Speak = Animator.StringToHash(nameof(Speak));
    }
  }

  public static class QTEBomb
  {
    public readonly static int Explosion = Animator.StringToHash(nameof(Explosion));
  }

  public static class AutoDoor
  {
    public static readonly int Closed = Animator.StringToHash(nameof(Closed));
    public static readonly int Opening = Animator.StringToHash(nameof(Opening));
    public static readonly int Opened = Animator.StringToHash(nameof(Opened));
    public static readonly int Closing = Animator.StringToHash(nameof(Closing));
  }

  public static class Effect
  {
    public static readonly int Idle = Animator.StringToHash(nameof(Idle));
    public static readonly int Play = Animator.StringToHash(nameof(Play));
  }

  public static class Player
  {
    public static class Parameter
    {
      public static readonly int Horizontal = Animator.StringToHash(nameof(Horizontal));
    }    

    public static class Clip
    {
      public static readonly int Idle = Animator.StringToHash(nameof(Idle));
      public static readonly int MoveBlend = Animator.StringToHash(nameof(MoveBlend));
      public static readonly int Bounced = Animator.StringToHash(nameof(Bounced));
      public static readonly int InputBegin = Animator.StringToHash(nameof(InputBegin));
      public static readonly int Inputing = Animator.StringToHash(nameof(Inputing));
      public static readonly int Stun = Animator.StringToHash(nameof(Stun));
      public static readonly int Clear = Animator.StringToHash(nameof(Clear));
      public static readonly int Exhausted = Animator.StringToHash(nameof(Exhausted));
      public static readonly int WallHit = Animator.StringToHash(nameof(WallHit));
      public static readonly int Perfect = Animator.StringToHash(nameof (Perfect));
    }
  }
}
