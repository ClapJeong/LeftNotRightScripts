namespace LR.Manager.Sound
{
  public interface IVolumeController
  {
    public void SetVolume(VolumeType volumeType, float normalized);
  }
}
