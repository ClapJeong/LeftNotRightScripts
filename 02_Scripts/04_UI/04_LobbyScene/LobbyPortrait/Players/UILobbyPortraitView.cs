using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.Lobby
{
  public class UILobbyPortraitView : BaseUIView
  {
    [field: SerializeField] public RectTransform PortraitRectTransform {  get; private set; }
    [field: SerializeField] public Image PortraitImage { get; private set; }
    [field: SerializeField] public Sprite ClearSprite { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      await UniTask.Yield();
      visibleState = Enum.VisibleState.Hidden;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      await UniTask.Yield();
      visibleState = Enum.VisibleState.Showen;
    }
  }
}
