using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace LR.UI.Lobby
{
  public class UILobbyDoctorView : BaseUIView
  {
    [field: SerializeField] public RectTransform Content { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.Yield();
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.Yield();
    }
  }
}
