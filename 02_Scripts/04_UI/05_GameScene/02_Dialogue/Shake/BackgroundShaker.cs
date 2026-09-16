using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Table.Dialogue;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.GameScene.Dialogue.Shake
{
  public class BackgroundShaker : IDisposable
  {        
    [Inject] private readonly IGameDataProvider gameDataProvider = null;
    [Inject] private readonly int stageIndex = 0;
    [Inject] private readonly Transform root = null;
    [Inject] private readonly BackgroundShakeFallObject fallObjectPrefab = null;
    [Inject] private readonly BackgroundShakeData data = null;

    private readonly List<BackgroundShakeFallObject> activatedObjects = new();
    private readonly Queue<BackgroundShakeFallObject> deactivatedObjects = new();

    public async UniTask PlayAsync(CancellationToken token)
    {
      try
      {
        var emission = Mathf.Lerp(data.EmissionMin, data.EmissionMax, stageIndex / gameDataProvider.StageDataCount);
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var fallObject = Get();
          var posX = UnityEngine.Random.Range(data.CreateWidthSpace, Screen.width - data.CreateWidthSpace);
          var posY = UnityEngine.Random.Range(Screen.height - data.CreateHeightSpace, Screen.height);
          var screenPosition = new Vector2(posX, posY);

          var size = Vector3.one *
            UnityEngine.Random.Range(data.SizeMin, data.SizeMax);

          var rotate = Mathf.Sign(UnityEngine.Random.Range(-1, 1)) *
            UnityEngine.Random.Range(data.RotateMin, data.RotateMax);

          var beginAlpha = UnityEngine.Random.Range(data.BeginAlphaMin, 1.0f);

          var minSpeed = data.SpeedMin + UnityEngine.Random.Range(-data.SpeedModifyRange, data.SpeedModifyRange);
          var maxSpeed = data.SpeedMax + UnityEngine.Random.Range(-data.SpeedModifyRange, data.SpeedModifyRange);

          var duration = UnityEngine.Random.Range(data.DurationMin, data.DurationMax);

          fallObject.PlayAsync(
            screenPosition,
            size,
            rotate,
            beginAlpha,
            minSpeed,
            maxSpeed,
            duration,
            onComplete: () =>
            {
              activatedObjects.Remove(fallObject);
              deactivatedObjects.Enqueue(fallObject);
            }).Forget();
          activatedObjects.Add(fallObject);

          await UniTask.WaitForSeconds(1.0f / emission);
        }        
      }
      catch (OperationCanceledException) { }
    }

    private BackgroundShakeFallObject Get()
    {
      if(deactivatedObjects.Count == 0)
        return GameObject.Instantiate(fallObjectPrefab, root);
      else
        return deactivatedObjects.Dequeue();
    }

    public void Dispose()
    {
    }
  }
}
