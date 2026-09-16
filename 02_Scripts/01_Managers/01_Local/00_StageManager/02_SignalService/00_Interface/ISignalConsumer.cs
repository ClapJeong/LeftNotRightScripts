namespace LR.Manager.Stage.Signal
{
  public interface ISignalConsumer
  {
    public void AcquireSignal(int key, int triggerID, out bool isFinalSignal);

    public void ReleaseSignal(int key, int triggerID);

    public void ResetAllSignal();
  }
}