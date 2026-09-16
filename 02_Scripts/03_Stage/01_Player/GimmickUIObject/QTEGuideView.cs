using Cysharp.Threading.Tasks;
using LR.Manager.Device;
using LR.Manager.Input;
using LR.Stage.Player.Enum;
using LR.Stage.Player.GimmickGuide.QTE;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;

namespace LR.Stage.Player.GimmickGuide
{
  public class QTEGuideView : BaseGimmickGuideView
  {
    [SerializeField] private QTEGuideObject iconPrefab;
    [Space(5)]
    [SerializeField] private Vector2 upLocalPosition;
    [SerializeField] private Vector2 rightLocalPosition;
    [SerializeField] private Vector2 downLocalPosition;
    [SerializeField] private Vector2 leftLocalPosition;
    [SerializeField] private float space;
    [Space(5)]
    [SerializeField] private float scaleMin;

    private IDeviceEvnetSubscriber deviceEvnetSubscriber;    
    private IRightInputStateService rightInputStateService;
    private IDeviceProvider deviceProvider;

    private LRDeviceType currentDeviceType;
    private PlayerType playerType;
    private Dictionary<LRDeviceType, SpriteAtlas> atlases;
    private ColorSO colorSO;

    private readonly Dictionary<LRInputType, List<QTEGuideObject>> usedIcons = new();
    private readonly Queue<QTEGuideObject> unusedIcons = new();


    public override async UniTask InitializeAsync(PlayerType playerType, DiContainer diContainer)
    {
      await base.InitializeAsync(playerType, diContainer);

      this.playerType = playerType;
      this.colorSO = diContainer.Resolve<ColorSO>();
      atlases = await InputAtlasProvider.GetInputAtlasesAsync(
        diContainer.Resolve<AddressableKeySO>(),
        diContainer.Resolve<IResourceManager>());

      deviceProvider = diContainer.Resolve<IDeviceProvider>();
      currentDeviceType = deviceProvider.CurrentDeviceType;

      deviceEvnetSubscriber = diContainer.Resolve<IDeviceEvnetSubscriber>();
      deviceEvnetSubscriber.SubscribeDeviceEvent(OnDeviceChanged);

      rightInputStateService = diContainer.Resolve<IRightInputStateService>();
      rightInputStateService.SubscribeRightInputStateChanged(OnRightInputStateChanged);
    }

    private void OnDeviceChanged(LRDeviceType deviceType)
    {
      currentDeviceType = deviceType;
      if(atlases.TryGetValue(deviceType, out var atlas))
      {
        foreach(var pair in usedIcons)
        {
          var spriteName = InputIconAssetName.GetInputTypeName(pair.Key, false);
          var sprite = atlas.GetSprite(spriteName);
          foreach(var icon in pair.Value)
            icon.UpdateSprite(sprite);
        }
      }
    }

    private void OnRightInputStateChanged(RightInputState rightInputState)
    {
      var deviceType = deviceProvider.CurrentDeviceType;
      if(deviceType == LRDeviceType.Keyboard && atlases.TryGetValue(deviceType, out var atlas))
      {
        foreach (var pair in usedIcons)
        {
          var additional = rightInputState == RightInputState.JIKL ? InputIconAssetName.IJKLAdditionalName : string.Empty;
          var spriteName = InputIconAssetName.GetInputTypeName(pair.Key, false) + additional;
          var sprite = atlas.GetSprite(spriteName);
          foreach (var icon in pair.Value)
            icon.UpdateSprite(sprite);
        }
      }
    }

    private Vector2 GetLocalPosition(Direction direction)
      => direction switch
      {
        Direction.Up => upLocalPosition,
        Direction.Right => rightLocalPosition,
        Direction.Down => downLocalPosition,
        Direction.Left => leftLocalPosition,
        _ => throw new System.NotImplementedException(),
      };

    public void UpdateIconCount(LRInputType inputType, int count)
    {
      if (!usedIcons.ContainsKey(inputType))
        usedIcons[inputType] = new();

      var pool = usedIcons[inputType];
      var direction = inputType.ParseToDirection();
      var beginPosition = GetLocalPosition(direction);
      var isKeyboard = deviceProvider.CurrentDeviceType == LRDeviceType.Keyboard;
      var isRight = inputType.IsRight();
      var spriteName = isKeyboard && isRight ? InputIconAssetName.GetInputTypeName(inputType, false, rightInputStateService.CurrentRightInputState)
                                             : InputIconAssetName.GetInputTypeName(inputType, false);
      var sprite = atlases.ContainsKey(currentDeviceType) ? atlases[currentDeviceType].GetSprite(spriteName)
                                                          : null;
      for (int i = 0; i < count; i++) 
      {
        var icon = GetNewIcon();
        icon.transform.localPosition = beginPosition + i * space * direction.ParseVector2();
        icon.UpdateSprite(sprite);
        icon.UpdateColor(colorSO.GetPlayerColor(playerType));

        pool.Add(icon);
      }
    }

    public void OnSuccessSingleQTE(LRInputType inputType, int count)
    {
      var pool = usedIcons[inputType];
      var targetIcon = pool[pool.Count - 1];
      targetIcon.UpdateColor(colorSO.GetPlayerColor(playerType));
      targetIcon.Deactivate(true);
      pool.Remove(targetIcon);
      unusedIcons.Enqueue(targetIcon);

    }

    public void ClearIcons()
    {
      foreach (LRInputType inputType in System.Enum.GetValues(typeof(LRInputType)))
      {
        if (usedIcons.TryGetValue(inputType, out var pool))
        {
          var count = pool.Count;
          for (int i = 0; i < count; i++)
          {
            var unusedIcon = pool[0];
            pool.Remove(unusedIcon);
            unusedIcon.Deactivate(false);
            unusedIcons.Enqueue(unusedIcon);
          }
        }
      }
    }

    public void UpdateDuration(float duration)
    {
      var targetScale = Mathf.Lerp(scaleMin, 1.0f, duration);
      foreach (var icons in usedIcons.Values)
        foreach (var icon in icons)
          icon.UpdateScale(targetScale);
    }

    private QTEGuideObject GetNewIcon()
    {
      if (unusedIcons.TryDequeue(out var existIcon))
      {
        existIcon.transform.SetParent(transform);
        existIcon.Activate();
        return existIcon;
      }
      else
      {
        var newIcon = Instantiate(iconPrefab, transform);
        return newIcon;
      }
    }


    private void OnDestroy()
    {
      deviceEvnetSubscriber.UnsubscribeDeviceEvent(OnDeviceChanged);
      rightInputStateService.UnsubscribeRightInputStateChanged(OnRightInputStateChanged);
    }
  }
}
