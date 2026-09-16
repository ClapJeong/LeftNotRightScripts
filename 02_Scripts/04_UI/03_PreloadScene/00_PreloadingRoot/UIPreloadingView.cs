using Cysharp.Threading.Tasks;
using System.Threading;

namespace LR.UI.Preloading
{
  public class UIPreloadingView : BaseUIView
  {
    public override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      return UniTask.CompletedTask;
    }

    public override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      return UniTask.CompletedTask;
    }
  }
}