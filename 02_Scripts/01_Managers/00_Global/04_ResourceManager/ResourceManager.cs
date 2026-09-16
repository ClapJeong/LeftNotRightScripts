using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Zenject;

public class ResourceManager : IResourceManager, IDisposable
{
  readonly private Dictionary<IResourceLocation, AsyncOperationHandle> cachedHandles = new();
  readonly private Dictionary<GameObject, IResourceLocation> createdLocations = new();
  private readonly Dictionary<string, AsyncOperationHandle> assetHandles = new();

  public async UniTask<List<AsyncOperationHandle>> LoadAssetsAsync(string key)
  {
    var locations = await GetLocationsAsync(key);
    var handles = await LoadAssetsAsync(locations);
    return handles;
  }

  public async UniTask<List<AsyncOperationHandle>> LoadAssetsAsync(AssetReference assetReference)
  {
    var locations = await GetLocationsAsync(assetReference);
    var handles = await LoadAssetsAsync(locations);
    return handles;
  }

  public async UniTask<List<AsyncOperationHandle>> LoadAssetsAsync(IList<IResourceLocation> locations)
  {
    var handles = new List<AsyncOperationHandle>();
    foreach (var location in locations)
    {
      var loadHandle = await LoadAsync(location);
      handles.Add(loadHandle);
    }

    await Addressables.ResourceManager.CreateGenericGroupOperation(handles, true);
    return handles;
  }

  public async UniTask<AsyncOperationHandle> LoadAsync(IResourceLocation location)
  {
    var handle = Addressables.LoadAssetAsync<object>(location);
    await handle;
    cachedHandles[location] = handle;
    return handle;
  }

  public async UniTask<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
  {
    if (assetHandles.TryGetValue(key, out var cached))
    {
      if (typeof(T) == typeof(GameObject))
        return cached.Result as T;

      if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(typeof(T)))
        return (cached.Result as GameObject).GetComponent<T>();
    }

    AsyncOperationHandle handle;

    // 🔥 MonoBehaviour면 GameObject로 로드
    if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(typeof(T)))
    {
      handle = Addressables.LoadAssetAsync<GameObject>(key);
    }
    else
    {
      handle = Addressables.LoadAssetAsync<T>(key);
    }

    await handle;

    assetHandles[key] = handle;

    if (typeof(T) == typeof(GameObject))
      return handle.Result as T;

    if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(typeof(T)))
      return (handle.Result as GameObject).GetComponent<T>();

    return (T)handle.Result;
  }

  public async UniTask<T> CreateAssetAsync<T>(string key, Transform root = null) where T: UnityEngine.Object
  {
    var results = await CreateAssetsAsync<T>(key, root);
    return results.First();
  }

  public async UniTask<T> CreateAssetAsync<T>(AssetReference assetReference, Transform root = null) where T: UnityEngine.Object
  {
    var results = await CreateAssetsAsync<T>(assetReference, root);
    return results.First();
  }

  public async UniTask<List<T>> CreateAssetsAsync<T>(string key, Transform root = null) where T: UnityEngine.Object
  {
    var locations = await GetLocationsAsync(key);
    return await CreateAssetsAsync<T>(locations, root);
  }

  public async UniTask<List<T>> CreateAssetsAsync<T>(AssetReference assetReference, Transform root = null) where T : UnityEngine.Object
  {
    var locations = await GetLocationsAsync(assetReference);
    return await CreateAssetsAsync<T>(locations, root);
  }

  public async UniTask<List<T>> CreateAssetsAsync<T>(IList<IResourceLocation> locations, Transform root = null) where T : UnityEngine.Object
  {
    var objects = new List<T>();
    foreach (var location in locations)
    {
      if (!cachedHandles.ContainsKey(location))
        await LoadAsync(location);

      var createdGameObject = await Addressables.InstantiateAsync(location, instantiateParameters: new(parent: root, false));

      createdLocations[createdGameObject] = location;
      
      if(typeof(T) == typeof(GameObject))     
        objects.Add(createdGameObject as T);
      else
        objects.Add(createdGameObject.GetComponent<T>());
    }
    
    return objects;
  }

  public async UniTask<IList<IResourceLocation>> GetLocationsAsync(string key)
  {
    var locationHandle = Addressables.LoadResourceLocationsAsync(key);
    await locationHandle;
    return locationHandle.Result;
  }

  public async UniTask<IList<IResourceLocation>> GetLocationsAsync(AssetReference assetReference)
  {
    var locationHandle = Addressables.LoadResourceLocationsAsync(assetReference);
    await locationHandle;
    return locationHandle.Result;
  }

  public void ReleaseInstance(GameObject gameObject, bool releaseHandle = false)
  {
    if (releaseHandle &&
      createdLocations.TryGetValue(gameObject, out var location) &&
      cachedHandles.TryGetValue(location,out var handle))
    {
      cachedHandles.Remove(location);
      Addressables.Release(handle);
    }    
    createdLocations.Remove(gameObject);
    Addressables.ReleaseInstance(gameObject);
  }

  public void ReleaseAll()
  {
    foreach (var handle in cachedHandles.Values)
      handle.Release();
    cachedHandles.Clear();
  }

  public void ReleaseAsset(string key)
  {
    if (!assetHandles.TryGetValue(key, out var handle))
      return;

    Addressables.Release(handle);
    assetHandles.Remove(key);
  }

  public void Dispose()
  {
    ReleaseAll();
  }
}
