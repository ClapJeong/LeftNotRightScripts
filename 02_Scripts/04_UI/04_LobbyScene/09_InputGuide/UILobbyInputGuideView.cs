using Cysharp.Threading.Tasks;
using LR.UI.Input;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LR.UI.Lobby
{
  public class UILobbyInputGuideView : BaseUIView
  {
    [field: SerializeField] public UIInputView LeftInputView { get; private set; }
    [field: SerializeField] public UIInputView RightInputView { get; private set; }

    public override async UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      gameObject.SetActive(false);
      visibleState = Enum.VisibleState.Hidden;
      await UniTask.CompletedTask;
    }

    public override async UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      gameObject.SetActive(true);
      visibleState = Enum.VisibleState.Showen;
      await UniTask.CompletedTask;
    }

    public void EnableLeftImage(List<Direction> directions)
    {
      foreach (var inputSet in LeftInputView.InputSets)
      {
        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
          var isExist = directions.Contains(direction);
          LeftInputView.GetImage(inputSet.DeviceType, direction).SetAlpha(isExist ? 1.0f : 0.3f);
        }
      }      
    }

    public void EnableRightImage(List<Direction> directions)
    {
      foreach (var inputSet in LeftInputView.InputSets)
      {
        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
          var isExist = directions.Contains(direction);
          RightInputView.GetImage(inputSet.DeviceType, direction).SetAlpha(isExist ? 1.0f : 0.3f);
        }
      }
    }
  }
}
