using Cysharp.Threading.Tasks;
using LR.UI.Enum;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.Indicator
{
  public class BaseUIIndicatorView : BaseUIView
  {
    [Header("[ LeftInput ]")]
    public RectTransform leftUpRectTransform;
    public RectTransform leftRightRectTransform;
    public RectTransform leftDownRectTransform;
    public RectTransform leftLeftRectTransform;

    public override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      gameObject.SetActive(false);
      visibleState = VisibleState.Hidden;
      return UniTask.CompletedTask;
    }

    public override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      leftUpRectTransform.gameObject.SetActive(false);
      leftRightRectTransform.gameObject.SetActive(false);
      leftDownRectTransform.gameObject.SetActive(false);
      leftLeftRectTransform.gameObject.SetActive(false);

      gameObject.SetActive(true);
      visibleState = VisibleState.Showen;
      return UniTask.CompletedTask;
    }

    public RectTransform GetLeftGuideRect(Direction direction)
      => direction switch
      {
        Direction.Up => leftUpRectTransform,
        Direction.Right => leftRightRectTransform,
        Direction.Down => leftDownRectTransform,
        Direction.Left => leftLeftRectTransform,
        _ => throw new System.NotImplementedException(),
      };
  }
}