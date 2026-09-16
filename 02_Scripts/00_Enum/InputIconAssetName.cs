using LR.Manager.Input;
using LR.Stage.Player.Enum;
using System;

public static class InputIconAssetName
{
  public static string IJKLAdditionalName = "_Sub";

  public static string Left_Up_Idle = nameof(Left_Up_Idle);
  public static string Left_Right_Idle = nameof(Left_Right_Idle);
  public static string Left_Down_Idle = nameof(Left_Down_Idle);
  public static string Left_Left_Idle = nameof(Left_Left_Idle);

  public static string Input_Left_Idle = nameof(Input_Left_Idle);
  public static string Input_Right_Idle = nameof(Input_Right_Idle);
  public static string Pause_Idle = nameof(Pause_Idle);
  public static string Restart_Idle = nameof(Restart_Idle);
  public static string Skip_Idle = nameof(Skip_Idle);

  public static string Right_Up_Idle = nameof(Right_Up_Idle);
  public static string Right_Right_Idle = nameof(Right_Right_Idle);
  public static string Right_Down_Idle = nameof(Right_Down_Idle);
  public static string Right_Left_Idle = nameof(Right_Left_Idle);

  public static string Right_Up_Idle_Sub = Right_Up_Idle + IJKLAdditionalName;
  public static string Right_Right_Idle_Sub = Right_Right_Idle + IJKLAdditionalName;
  public static string Right_Down_Idle_Sub = Right_Down_Idle + IJKLAdditionalName;
  public static string Right_Left_Idle_Sub = Right_Left_Idle + IJKLAdditionalName;

  public static string Left_Up_Input = nameof(Left_Up_Input);
  public static string Left_Right_Input = nameof(Left_Right_Input);
  public static string Left_Down_Input = nameof(Left_Down_Input);
  public static string Left_Left_Input = nameof(Left_Left_Input);

  public static string Input_Left_Input = nameof(Input_Left_Input);
  public static string Input_Right_Input = nameof(Input_Right_Input);
  public static string Pause_Input = nameof(Pause_Input);
  public static string Restart_Input = nameof(Restart_Input);
  public static string Skip_Input = nameof(Skip_Input);

  public static string Right_Up_Input = nameof(Right_Up_Input);
  public static string Right_Right_Input = nameof(Right_Right_Input);
  public static string Right_Down_Input = nameof(Right_Down_Input);
  public static string Right_Left_Input = nameof(Right_Left_Input);

  public static string Right_Up_Input_Sub = Right_Up_Input + IJKLAdditionalName;
  public static string Right_Right_Input_Sub = Right_Right_Input + IJKLAdditionalName;
  public static string Right_Down_Input_Sub = Right_Down_Input + IJKLAdditionalName;
  public static string Right_Left_Input_Sub = Right_Left_Input + IJKLAdditionalName;

  public static string Practice_Idle = nameof(Practice_Idle);
  public static string Practice_Input = nameof(Practice_Input);

  public static string GetInputPreviewName(bool isLeft, bool isIdle)
    => isLeft switch
    {
      true => isIdle ? Input_Left_Idle : Input_Left_Input,
      false => isIdle ? Input_Right_Idle : Input_Right_Input,
    };

  public static string GetInputPreviewName(PlayerType playerType, bool isIdle)
    => playerType switch
    {
      PlayerType.Left => isIdle ? Input_Left_Idle : Input_Left_Input,
      PlayerType.Right => isIdle ? Input_Right_Idle : Input_Right_Input,
      _ => throw new NotImplementedException(),
    };


  public static string GetInputTypeName(LRInputType inputType, bool isIdle)
    => inputType switch
    {
      LRInputType.LeftUp => isIdle ? Left_Up_Idle : Left_Up_Input,
      LRInputType.LeftRight => isIdle ? Left_Right_Idle : Left_Right_Input,
      LRInputType.LeftDown => isIdle ? Left_Down_Idle : Left_Down_Input,
      LRInputType.LeftLeft => isIdle ? Left_Left_Idle : Left_Left_Input,

      LRInputType.RightUp => isIdle ? Right_Up_Idle : Right_Up_Input,
      LRInputType.RightRight => isIdle ? Right_Right_Idle : Right_Right_Input,
      LRInputType.RightDown => isIdle ? Right_Down_Idle : Right_Down_Input,
      LRInputType.RightLeft => isIdle ? Right_Left_Idle : Right_Left_Input,

      LRInputType.Pause => isIdle ? Pause_Idle : Pause_Input,
      LRInputType.StageRestart => isIdle ? Restart_Idle : Restart_Input,
      LRInputType.DialogueSkip => isIdle ? Skip_Idle : Skip_Input,

      LRInputType.Practice => isIdle ? Practice_Idle : Practice_Input,
      _ => throw new NotImplementedException(),
    };

  public static string GetInputTypeName(LRInputType inputType, bool isIdle, RightInputState rightInputState)
  => inputType switch
  {
    LRInputType.LeftUp => isIdle ? Left_Up_Idle : Left_Up_Input,
    LRInputType.LeftRight => isIdle ? Left_Right_Idle : Left_Right_Input,
    LRInputType.LeftDown => isIdle ? Left_Down_Idle : Left_Down_Input,
    LRInputType.LeftLeft => isIdle ? Left_Left_Idle : Left_Left_Input,

    LRInputType.RightUp => rightInputState switch
    {
      RightInputState.Arrow => isIdle ? Right_Up_Idle : Right_Up_Input,
      RightInputState.JIKL => isIdle ? Right_Up_Idle_Sub : Right_Up_Input_Sub,
      _ => throw new NotImplementedException(),
    },
    LRInputType.RightRight => rightInputState switch
    {
      RightInputState.Arrow => isIdle ? Right_Right_Idle : Right_Right_Input,
      RightInputState.JIKL => isIdle ? Right_Right_Idle_Sub : Right_Right_Input_Sub,
      _ => throw new NotImplementedException(),
    },
    LRInputType.RightDown => rightInputState switch
    {
      RightInputState.Arrow => isIdle ? Right_Down_Idle : Right_Down_Input,
      RightInputState.JIKL => isIdle ? Right_Down_Idle_Sub : Right_Down_Input_Sub,
      _ => throw new NotImplementedException(),
    },
    LRInputType.RightLeft => rightInputState switch
    {
      RightInputState.Arrow => isIdle ? Right_Left_Idle : Right_Left_Input,
      RightInputState.JIKL => isIdle ? Right_Left_Idle_Sub : Right_Left_Input_Sub,
      _ => throw new NotImplementedException(),
    },

    LRInputType.Pause => isIdle ? Pause_Idle : Pause_Input,
    LRInputType.StageRestart => isIdle ? Restart_Idle : Restart_Input,
    LRInputType.DialogueSkip => isIdle ? Skip_Idle : Skip_Input,

    LRInputType.Practice => isIdle ? Practice_Idle : Practice_Input,
    _ => throw new NotImplementedException(),
  };

}
