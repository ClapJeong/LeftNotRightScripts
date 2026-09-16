using UnityEngine;
using LR.UI.Enum;

namespace LR.Manager.UI
{
  public interface ICanvasProvider
  {
    public Canvas GetCanvas(RootType rootType);
  }
}
