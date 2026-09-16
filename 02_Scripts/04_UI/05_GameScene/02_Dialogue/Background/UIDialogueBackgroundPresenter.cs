using Cysharp.Threading.Tasks;
using LR.Manager.Local.CameraService;
using LR.Manager.Scene;
using LR.Manager.Sound;
using LR.Manager.UI;
using LR.UI.Enum;
using LR.UI.GameScene.Dialogue.Shake;
using LR.UI.Lobby;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.GameScene.Dialogue
{
  public class UIDialogueBackgroundPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public UISO uiSO;
      [Inject] public ISFXController sfxController;
      [Inject] public DialogueUIDataSO dialogueUIDataSO;
      [Inject] public ISceneProvider sceneProvider;
      [Inject] public IUIPresenterContainer uiContainer;
      [Inject] public ICameraEffectService cameraEffectService;

      [Inject] public int dialogueIndex;
      [Inject] public bool enableLobbyBackground;      
    }    

    private readonly Model model;
    private readonly UIDialogueBackgroundView view;

    private readonly BackgroundShaker backgroundShaker;
    private readonly CTSContainer sfxFadeCTS = new();
    private readonly CTSContainer shakingCTS = new();
    private bool isBackgroundShaking = false;

    private AudioLoopHandle audioLoopHandle;

    public UIDialogueBackgroundPresenter(Model model, UIDialogueBackgroundView view)
    {
      this.model = model;
      this.view = view;

      view.LobbyBackgroundCavasGroup.gameObject.SetActive(model.enableLobbyBackground);
      backgroundShaker = model.diContainer.Instantiate<BackgroundShaker>(new object[] 
      { 
        model.dialogueIndex, 
        view.ShakeFallRoot, 
        view.ShakeFallObjectPrefab ,
        model.dialogueUIDataSO.BackgroundShakeData,
      });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.HideAsync(isImmedieately, token);
    }

    public void UpdateShadow(DialogueDataEnum.Background.Shadow shadowType)
    {
      view.DoctorShadow.enabled = shadowType == DialogueDataEnum.Background.Shadow.Doctor;
      view.LRShadow.enabled = shadowType == DialogueDataEnum.Background.Shadow.LR;
    }

    public void UpdateShake(bool isShake)
    {
      if (isShake && !isBackgroundShaking)
      {
        sfxFadeCTS.Cancel();
        sfxFadeCTS.Create();
        var sfxToken = sfxFadeCTS.token;
        CreateEarthquakeLoopAsync(sfxToken).Forget();
        isBackgroundShaking = true;

        shakingCTS.Cancel();
        shakingCTS.Create();
        var shakeToken = shakingCTS.token;
        backgroundShaker.PlayAsync(shakeToken).Forget();

        switch (model.sceneProvider.GetCurrentSceneType())
        {         
          case SceneType.Lobby:
            model.uiContainer.GetFirst<UILobbyRootPresenter>().ShakeForDialogue();
            break;

          case SceneType.Game:
            model.cameraEffectService.PlayNoise(ICameraEffectService.NoiseType.DialogueShake);
            break;
        }
      }
      else if (!isShake)
      {
        shakingCTS.Cancel();
        isBackgroundShaking = false;

        switch (model.sceneProvider.GetCurrentSceneType())
        {
          case SceneType.Lobby:
            model.uiContainer.GetFirst<UILobbyRootPresenter>().StopDialogueShake();
            break;

          case SceneType.Game:
            model.cameraEffectService.StopNoise();
            break;
        }

        if (audioLoopHandle != null)
        {
          sfxFadeCTS.Cancel();
          sfxFadeCTS.Create();
          var sfxToken = sfxFadeCTS.token;
          StopEarthquakeLoopAsync(sfxToken).Forget();
        }
      }
    }

    private async UniTask CreateEarthquakeLoopAsync(CancellationToken token)
    {
      try
      {
        audioLoopHandle = model.sfxController.CreateLoopSource(CharacterPositionType.Center, SFX.Earthquake, 1.0f, 0.0f);
        var duration = model.dialogueUIDataSO.BackgroundShakeData.AudioFadeInDuration;
        var time = 0.0f;
        while (time < duration)
        {
          token.ThrowIfCancellationRequested();
          if(audioLoopHandle != null)
            audioLoopHandle.Volume = time / duration;

          time += Time.deltaTime;
          await UniTask.Yield();
        }
        if(audioLoopHandle != null)
        audioLoopHandle.Volume = 1.0f;
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask StopEarthquakeLoopAsync(CancellationToken token)
    {
      try
      {
        var duration = model.dialogueUIDataSO.BackgroundShakeData.AudioFadeOutDuration;

        var time = 0.0f;
        while (time < duration)
        {
          token.ThrowIfCancellationRequested();
          var t = (time / duration);
          if(audioLoopHandle != null)
            audioLoopHandle.Volume = 1.0f - t;

          time += Time.deltaTime;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        audioLoopHandle?.Dispose();
        audioLoopHandle = null;
      }
    }

    public void Dispose()
    {
      switch (model.sceneProvider.GetCurrentSceneType())
      {
        case SceneType.Lobby:
          model.uiContainer.GetFirst<UILobbyRootPresenter>()?.StopDialogueShake();
          break;

        case SceneType.Game:
          model.cameraEffectService.StopNoise();
          break;
      }

      shakingCTS.Dispose();
      backgroundShaker.Dispose();
      sfxFadeCTS.Dispose();
      audioLoopHandle?.Dispose();
    }    

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public VisibleState GetVisibleState()
      => view.GetVisibleState();
  }
}
