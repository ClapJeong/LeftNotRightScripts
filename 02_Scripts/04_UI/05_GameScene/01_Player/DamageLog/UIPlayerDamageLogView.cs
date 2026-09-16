using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;


namespace LR.UI.GameScene.Player
{
  public class UIPlayerDamageLogView : BaseUIView
  {
    [field: SerializeField] public UINumberFontObject TextPrefab { get; private set; }
    [field: SerializeField] public RectTransform TextRoot { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }
  }
}