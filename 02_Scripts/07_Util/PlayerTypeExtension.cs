using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Stage.Player.Enum;
using System;

public static class PlayerTypeExtension
{
  public static PlayerType ParseOpposite(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => PlayerType.Right,
      PlayerType.Right => PlayerType.Left,
      _ => throw new System.NotImplementedException(),
    };

  public static CharacterPositionType ParseToCharacterPositionType(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => CharacterPositionType.Left,
      PlayerType.Right => CharacterPositionType.Right,
      _ => throw new System.NotImplementedException(),
    };

  public static AudioSourceType ParseToAudioSourceType(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => AudioSourceType.Left,
      PlayerType.Right => AudioSourceType.Right,
      _ => throw new System.NotImplementedException(),
    };

  public static ICameraEffectService.ImpulseType ParseToWallHitImpulseType(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => ICameraEffectService.ImpulseType.LeftWallBump,
      PlayerType.Right => ICameraEffectService.ImpulseType.RightWallBump,
      _ => throw new NotImplementedException(),
    };

  public static ICameraEffectService.ImpulseType ParseToDamagedImpulseType(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => ICameraEffectService.ImpulseType.LeftDamaged,
      PlayerType.Right => ICameraEffectService.ImpulseType.RightDamaged,
      _ => throw new NotImplementedException(),
    };

  public static SFX ParseToWallHitSFX(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => SFX.LWallHit,
      PlayerType.Right => SFX.RWallHit,
      _ => throw new System.NotImplementedException(),
    };

  public static InstanceEffectType ParseToWallHitEffectType(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => InstanceEffectType.LeftWallHit,
      PlayerType.Right => InstanceEffectType.RightWallHit,
      _ => throw new NotImplementedException(),
    };

  public static SFX ParseToElectricSFX(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => SFX.LElectric,
      PlayerType.Right => SFX.RElectric,
      _ => throw new NotImplementedException(),
    };

  public static SFX ParseToShooterSFX(this PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => SFX.LShooter,
      PlayerType.Right => SFX.RShooter,
      _ => throw new NotImplementedException(),
    };
}