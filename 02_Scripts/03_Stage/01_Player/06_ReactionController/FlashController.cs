using Cysharp.Threading.Tasks;
using LR.Table.Player;
using System;
using UnityEngine;

namespace LR.Stage.Player.ReactionController
{
  public class FlashController : IDisposable
  {
    private readonly SpriteRenderer spriteRenderer;
    private readonly PlayerBlinkData blinkData;

    private readonly CTSContainer cts = new();
    private readonly MaterialPropertyBlock materialPropertyBlock = new();

    public FlashController(
      SpriteRenderer spriteRenderer,
      PlayerBlinkData blinkData)
    {
      this.spriteRenderer = spriteRenderer;
      this.blinkData = blinkData;
      spriteRenderer.GetPropertyBlock(materialPropertyBlock);
    }

    public async UniTask WallHitBlinkAsync()
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;

      var id = ShaderHash.Player._Flash;
      spriteRenderer.GetPropertyBlock(materialPropertyBlock);

      try
      {
        materialPropertyBlock.SetFloat(id, 1.0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
        await UniTask.WaitForSeconds(blinkData.WallHitBlinkDuration);
      }
      catch (OperationCanceledException) { }
      finally
      {
        materialPropertyBlock.SetFloat(id, 0.0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
      }
    }

    public async UniTask TeleportBlinkAsync()
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;

      var id = ShaderHash.Player._Flash;
      spriteRenderer.GetPropertyBlock(materialPropertyBlock);

      try
      {
        var duration = blinkData.TeleportBlinkDuration;
        materialPropertyBlock.SetFloat(id, 1.0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);

        while (duration > 0.0f)
        {
          token.ThrowIfCancellationRequested();
          materialPropertyBlock.SetFloat(id, duration / blinkData.TeleportBlinkDuration);
          spriteRenderer.SetPropertyBlock(materialPropertyBlock);
          duration -= Time.deltaTime;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        materialPropertyBlock.SetFloat(id, 0.0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
      }
    }


    public void Dispose()
    {
      cts.Dispose();
    }
  }
}
