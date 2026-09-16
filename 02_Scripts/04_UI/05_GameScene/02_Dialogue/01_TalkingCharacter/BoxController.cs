using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Table.Dialogue;
using System;
using System.Threading;
using UnityEngine;

namespace LR.UI.GameScene.Dialogue.Character
{
  public class BoxController : IDisposable
  {
    private readonly UISO uiSO;
    private readonly CharacterPositionType positionType;
    private readonly RectTransform boxRectTransform;

    private readonly CTSContainer jumpCTS = new();
    private readonly CTSContainer rotateCTS = new();

    public BoxController(UISO uiSO, CharacterPositionType positionType, RectTransform boxRectTransform)
    {
      this.positionType = positionType;
      this.uiSO = uiSO;
      this.boxRectTransform = boxRectTransform;
    }    

    public async UniTask JumpAsync()
    {
      jumpCTS.Cancel();
      jumpCTS.Create();
      var token = jumpCTS.token;
      try
      {
        await boxRectTransform
          .DOJumpAnchorPos(Vector2.zero, uiSO.Dialogue.BoxJumpLength, 1, uiSO.Dialogue.BoxJumpDuration)
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException) { }
    }

    public async UniTask RotateAsync()
    {
      rotateCTS.Cancel();
      rotateCTS.Create();
      var token = rotateCTS.token;
      var roateValue = positionType switch
      {
        CharacterPositionType.Left => uiSO.Dialogue.LeftBoxRotateValue,
        CharacterPositionType.Center => throw new NotImplementedException(),
        CharacterPositionType.Right => uiSO.Dialogue.RightBoxRotateValue,
        _ => throw new NotImplementedException(),
      };
      var duration = uiSO.Dialogue.BoxRotateDuration;
      try
      {
        await DOTween
          .Sequence()
          .AppendInterval(uiSO.Dialogue.BoxRotateDelay)
          .Append(boxRectTransform.DORotate(new Vector3(0.0f, 0.0f, roateValue), duration))
          .Append(boxRectTransform.DORotate(Vector3.zero, duration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);

      }
      catch (OperationCanceledException) { }
    }

    public void CompleteDialogueImmediately()
    {
      jumpCTS.Cancel();
      rotateCTS.Cancel();
    }

    public void Dispose()
    {
      jumpCTS.Dispose();
    }
  }
}
