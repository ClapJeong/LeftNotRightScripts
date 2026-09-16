using LR.Stage.TriggerTile.Enum;
using System.Collections.Generic;

namespace LR.Manager.Stage.Signal
{
  public interface ISignalIDLifeProvider
  {
    public Dictionary<int, SignalLife> GetSignalIDLifes(int key);
  }
}