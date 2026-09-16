using LR.Manager.Device;
using LR.Stage.Player.Enum;
using UnityEngine;

[System.Serializable]
public class AtlasName
{
  [field: SerializeField] public string ChatPortrait { get; private set; }

  [field: Header("[ Dialogue Atlas ]")]
  [field: SerializeField] public string LeftDialoguePortrait { get; private set; }
  [field: SerializeField] public string CenterDialoguePortrait { get; private set; }
  [field: SerializeField] public string RightDialoguePortrait { get; private set; }

  public string GetDialoguePortrait(CharacterPositionType characterPositionType)
    => characterPositionType switch
    {
      CharacterPositionType.Left => LeftDialoguePortrait,
      CharacterPositionType.Center => CenterDialoguePortrait,
      CharacterPositionType.Right => RightDialoguePortrait,
      _ => throw new System.NotImplementedException(),
    };

  [field: Header("[ State Portrait Atlas ]")]
  [field: SerializeField] public string LeftStatePortrait { get; private set; }
  [field: SerializeField] public string RightStatePortrait { get; private set; }

  public string GetStatePortrait(PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => LeftStatePortrait,
      PlayerType.Right => RightStatePortrait,
      _ => throw new System.NotImplementedException(),
    };

  [field: Header("[ Input Atlas ]")]
  [field: SerializeField] public string KeyboardInput { get; private set; }
  [field: SerializeField] public string XboxInput { get; private set; }
  [field: SerializeField] public string SwitchInput { get; private set; }
  [field: SerializeField] public string DualShockInput { get; private set; }
  public string GetDeviceInput(LRDeviceType deviceType)
    => deviceType switch
    {
      LRDeviceType.Keyboard => KeyboardInput,
      LRDeviceType.XBox => XboxInput,
      LRDeviceType.Switch => SwitchInput,
      LRDeviceType.DualShock => DualShockInput,
      _ => throw new System.NotImplementedException(),
    };
}
