namespace LR.Manager.Sound
{
  public interface IVolumeProvider
  {
    public float GetNormalizedVolume(VolumeType volumeType);
  }
}
