using System;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;
using Zenject;

namespace LR.Manager.Device
{
  public class DeviceManager : 
    IDeviceEvnetSubscriber, 
    IDeviceProvider, 
    IDisposable
  {
    private readonly UnityEvent<LRDeviceType> onDeviceChanged = new();

    public LRDeviceType CurrentDeviceType => currentDeviceType;
    private LRDeviceType currentDeviceType;

    [Inject]
    public DeviceManager()
    {
      if (Gamepad.current != null)
      {
        currentDeviceType = GetGamepadType(Gamepad.current);
      }
      else
      {
        currentDeviceType = LRDeviceType.Keyboard;
      }

      InputSystem.onEvent += OnInputEvent;
    }

    #region IDeviceEvnetSubscriber
    public void SubscribeDeviceEvent(UnityAction<LRDeviceType> onChanged)
    {
      onDeviceChanged.AddListener(onChanged);
      onChanged?.Invoke(currentDeviceType);
    }

    public void UnsubscribeDeviceEvent(UnityAction<LRDeviceType> onChanged)
    {
      onDeviceChanged.RemoveListener(onChanged);
    }
    #endregion

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
      if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
        return;

      LRDeviceType newType;

      if (device is Keyboard || device is Mouse)
        newType = LRDeviceType.Keyboard;
      else if (device is Gamepad pad)
        newType = GetGamepadType(pad);
      else
        return;

      if (newType == currentDeviceType)
        return;

      currentDeviceType = newType;
      onDeviceChanged.Invoke(newType);
    }

    private LRDeviceType GetGamepadType(Gamepad pad)
    {
      // ===== PlayStation =====
      // 가장 안정적인 방법
      if (pad is DualShockGamepad)
        return LRDeviceType.DualShock;

      var name = pad.displayName.ToLower();
      var layout = pad.layout.ToLower();

      // 보조 문자열 체크
      if (name.Contains("dualshock") ||
          name.Contains("dualsense") ||
          name.Contains("playstation") ||
          layout.Contains("dualshock"))
      {
        return LRDeviceType.DualShock;
      }

      // ===== Switch =====
      if (name.Contains("switch") ||
          name.Contains("pro controller") ||
          name.Contains("joy-con"))
      {
        return LRDeviceType.Switch;
      }

      // ===== Xbox =====
      if (pad is XInputController ||
          name.Contains("xbox") ||
          layout.Contains("xinput"))
      {
        return LRDeviceType.XBox;
      }

      // 기본값
      return LRDeviceType.XBox;
    }
    public void Dispose()
    {
      InputSystem.onEvent -= OnInputEvent;
    }
  }
}