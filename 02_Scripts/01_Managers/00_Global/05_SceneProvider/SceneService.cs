using Cysharp.Threading.Tasks;
using LR.UI.Loading;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using LR.Manager.UI;
using Zenject;
using LR.UI;
using LR.Manager.Sound;

namespace LR.Manager.Scene
{
  public class SceneService : ISceneLoader, ISceneProvider
  {
    [Inject] private readonly DiContainer diContainer = null;
    [Inject] private readonly IResourceManager resourceManager = null;
    [Inject] private readonly ICanvasProvider canvasProvider = null;
    [Inject] private readonly AddressableKeySO addressableSO = null;
    [Inject] private readonly SoundSO soundSO = null;
    [Inject] private readonly IBGMController bgmController = null;

    private SceneType currentScene = SceneType.Initialize;
    private AsyncOperationHandle<SceneInstance>? currentHandle;

    public SceneType GetCurrentSceneType()
      => currentScene;

    public async UniTask LoadSceneAsync(
      SceneType sceneType,
      bool useUI = true,
      CancellationToken token = default,
      UnityAction<float> onProgress = null,
      UnityAction onComplete = null,
      Func<UniTask> waitUntilLoad = null,
      bool downVolume = true)
    {
      IUIPresenter loadingPresenter = useUI ? await UILoadingPresenter.CreateAsync(diContainer, addressableSO, canvasProvider, resourceManager)
                                            : null;

      try
      {
        bgmController.UpdateStero(0.5f);
        bgmController.UpdatePitch(1.0f);
        if (downVolume)
          bgmController.UpdateVolume(soundSO.BGMVolume.LoadingVolume);
        if (useUI)
          await loadingPresenter.ActivateAsync(false, token);

        Camera.main.targetTexture = null;

        if (currentHandle.HasValue && currentHandle.Value.IsValid())
        {
          await Addressables.UnloadSceneAsync(currentHandle.Value);
          currentHandle = null;
        }

        var handle = Addressables.LoadSceneAsync(
            GetSceneKey(sceneType),
            LoadSceneMode.Single,
            activateOnLoad: false
        );

        currentHandle = handle;

        while (!handle.IsDone)
        {
          token.ThrowIfCancellationRequested();
          onProgress?.Invoke(handle.PercentComplete);
          await UniTask.Yield();
        }

        await handle.Result.ActivateAsync();

        currentScene = sceneType;

        LocalManager localManager = null;
        foreach (var root in handle.Result.Scene.GetRootGameObjects())
        {
          localManager = root.GetComponentInChildren<LocalManager>(true);
          if (localManager != null)
          {
            await localManager.InitializeAsync();
            break;
          }
        }

        if (useUI)
          await loadingPresenter.DeactivateAsync();

        if (downVolume)
          bgmController.UpdateVolume(1.0f);
        localManager?.Play();
        onComplete?.Invoke();
      }
      catch (OperationCanceledException e) { Debug.Log(e); }
    }

    public async UniTask ReloadCurrentSceneAsync(
      bool useUI = true,
      CancellationToken token = default,
      UnityAction<float> onProgress = null,
      UnityAction onComplete = null,
      Func<UniTask> waitUntilLoad = null)
    {
      await LoadSceneAsync(currentScene, useUI, token, onProgress, onComplete, waitUntilLoad);
    }

    private string GetSceneKey(SceneType sceneType)
    {
      return sceneType switch
      {
        SceneType.Initialize => throw new NotImplementedException(),
        SceneType.Preloading => addressableSO.Path.Scene + addressableSO.SceneName.Preloading,
        SceneType.Lobby => addressableSO.Path.Scene + addressableSO.SceneName.Lobby,
        SceneType.Game => addressableSO.Path.Scene + addressableSO.SceneName.Game,
        _ => throw new NotImplementedException()
      };
    }
  }
}