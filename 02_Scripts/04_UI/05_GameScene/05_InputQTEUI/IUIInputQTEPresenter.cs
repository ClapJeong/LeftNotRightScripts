using UnityEngine;

namespace LR.UI.GameScene.InputQTE
{
  public interface IUIInputQTEPresenter : IUIPresenter, ISignalGimmickSwapable
  {
    public void SetFollowTransform(Transform transform);

    public void OnSequenceBegin();

    public void OnQTEBegin(Direction direction);

    public void OnQTEProgress(float value);

    public void OnQTEResult(bool isSuccess);

    public void OnSequenceProgress(float value);

    public void OnQTECountChanged(int count);

    public void OnSequenceResult(bool isSuccess);
  }
}