using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.Effect
{
  public abstract class BaseEffectObject : MonoBehaviour
  {
    public abstract UniTask PlayAsync(UnityAction onComplete = null);

    public abstract void StopImmediately();
  }
}