using Cysharp.Threading.Tasks;

namespace LR.Manager.Stage
{
  public interface IStageStateHandler
  {
    public bool IsRestartDelay { get; }

    public void Play();

    public void BeginStage();

    public void Pause();

    public void Resume();

    public void Complete();

    public UniTask RestartAsync();

    public void SetState(StageEnum.State state);
  }
}