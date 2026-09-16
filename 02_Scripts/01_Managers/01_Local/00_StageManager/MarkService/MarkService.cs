using Cysharp.Threading.Tasks;
using LR.Stage.Player.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace LR.Manager.Stage.Marking
{
  public class MarkService : 
    IMarkPlacer,
    IMarkController
  {
    [Inject] private readonly IResourceManager resourceManager = null;
    [Inject] private readonly AddressableKeySO addressableKeySO = null;
    [Inject] private readonly Transform root = null;

    private readonly Queue<WallHitPaint> createdLeftWallPaints = new();    
    private readonly Queue<WallHitPaint> createdRightWallPaints = new();

    private readonly Queue<WallHitPaint> prevLeftWallPaints = new();
    private readonly Queue<WallHitPaint> prevRightWallPaints = new();

    private readonly Queue<WallHitPaint> disabledLeftWallPaints = new();
    private readonly Queue<WallHitPaint> disabledRightWallPaints = new();
    private GameObject leftDeadPaint;
    private GameObject rightDeadPaint;

    private WallHitPaint leftWallPaintPrefab;
    private WallHitPaint rightWallPaintPrefab;   

    public async UniTask InitializeAsync()
    {
      leftWallPaintPrefab = await resourceManager.LoadAssetAsync<WallHitPaint>(addressableKeySO.Path.GameObjects + addressableKeySO.GameObjectName.LeftWallHitPaint);
      rightWallPaintPrefab = await resourceManager.LoadAssetAsync<WallHitPaint>(addressableKeySO.Path.GameObjects + addressableKeySO.GameObjectName.RightWallHitPaint);
      leftDeadPaint = await resourceManager.CreateAssetAsync<GameObject>(addressableKeySO.Path.GameObjects + addressableKeySO.GameObjectName.LeftDeadPaint, root);
      leftDeadPaint.transform.position = Vector3.one * 100.0f;
      leftDeadPaint.SetActive(false);
      rightDeadPaint = await resourceManager.CreateAssetAsync<GameObject>(addressableKeySO.Path.GameObjects + addressableKeySO.GameObjectName.RightDeadPaint, root);
      rightDeadPaint.transform.position = Vector3.one * 100.0f;
      rightDeadPaint.SetActive(false);
    }

    public void MarkDeadPaint(PlayerType playerType, Vector2 worldPosition)
    {
      var deadPaint = playerType switch
      {
        PlayerType.Left => leftDeadPaint,
        PlayerType.Right => rightDeadPaint,
        _ => throw new NotImplementedException(),
      };
      deadPaint.transform.position = worldPosition;
      deadPaint.SetActive(false);
    }

    public void MarkWallHitPaint(PlayerType playerType, Vector2 worldPosition)
    {
      var paint = GetWallHitPaint(playerType);
      paint.UpdateIdleAlpha();
      paint.transform.position = worldPosition;

      var targetCreatedQeueue = playerType switch
      {
        PlayerType.Left => createdLeftWallPaints,
        PlayerType.Right => createdRightWallPaints,
        _ => throw new NotImplementedException(),
      };
      targetCreatedQeueue.Enqueue(paint);
    }

    private WallHitPaint GetWallHitPaint(PlayerType playerType)
    {
      var targetQueue = playerType switch
      {
        PlayerType.Left => disabledLeftWallPaints,
        PlayerType.Right => disabledRightWallPaints,
        _ => throw new NotImplementedException(),
      };

      if (targetQueue.TryDequeue(out var existPaint))
      {
        existPaint.gameObject.SetActive(true);
        return existPaint;
      }
      else
      {
        var newPaint = GameObject.Instantiate(playerType switch
        {
          PlayerType.Left => leftWallPaintPrefab,
          PlayerType.Right => rightWallPaintPrefab,
          _ => throw new NotImplementedException(),
        }, root);        
        return newPaint;
      }
    }

    public void ClearWallHitPaints()
    {
      ClearPrevWallPaints();
      ChangeToPrevWallPaints();
    }

    private void ChangeToPrevWallPaints()
    {
      var leftCount = createdLeftWallPaints.Count;
      for (int i = 0; i < leftCount; i++)
      {
        var leftPaint = createdLeftWallPaints.Dequeue();
        leftPaint.UpdatePreviewAlpha();
        prevLeftWallPaints.Enqueue(leftPaint);
      }

      var rightCount = createdRightWallPaints.Count;
      for (int i = 0; i < rightCount; i++)
      {
        var rightPaint = createdRightWallPaints.Dequeue();
        rightPaint.UpdatePreviewAlpha();
        prevRightWallPaints.Enqueue(rightPaint);
      }
    }

    private void ClearPrevWallPaints()
    {
      var leftCount = prevLeftWallPaints.Count;
      for (int i = 0; i < leftCount; i++)
      {
        var leftPaint = prevLeftWallPaints.Dequeue();
        leftPaint.gameObject.SetActive(false);
        disabledLeftWallPaints.Enqueue(leftPaint);
      }

      var rightCount = prevRightWallPaints.Count;
      for (int i = 0; i < rightCount; i++)
      {
        var rightPaint = prevRightWallPaints.Dequeue();
        rightPaint.gameObject.SetActive(false);
        disabledRightWallPaints.Enqueue(rightPaint);
      }
    }

    public void ClearDeadPaints()
    {
      leftDeadPaint.SetActive(false);
      rightDeadPaint.SetActive(false);
    }

    public void ShowDeadPaints()
    {
      leftDeadPaint.SetActive(true);
      rightDeadPaint.SetActive(true);
    }
  }
}
