using Cysharp.Threading.Tasks;

namespace LR.Manager.Local.CameraService
{
  public interface ICameraEffectService
  {
    public enum ImpulseType
    {
      LeftWallBump,
      LeftDamaged,
      RightWallBump,
      RightDamaged,
      GimmickStun,
      ExitOpen,
    }
    public void GenerateImpulse(ImpulseType type, float value = 1.0f);

    public void ResetImpulse();

    public UniTask PlayGimmickStunChromaticAsync();

    public UniTask PlaySwapChromaticAsync();

    public UniTask PlayGrainAsync();

    public void StopGrain();

    public void LaserZoom();

    public enum NoiseType
    {
      Exhaust,
      DialogueShake,
    }

    public void PlayNoise(NoiseType type);

    public void StopNoise();
  }
}
