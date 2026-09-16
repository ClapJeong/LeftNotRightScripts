using Cysharp.Threading.Tasks;
using LR.Manager.Device;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.U2D;

public static class InputAtlasProvider
{
  public static async UniTask<Dictionary<LRDeviceType, SpriteAtlas>> GetInputAtlasesAsync(
    AddressableKeySO addressableKeySO, 
    IResourceManager resourceManager,
    UnityAction<LRDeviceType, SpriteAtlas> onForeach = null)
  {
    var atlases = new Dictionary<LRDeviceType, SpriteAtlas>();
    
    foreach (LRDeviceType deviceType in System.Enum.GetValues(typeof(LRDeviceType)))
    {
      var name = addressableKeySO.AtlasName.GetDeviceInput(deviceType);
      if (string.IsNullOrEmpty(name))
        continue;
      var path = addressableKeySO.Path.SpriteAtlas;
      var key = path + name;
      var atlas = await resourceManager.LoadAssetAsync<SpriteAtlas>(key);
      atlases[deviceType] = atlas;
      onForeach?.Invoke(deviceType, atlas);
    }

    return atlases;
  }
}
