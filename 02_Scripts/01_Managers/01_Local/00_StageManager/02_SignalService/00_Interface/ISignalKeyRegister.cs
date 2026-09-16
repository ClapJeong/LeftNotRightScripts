using LR.Stage.TriggerTile.Enum;

namespace LR.Manager.Stage.Signal
{
  public interface ISignalKeyRegister
  {
    public void RegisterKey(int key, int triggerID, SignalLife signalLife);
  }
}