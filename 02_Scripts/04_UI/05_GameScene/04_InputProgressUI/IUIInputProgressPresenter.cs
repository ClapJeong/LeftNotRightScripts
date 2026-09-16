using UnityEngine;

namespace LR.UI.GameScene.InputProgress
{
  public interface IUIInputProgressPresenter : IUIPresenter, ISignalGimmickSwapable
  {
    public void SetFollowTransform(Transform transform);

    public void OnProgress(float normalizedValue);

    public void OnComplete();

    public void OnFail();
  }
}
