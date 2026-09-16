using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public interface IResourceManager
{
  public UniTask<List<AsyncOperationHandle>> LoadAssetsAsync(string key);

  public UniTask<List<AsyncOperationHandle>> LoadAssetsAsync(AssetReference assetReference);

  public UniTask<List<AsyncOperationHandle>> LoadAssetsAsync(IList<IResourceLocation> locations);

  public UniTask<AsyncOperationHandle> LoadAsync(IResourceLocation location);

  public UniTask<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object;

  public UniTask<T> CreateAssetAsync<T>(string key, Transform root = null) where T : UnityEngine.Object;

  public UniTask<T> CreateAssetAsync<T>(AssetReference assetReference, Transform root = null) where T : UnityEngine.Object;

  public UniTask<List<T>> CreateAssetsAsync<T>(string key, Transform root = null) where T : UnityEngine.Object;

  public UniTask<List<T>> CreateAssetsAsync<T>(AssetReference assetReference, Transform root = null) where T : UnityEngine.Object;

  public UniTask<List<T>> CreateAssetsAsync<T>(IList<IResourceLocation> locations, Transform root = null) where T : UnityEngine.Object;

  public UniTask<IList<IResourceLocation>> GetLocationsAsync(string key);

  public UniTask<IList<IResourceLocation>> GetLocationsAsync(AssetReference assetReference);

  public void ReleaseInstance(GameObject gameObject, bool releaseHandle = false);

  public void ReleaseAsset(string key);

  public void ReleaseAll();
}
