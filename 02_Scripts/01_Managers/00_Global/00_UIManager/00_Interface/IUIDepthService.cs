using UnityEngine;

namespace LR.Manager.UI
{
  public interface IUIDepthService
  {
    public void SelectTopObject();

    public void RaiseDepth(GameObject newDepthFirstSelectingGameObject);

    public void LowerDepth();
  }
}