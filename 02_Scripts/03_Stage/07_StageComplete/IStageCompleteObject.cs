using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;

namespace LR.Stage.Complete
{
  public interface IStageCompleteObject
  {
    public UniTask PlayAsync(UnityAction onComplete, CancellationToken token = default);
  }
}