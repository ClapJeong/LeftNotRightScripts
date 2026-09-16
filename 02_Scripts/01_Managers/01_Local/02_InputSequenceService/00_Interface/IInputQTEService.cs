using LR.Table.Input;
using UnityEngine;
using UnityEngine.Events;

public interface IInputQTEService : IInputSequenceStopController, ISignalGimmickSwapable
{
  public void Play(
    InputQTEData data, 
    Transform targetTransform,
    UnityAction onSuccess,
    UnityAction onFail);
  }
