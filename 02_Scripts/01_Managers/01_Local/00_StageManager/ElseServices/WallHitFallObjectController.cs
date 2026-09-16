using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Local.CameraService;
using LR.Stage.Player.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace LR.Manager.Stage
{
  public class WallHitFallObjectController : IDisposable
  {
    [Inject] private readonly WallHitObjectSO wallHitObjectSO = null;
    [Inject] private readonly AddressableKeySO addressableKeySO = null;
    [Inject] private readonly IResourceManager resourceManager = null;
    [Inject] private readonly IPlayerGetter playerGetter = null;
    [Inject] private readonly ICameraValueService cameraValueService = null;
    [Inject] private readonly IGameDataProvider gameDataProvider = null;
    [Inject] private readonly IPracticeService practiceService = null;

    [Inject] private readonly Transform root = null;

    private readonly List<WallHitFallObject> workingObjects = new();
    private readonly Queue<WallHitFallObject> disableObjects = new();
    private int currentChapter;

    public void Initialize()
    {
      this.currentChapter = gameDataProvider.GetSelectedChapter();

      var leftPlayerEnergySubscriber = playerGetter.GetPlayer(PlayerType.Left).GetEnergySubscriber();
      leftPlayerEnergySubscriber.SubscribeOnHit(OnPlayerHit);
    }

    private void OnPlayerHit(PlayerType playerType, DamageType damageType)
    {
      if (damageType != DamageType.WallBump || practiceService.IsPractice)
        return;

      var targetPlayer = playerGetter.GetPlayer(playerType);
      var playerPosition = targetPlayer.GetMoveController().GetCurrentPosition();
      var angle = UnityEngine.Random.Range(0.0f, 360.0f) * UnityEngine.Mathf.Deg2Rad;
      var randomRange = new Vector2(UnityEngine.Mathf.Cos(angle), UnityEngine.Mathf.Sign(angle)) * wallHitObjectSO.CreateRange;
      var additionalHeight = Vector2.up * wallHitObjectSO.CreateHeight;
      var beginPosition = playerPosition + randomRange + additionalHeight;

      var cameraSize = cameraValueService.GetInitializedOrthographizSize();
      var cameraVerticalClamp = cameraSize * 1.7f;
      beginPosition.x = UnityEngine.Mathf.Clamp(beginPosition.x , - cameraVerticalClamp, cameraVerticalClamp);
      beginPosition.y = UnityEngine.Mathf.Min(beginPosition.y, cameraSize + 1.5f);

      var stageT = currentChapter / 12.0f;

      var sizeMin = Mathf.Lerp(wallHitObjectSO.SizeMin.Min, wallHitObjectSO.SizeMax.Min, stageT);
      var sizeMax = Mathf.Lerp(wallHitObjectSO.SizeMin.Max, wallHitObjectSO.SizeMax.Max, stageT);

      var emmision = Mathf.Lerp(wallHitObjectSO.EmmisionMin, wallHitObjectSO.EmmisionMax, stageT);
      var emmsionMin = Mathf.Max(1, emmision - wallHitObjectSO.EmmisionRange);
      var emmsionMax = emmision + wallHitObjectSO.EmmisionRange;
      CreateFallObjectAsync(beginPosition, emmsionMin, emmsionMax, sizeMin, sizeMax).Forget();
    }

    private async UniTask CreateFallObjectAsync(
      Vector2 beginPosition, 
      float emmisionMin, 
      float emmisionMax,
      float sizeMin,
      float sizeMax)
    {
      var enableObject = await GetEnableFallObjectAsync();
      enableObject.gameObject.SetActive(true);
      enableObject.PlayAsync(beginPosition, emmisionMin, emmisionMax, sizeMin, sizeMax,
        onComplete: () =>
        {
          enableObject.gameObject.SetActive(false);
          workingObjects.Remove(enableObject);
          disableObjects.Enqueue(enableObject);
        }).Forget();
    }

    private async UniTask<WallHitFallObject> GetEnableFallObjectAsync()
    {
      if (disableObjects.TryDequeue(out var enableObject))
      {
        workingObjects.Add(enableObject);
        return enableObject;
      }
      else
      {
        var key = addressableKeySO.Path.GameObjects + addressableKeySO.GameObjectName.WallHitFallingObject;
        var newObject = await resourceManager.CreateAssetAsync<WallHitFallObject>(key, root);
        workingObjects.Add(newObject);
        return newObject;
      }
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }
  }
}