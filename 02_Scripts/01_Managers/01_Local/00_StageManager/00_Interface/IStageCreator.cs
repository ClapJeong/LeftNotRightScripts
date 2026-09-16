using Cysharp.Threading.Tasks;

namespace LR.Manager.Stage
{
  public interface IStageCreator
  {
    public UniTask CreateAsync(int index, bool isEnableImmediately = false);
  }
}