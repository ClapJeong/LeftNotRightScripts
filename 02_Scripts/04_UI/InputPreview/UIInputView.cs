using LR.Manager.Device;
using LR.Manager.Input;
using LR.Stage.Player.Enum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.Input
{
  public class UIInputView : MonoBehaviour
  {
    [System.Serializable]
    public class DeviceInputSets
    {
      [field: SerializeField] public LRDeviceType DeviceType { get; private set; }
      [field: SerializeField] public RectTransform RectTransform { get; private set; }
      [field: SerializeField] public GameObject GameObject { get; private set; }
      [field: SerializeField] public InputSet UpInputSet { get; private set; }
      [field: SerializeField] public InputSet RightInputSet { get; private set; }
      [field: SerializeField] public InputSet DownInputSet { get; private set; }
      [field: SerializeField] public InputSet LeftInputSet { get; private set; }

      public InputSet GetInputSet(Direction direction)
        => direction switch
        {
          Direction.Up => UpInputSet,
          Direction.Right => RightInputSet,
          Direction.Down => DownInputSet,
          Direction.Left => LeftInputSet,
          _ => throw new System.NotImplementedException(),
        };

      public RectTransform GetImageRectTransform(Direction direction)
        => direction switch
        {
          Direction.Up => UpInputSet.RectTrasnform,
          Direction.Right => RightInputSet.RectTrasnform,
          Direction.Down => DownInputSet.RectTrasnform,
          Direction.Left => LeftInputSet.RectTrasnform,
          _ => throw new System.NotImplementedException(),
        };

      public Image GetImage(Direction direction)
        => direction switch
        {
          Direction.Up => UpInputSet.Image,
          Direction.Right => RightInputSet.Image,
          Direction.Down => DownInputSet.Image,
          Direction.Left => LeftInputSet.Image,
          _ => throw new System.NotImplementedException(),
        };
    }

    [System.Serializable]
    public class InputSet
    {
      [field: SerializeField] public Image Image { get; private set; }
      [field: SerializeField] public RectTransform RectTrasnform { get; private set; }
    }

    [field: SerializeField] public List<DeviceInputSets> InputSets { get; private set; }
    [Space(10)]
    [SerializeField] private RectTransform rectTransform;

    private readonly Dictionary<LRDeviceType, SpriteAtlas> atlases = new();

    public RectTransform GetEnableRectTransform()
    {
      foreach(var inputSet in InputSets)
        if(inputSet.GameObject.activeSelf)
          return inputSet.RectTransform;

      return null;
    }

    private IDeviceProvider deviceProvider;
    private IRightInputStateService rightInputStateService;

    [Inject]
    public void InitializeRightInputState(
      IDeviceProvider deviceProvider,
      IRightInputStateService rightInputStateService)
    {
      this.deviceProvider = deviceProvider;
      this.rightInputStateService = rightInputStateService;

      rightInputStateService.SubscribeRightInputStateChanged(OnRightInputStateChanged);
    }

    private void OnRightInputStateChanged(RightInputState rightInputState)
    {
      var currentDevice = deviceProvider.CurrentDeviceType;
      if (currentDevice == LRDeviceType.Keyboard)
      {
        foreach(var inputSet in InputSets)
        {          
          if (inputSet.DeviceType != currentDevice)
            continue;

          if (!atlases.ContainsKey(currentDevice))
            continue;

          foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
          {
            var atlas = atlases[currentDevice];
            var spriteName = InputIconAssetName.GetInputTypeName(direction.ParseToLRInputType(PlayerType.Right), true, rightInputState);

            var targetSprite = atlas.GetSprite(spriteName);
            inputSet
              .GetImage(direction)
              .sprite = targetSprite;
          }          
        }
      }
    }

    public void AddAtlas(PlayerType playerType, LRDeviceType deviceType, SpriteAtlas spriteAtlas)
    {
      atlases[deviceType] = spriteAtlas;
      foreach(var inputSet in InputSets)
      {
        if(inputSet.DeviceType == deviceType)
        {
          foreach(Direction direction in System.Enum.GetValues(typeof(Direction)))
          {
            var inputDirection = direction.ParseToLRInputType(playerType);
            var spriteName = InputIconAssetName.GetInputTypeName(inputDirection, true);
            var sprite = spriteAtlas.GetSprite(spriteName);
            inputSet
              .GetInputSet(direction)
              .Image
              .sprite = sprite;
          }

          break;
        }
      }
    }

    public void ToggleDeviceInputSet(LRDeviceType deviceType)
    {
      foreach (var inputSet in InputSets)
      {
        var isEnable = inputSet.DeviceType == deviceType;
        inputSet.GameObject.SetActive(isEnable);
      }
      LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void ApplyColor(ColorSO colorSO, PlayerType playerType)
    {
      foreach(var inputSet in InputSets)
      {
        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
          inputSet.GetImage(direction).color = playerType switch
          {
            PlayerType.Left => colorSO.LeftColor,
            PlayerType.Right => colorSO.RightColor,
            _ => throw new System.NotImplementedException(),
          };
      }
    }

    public DeviceInputSets GetDeviceInputSet(LRDeviceType deviceType)
      => InputSets.FirstOrDefault(set => set.DeviceType == deviceType);

    public InputSet GetInputSet(LRDeviceType deviceType, Direction direction)
      => GetDeviceInputSet(deviceType).GetInputSet(direction);

    public RectTransform GetImageRectTransform(LRDeviceType deviceType, Direction direction)
      => GetDeviceInputSet(deviceType).GetImageRectTransform(direction);

    public Image GetImage(LRDeviceType deviceType, Direction direction)
      => GetDeviceInputSet(deviceType).GetImage(direction);

    public void UpdateIcon(PlayerType playerType, Direction direction, bool isInput)
    {
      foreach (var inputSet in InputSets)
      {
        var deviceType = inputSet.DeviceType;
        if (!atlases.ContainsKey(deviceType))
          continue;

        var isRightKeyboardInput = deviceType == LRDeviceType.Keyboard && playerType == PlayerType.Right;

        var targetSprite = atlases[deviceType]
          .GetSprite(
          isRightKeyboardInput ? InputIconAssetName.GetInputTypeName(direction.ParseToLRInputType(playerType), !isInput, rightInputStateService.CurrentRightInputState)
                               : InputIconAssetName.GetInputTypeName(direction.ParseToLRInputType(playerType), !isInput));
        inputSet
          .GetImage(direction)
          .sprite = targetSprite;
      }
    }

    private void OnDestroy()
    {
      rightInputStateService?.SubscribeRightInputStateChanged(OnRightInputStateChanged);
    }
  }
}
