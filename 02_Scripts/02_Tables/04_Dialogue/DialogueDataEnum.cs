
public static class DialogueDataEnum
{
  public static class Portrait
  {
    public enum Emotion
    {
      None,
      Sweat,
      Angry,
      Surprised,
      Depressed,
      LeftCaptcha,
      RightCaptcha,
      LeftCaptchaAppear,
      RightCaptchaAppear,
    }

    public enum Left
    {
      Null,

      IdleForward,
      SpeakForward,
      SpeakBadForward,
      SpeakBadPointForward,
      BadForward,
      EhForward,
      GazeForward,
      DarkForward,
      DprForward,

      IdleDoc,
      SpeakDoc,
      SpeakBadDoc,
      BadDoc,
      EhDoc,
      GazeDoc,
      DarkDoc,
      Wonder,
      DprDoc,
      DprDocSmile,
      DprIdle,

      Sleep,
      Shocking,
      ShoutAngry,
      WonderLkUp,
      EyeHide,
      Smile,

      KnockOut,
      KnockOutLkUp,
      KnockOutLkDoc,
      KnockOutLkDocDark,
      KnockOutPntDoc,
      KnockOutGrabHead,
    }

    public enum Right
    {
      Null,

      IdleForward,
      SpeakForward,
      SpeakBadForward,
      SpeakBadPointForward,
      BadForward,
      EhForward,
      GazeForward,    
      DarkForward,
      DprForward,
      
      IdleDoc,
      SpeakDoc,
      SpeakBadDoc,
      BadDoc,
      EhDoc,
      GazeDoc,
      DarkDoc,
      Wonder,
      DprDoc,
      DprDocSmile,
      DprIdle,

      Sleep,
      Shocking,
      ShoutAngry,
      WonderLkUp,      
      EyeHide,      
      Smile,

      KnockOut,
      KnockOutLkUp,
      KnockOutLkDoc,
      KnockOutLkDocDark,
      KnockOutPntDoc,
      KnockOutGrabHead,
    }

    public enum Center
    {
      Null,
      Idle,
      DspHead,
      ShoutSad,
      Smile,
      WeirdSmile,
      SmileShine,
      Worry,
      WorryLookUp,
      Thinking,
      EyeHide,
      Surprised,
      VerySurprised,
      Scared,
      ScaredOff,
      Confuse,
      Speak,
      SpeakPointUP,
      Closed,

      IdleScreen,
      WeirdSmileScreen,
      ThinkingScreen,
      SurprisedScreen,
      WorryScreen,
      Run,
    }

    public enum ChangeType
    {
      None,
      Fade,
      Move,
    }

    public enum AnimationType
    {
      None,
      Surprised,
      Jump,
      Jumping,
      Shake,
      Shaking,
      Run,
      Speak,
    }

    public enum AlphaType
    {
      Max,
      Ahlpha75,
      Ahlpha50,
      Min,
    }
  }

  public static class Background
  {
    public enum Shadow
    {
      None,
      Doctor,
      LR,
    }
  }  
}
