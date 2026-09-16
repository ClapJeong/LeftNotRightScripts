using Cysharp.Threading.Tasks;
using LR.Stage.StageDataContainer;
using System.Threading;
using UnityEngine.Events;

namespace LR.Manager.Local.StagePreview
{
  public interface IStagePreviewCreator
  {
    public UniTask CreatePreviewAsync(int index, UnityAction<StageDataContainer> onComplete, CancellationToken token);

    public void ClosePreview();
  }
}
