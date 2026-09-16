using UnityEngine;

namespace LR.Stage
{
  public interface IStageObjectController
  {
    public void Enable(bool enable);

    public void Restart();
  }
}