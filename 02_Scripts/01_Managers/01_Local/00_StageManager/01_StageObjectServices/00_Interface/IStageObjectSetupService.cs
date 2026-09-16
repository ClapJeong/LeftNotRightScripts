using Cysharp.Threading.Tasks;
using LR.Stage.StageDataContainer;

namespace LR.Manager.Stage.StageObject
{
  public interface IStageObjectSetupService
  {
    public UniTask SetupAsync(StageDataContainer stageDataContainer, bool isEnableImmediately = false);

    public void Release();

    public UniTask AwaitUntilSetupCompleteAsync();
  }
}