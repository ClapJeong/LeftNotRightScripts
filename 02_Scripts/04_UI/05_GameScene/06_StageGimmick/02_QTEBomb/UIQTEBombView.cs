using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.GameScene.StageGimmick.QTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace LR.UI.GameScene.StageGimmick
{
  public class UIQTEBombView : BaseUIView
  {
    [SerializeField] private CanvasGroup canvasGroup;
    [field: SerializeField] public RectTransform RootRectTransform { get; private set; }

    [field: Header("[ Random ]")]
    [field: SerializeField] public GameObject RandomIconGameObject { get; private set; }
    [field: SerializeField] public Image RandomIconImage { get; private set; }

    [field: Header("[ Duration ]")]
    [field: SerializeField] public Image OutlineImage { get; private set; }
    [field: SerializeField] public Image DurationImage { get; private set;  }

    [field: Header("[ Icon ]")]
    [field: SerializeField] public CanvasGroup IconContentCanvasGroup { get; private set; }
    [field: SerializeField] public UIQTEIcon IconPrefab { get; private set; }
    [field: SerializeField] public RectTransform UpRoot { get; private set; }
    [field: SerializeField] public RectTransform RightRoot { get; private set; }
    [field: SerializeField] public RectTransform DownRoot { get; private set; }
    [field: SerializeField] public RectTransform LeftRoot { get; private set; }

    private readonly Dictionary<Direction, List<UIQTEIcon>> usedIcons = new();
    private readonly Queue<UIQTEIcon> unusedIcons = new();

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      var fadeDuration = isImmediately ? 0.0f : UISO.StageGimmick.FadeDuration;
      var moveDuration = isImmediately ? 0.0f : UISO.StageGimmick.MoveDuration;
      var targetAlpha = 0.0f;
      var targetPositon = new Vector2(0.0f, -UISO.StageGimmick.HideLength);
      try
      {
        await DOTween
          .Sequence()
          .Join(canvasGroup.DOFade(targetAlpha, fadeDuration))
          .Join(RectTransform.DOAnchorPos(targetPositon, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = Enum.VisibleState.Hidden;
      }
      catch (OperationCanceledException) { }
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      var fadeDuration = isImmediately ? 0.0f : UISO.StageGimmick.FadeDuration;
      var moveDuration = isImmediately ? 0.0f : UISO.StageGimmick.MoveDuration;
      var targetAlpha = 1.0f;
      var targetPositon = Vector2.zero;
      try
      {
        await DOTween
          .Sequence()
          .Join(canvasGroup.DOFade(targetAlpha, fadeDuration))
          .Join(RectTransform.DOAnchorPos(targetPositon, moveDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = Enum.VisibleState.Showen;
      }
      catch (OperationCanceledException) { }

    }

    public void ClearIcons()
    {
      foreach(Direction direction in System.Enum.GetValues(typeof(Direction)))
      {
        if(usedIcons.TryGetValue(direction, out var pool))
        {
          var count = pool.Count;
          for (int i = 0; i < count; i++)
          {
            var unusedIcon = pool[0];
            pool.Remove(unusedIcon);
            unusedIcon.gameObject.SetActive(false);
            unusedIcons.Enqueue(unusedIcon);
          }          
        }
      }
    }

    public void UpdateIconCount(Direction direction, int count)
    {
      if (!usedIcons.ContainsKey(direction))
        usedIcons[direction] = new();

      var pool = usedIcons[direction];
      var root = GetIconRoot(direction);
      for (int i = 0; i < count; i++)
        pool.Add(GetNewIcon(root));

      var index = 0;
      foreach (var icon in pool)
      {
        icon.UpdateAlpha(1.0f);
        index++;
      }
    }

    public void UpdateIconSprite(Direction direction, Sprite sprite)
    {
      if (usedIcons.TryGetValue(direction, out var pool))
        foreach (var icon in pool)
        {
          icon.UpdateSprite(sprite);
        }          
    }

    public void UpdateIconColor(Direction direction, Color color)
    {
      if (usedIcons.TryGetValue(direction, out var pool))
        foreach (var icon in pool)
        {
          icon.UpdateColor(color);
        }
    }

    public void UpdateAlpha(Direction direction, int index, float alpha)
    {
      usedIcons[direction][index].UpdateAlpha(alpha);
    }

    public RectTransform GetIconRoot(Direction direction)
      => direction switch
      {
        Direction.Up => UpRoot,
        Direction.Right => RightRoot,
        Direction.Down => DownRoot,
        Direction.Left => LeftRoot,
        _ => throw new NotImplementedException(),
      };

    public bool TryGetIcons(Direction direction, out List<UIQTEIcon> icons)
    {
      if(usedIcons.TryGetValue(direction, out var existIcons))
      {
        icons = existIcons;
        return true;
      }
      else
      {
        icons = null;
        return false;
      }
    }

    private UIQTEIcon GetNewIcon(Transform newRoot)
    {
      if(unusedIcons.TryDequeue(out var existIcon))
      {
        existIcon.transform.SetParent(newRoot);
        existIcon.gameObject.SetActive(true);
        return existIcon;
      }
      else
      {
        var newIcon = Instantiate(IconPrefab, newRoot);
        return newIcon;
      }
    }
  }
}
