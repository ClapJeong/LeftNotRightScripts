namespace LR.Manager.Device
{
  public interface IDeviceProvider
  {
    public LRDeviceType CurrentDeviceType { get; }
  }
}
