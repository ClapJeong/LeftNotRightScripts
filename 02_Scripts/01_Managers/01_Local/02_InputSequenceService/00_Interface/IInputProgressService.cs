using LR.Table.Input;
using UnityEngine;
using UnityEngine.Events;

public interface IInputProgressService : IInputSequenceStopController, ISignalGimmickSwapable
{
  public void Play(
    InputProgressData data,
    Transform followTarget,
    UnityAction<float> onProgress, 
    UnityAction onComplete,
    UnityAction onFail);
}
