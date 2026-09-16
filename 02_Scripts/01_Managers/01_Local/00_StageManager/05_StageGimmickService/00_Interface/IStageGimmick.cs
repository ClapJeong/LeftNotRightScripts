using LR.Stage.StageDataContainer;
using LR.UI;
using System;

namespace LR.Manager.Stage.Gimmick
{
  public interface IStageGimmick : IDisposable
  {
    public void InjectUI(IUIPresenter presenter);

    public void Begin();

    public void Pause();

    public void Resume();

    public void Restart();

    public void Complete();
  }
}