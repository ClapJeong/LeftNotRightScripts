using Cysharp.Threading.Tasks;
using LR.Stage.StageDataContainer;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.Manager.Local.StagePreview
{
  public class StagePreviewService : IStagePreviewCreator, IDisposable
  {
    private class StageDataSet
    {
      public readonly StageDataContainer stage;
      public readonly string key;

      public StageDataSet(StageDataContainer stage, string key)
      {
        this.stage = stage;
        this.key = key;
      }
    }

    [Inject] private readonly Transform root = null;
    [Inject] private readonly AddressableKeySO addressableKeySO = null;
    [Inject] private readonly IResourceManager resourceManager = null;

    private readonly Dictionary<int, StageDataSet> stageDataSets = new();
    private int selectedStageIndex;

    public async UniTask CreatePreviewAsync(int index, UnityAction<StageDataContainer> onComplete, CancellationToken token)
    {
      if (stageDataSets.TryGetValue(selectedStageIndex, out var stageDataSet))
        stageDataSet.stage.gameObject.SetActive(false);

      StageDataContainer stageObject = null;

      if (stageDataSets.TryGetValue(index, out var existStageDataSet))
      {
        stageObject = existStageDataSet.stage;
        existStageDataSet.stage.gameObject.SetActive(true);
      }        
      else
      {
        var key = addressableKeySO.Path.Stage + string.Format(addressableKeySO.StageName.StageNameFormat, index);
        stageObject = await resourceManager.CreateAssetAsync<StageDataContainer>(key, root);
        stageDataSets.Add(index, new(stageObject, key));
      }

      if (!token.IsCancellationRequested)
      {
        selectedStageIndex = index;
        onComplete?.Invoke(stageObject);
      }
      else
      {
        if (stageObject != null)
          stageObject.gameObject.SetActive(false);
      }
    }

    public void ClosePreview()
    {
      if (stageDataSets.TryGetValue(selectedStageIndex, out var stageDataSet))
        stageDataSet.stage.gameObject.SetActive(false);
    }

    public void Dispose()
    {
      foreach (var stageData in stageDataSets.Values)
        resourceManager.ReleaseAsset(stageData.key);
    }
  }
}
