using Cysharp.Threading.Tasks;
using LR.Stage.Effect;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class EffectService : IEffectService
{
  private class EffectPool : IDisposable
  {
    private readonly InstanceEffectType effectType;
    private readonly string basePath;
    private readonly IResourceManager resourceManager;
    private readonly int poolingCount;

    private readonly List<BaseEffectObject> enables = new();
    private readonly Queue<BaseEffectObject> disables = new();

    public EffectPool(InstanceEffectType effectType, string basePath, IResourceManager resourceManager, int poolingCount)
    {
      this.effectType = effectType;
      this.basePath = basePath;
      this.resourceManager = resourceManager;
      this.poolingCount = poolingCount;
    }

    public void Dispose()
    {
      var path =
        basePath +
        effectType.ToString() +
        ".prefab";

      resourceManager.ReleaseAsset(path);
    }

    public async UniTask PlayOnceAsync(Vector3 position, Quaternion rotation, Transform root, UnityAction onComplete)
    {
      var baseEffectObject = disables.Count > 0 ? disables.Dequeue()
                                                : await CreateAsync(root);
      baseEffectObject.transform.SetPositionAndRotation(position, rotation);
      baseEffectObject.gameObject.SetActive(true);

      enables.Add(baseEffectObject);

      baseEffectObject.PlayAsync(() =>
      {
        onComplete?.Invoke();
        if(disables.Count < poolingCount)
        {
          enables.Remove(baseEffectObject);
          if(baseEffectObject != null)
          {
            baseEffectObject.gameObject.SetActive(false);
            disables.Enqueue(baseEffectObject);
          }          
        }
        else
        {
          baseEffectObject.StopImmediately();
          GameObject.Destroy(baseEffectObject.gameObject);
        }
      }).Forget();
    }

    private async UniTask<BaseEffectObject> CreateAsync(Transform root)
    {
      var path =
        basePath +
        effectType.ToString() +
        ".prefab";
      var baseEffectObject = await resourceManager.CreateAssetAsync<BaseEffectObject>(path, root);

      return baseEffectObject;
    }
  }
  private readonly Dictionary<InstanceEffectType, EffectPool> pools = new();

  [Inject] private readonly IResourceManager resourceManager = null;
  [Inject] private readonly AddressableKeySO addressableKeySO = null;
  [Inject] private readonly EffectTableSO effectTableSO = null;
  [Inject(Id = "effectDefaultroot")] private readonly Transform effectDefaultroot = null;

  public void Create(InstanceEffectType effectType, Vector3 position, Quaternion rotation, UnityAction onComplete = null, Transform root = null)
  {
    root ??= effectDefaultroot;

    if (pools.TryGetValue(effectType, out var pool))
    {
      pool.PlayOnceAsync(position, rotation, root, onComplete).Forget();
    }
    else
    {
      var newPool = new EffectPool(effectType, addressableKeySO.Path.Effect, resourceManager, effectTableSO.PoolingCount);
      pools[effectType] = newPool;
      newPool.PlayOnceAsync(position, rotation, root, onComplete).Forget();
    }
  }

  public void Create(InstanceEffectType effectType, Vector3 position, Vector3 euler, UnityAction onComplete = null, Transform root = null)
    => Create(effectType, position, Quaternion.Euler(euler), onComplete, root);

  public void Dispose()
  {
    foreach(var pool in pools.Values)
      pool.Dispose();
  }
}
