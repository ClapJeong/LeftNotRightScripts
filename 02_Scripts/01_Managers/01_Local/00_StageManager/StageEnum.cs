namespace LR.Manager.Stage
{
  public static class StageEnum
  {
    public enum State
    {
      None,
      Ready,

      Playing,

      Success,
      Fail,

      Pause,

      RestartWait,
    }
  }
}