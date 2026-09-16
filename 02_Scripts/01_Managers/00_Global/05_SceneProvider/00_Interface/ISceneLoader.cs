using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Events;

namespace LR.Manager.Scene
{
  public interface ISceneLoader
  {
    public UniTask LoadSceneAsync(
      SceneType sceneType,
      bool useUI = true,
      CancellationToken token = default,
      UnityAction<float> onProgress = null,
      UnityAction onComplete = null,
      Func<UniTask> waitUntilLoad = null,
      bool downVolume = true);

    public UniTask ReloadCurrentSceneAsync(
      bool useUI = true,
      CancellationToken token = default,
      UnityAction<float> onProgress = null,
      UnityAction onComplete = null,
      Func<UniTask> waitUntilLoad = null);
  }
}