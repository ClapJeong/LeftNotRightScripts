using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using LR.UI.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerInputView : BaseUIView
  {
    [SerializeField] private PlayerType playerType;
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public UIInputView InputView { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hiding;
      var fadeDuration = isImmediately ? 0.0f : UISO.Player.InputFadeDuration;
      var moveDuration = isImmediately ? 0.0f : UISO.Player.InputMoveDuration;
      var delay = isImmediately ? 0.0f : UISO.Player.InputMoveDelay;
      var targetAlpha = 0.0f;
      var targetPosition = new Vector3((playerType == PlayerType.Left ? -1.0f : 1.0f) * UISO.Player.InputMoveLength, 0.0f);      
      try
      {
        var tasks = new List<UniTask>();
        foreach (var inputSet in InputView.InputSets)
        {
          var firstSet = playerType switch
          {
            PlayerType.Left => inputSet.LeftInputSet,
            PlayerType.Right => inputSet.RightInputSet,
            _ => throw new System.NotImplementedException(),
          };
          var lastSet = playerType switch
          {
            PlayerType.Left => inputSet.RightInputSet,
            PlayerType.Right => inputSet.LeftInputSet,
            _ => throw new System.NotImplementedException(),
          };
          tasks.Add(
            DOTween
            .Sequence()
            .Join(firstSet.Image.DOFade(targetAlpha, fadeDuration))
            .Join(firstSet.RectTrasnform.DOAnchorPos(targetPosition, moveDuration))
            .Insert(delay, inputSet.UpInputSet.Image.DOFade(targetAlpha, fadeDuration))
            .Insert(delay, inputSet.UpInputSet.RectTrasnform.DOAnchorPos(targetPosition, moveDuration))
            .Insert(delay, inputSet.DownInputSet.Image.DOFade(targetAlpha, fadeDuration))
            .Insert(delay, inputSet.DownInputSet.RectTrasnform.DOAnchorPos(targetPosition, moveDuration))
            .Insert(delay * 2.0f, lastSet.Image.DOFade(targetAlpha, fadeDuration))
            .Insert(delay * 2.0f, lastSet.RectTrasnform.DOAnchorPos(targetPosition, moveDuration))
            .ToUniTask(TweenCancelBehaviour.Complete, token));
        }
        await tasks;
        visibleState = VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showing;
      var fadeDuration = isImmediately ? 0.0f : UISO.Player.InputFadeDuration;
      var moveDuration = isImmediately ? 0.0f : UISO.Player.InputMoveDuration;
      var delay = isImmediately ? 0.0f : UISO.Player.InputMoveDelay;
      var targetAlpha = 1.0f;
      var targetPosition = Vector2.zero;
      try
      {
        var tasks = new List<UniTask>();
        foreach (var inputSet in InputView.InputSets)
        {
          var firstSet = playerType switch
          {
            PlayerType.Left => inputSet.RightInputSet,
            PlayerType.Right => inputSet.LeftInputSet,
            _ => throw new System.NotImplementedException(),
          };
          var lastSet = playerType switch
          {
            PlayerType.Left => inputSet.LeftInputSet,
            PlayerType.Right => inputSet.RightInputSet,
            _ => throw new System.NotImplementedException(),
          };

          tasks.Add(
            DOTween
            .Sequence()
            .Join(firstSet.Image.DOFade(targetAlpha, fadeDuration))
            .Join(firstSet.RectTrasnform.DOAnchorPos(targetPosition, moveDuration))
            .Insert(delay, inputSet.UpInputSet.Image.DOFade(targetAlpha, fadeDuration))
            .Insert(delay, inputSet.UpInputSet.RectTrasnform.DOAnchorPos(targetPosition, moveDuration))
            .Insert(delay, inputSet.DownInputSet.Image.DOFade(targetAlpha, fadeDuration))
            .Insert(delay, inputSet.DownInputSet.RectTrasnform.DOAnchorPos(targetPosition, moveDuration))
            .Insert(delay * 2.0f, lastSet.Image.DOFade(targetAlpha, fadeDuration))
            .Insert(delay * 2.0f, lastSet.RectTrasnform.DOAnchorPos(targetPosition, moveDuration))
            .ToUniTask(TweenCancelBehaviour.Complete, token));
        }
        await tasks;
        visibleState = VisibleState.Showen;
      }
      catch (OperationCanceledException) { }
      LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    }

    public void ResetInputPositions()
    {
      foreach (var inputSet in InputView.InputSets)
      {
        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
          inputSet.GetInputSet(direction).RectTrasnform.anchoredPosition = Vector2.zero;
          inputSet.GetInputSet(direction).Image.SetAlpha(1.0f);
        }
      }
    }

    public async UniTask ExhaustAsync(CancellationToken token)
    {
      var tasks = new List<UniTask>();
      var directions = DirectionUtil.GetShuffledDirections();
      var fadeDuration = UISO.Player.InputFailFadeDuration;
      var moveDuration = UISO.Player.InputFailMoveDuration;
      var targetAnchoredPosition = new Vector2(0.0f, -UISO.Player.InputFailMoveLength);

      foreach (var inputSet in InputView.InputSets)
      {
        var sequence = DOTween.Sequence();
        for(int i = 0; i < directions.Count; i++)
        {
          var direction = directions[i];
          var targetInputSet = inputSet.GetInputSet(direction);
          var delay = i * UISO.Player.InputFailDelay;

          var _ = sequence
            .Insert(delay, targetInputSet.Image.DOFade(0.0f, fadeDuration))
            .Insert(delay, targetInputSet.RectTrasnform.DOAnchorPos(targetAnchoredPosition, moveDuration));
        }
        tasks.Add(sequence.ToUniTask(TweenCancelBehaviour.Kill, token));
      }
      try
      {
        await UniTask.WhenAll(tasks);
      }
      catch (OperationCanceledException) { }
    }
  }
}