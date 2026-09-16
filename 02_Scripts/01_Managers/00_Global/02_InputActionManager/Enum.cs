namespace LR.Manager.Input
{
  public enum LRInputType
  {
    LeftAny,
    LeftUp,
    LeftRight,
    LeftDown,
    LeftLeft,

    RightAny,
    RightUp,
    RightRight,
    RightDown,
    RightLeft,

    Pause,
    StageRestart,
    DialogueSkip,
    Practice,
    UISubmit,
  }

  public enum InputPhase
  {
    Performed,
    Canceled,
  }

  public enum RightInputState
  {
    Arrow,
    JIKL,
  }
}
