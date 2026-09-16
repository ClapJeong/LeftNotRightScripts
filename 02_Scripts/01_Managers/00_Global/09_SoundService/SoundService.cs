using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.Manager.Sound
{
  public class SoundService : 
    IVolumeController, 
    IVolumeProvider,
    IVolumeSubscriber,
    ISFXController,
    IBGMController,
    IDisposable
  {
    private enum BGMState
    {
      None,
      Lobby,
      Game
    }

    private const float DefaultNormalizedVolume = 0.7f;

    private readonly DiContainer diContainer;
    private readonly SoundSO soundSO;
    private readonly AudioSourceContainer audioSourceContainer;

    private readonly UnityEvent<VolumeType, float> onVolumeChanged = new();
    private readonly Dictionary<SFX, List<AudioLoopHandle>> createdHandles = new();
    private readonly Dictionary<AudioClip, int> playOnceFrames = new();
    private readonly GameBGMPlayer bgmPlayer;
    private bool sfxPlayable = true;
    private BGMState currentBGMState = BGMState.None;

    private readonly CTSContainer bgmCTS = new();

    public SoundService(
      DiContainer diContainer,
      SoundSO soundSO,
      AudioSourceContainer audioSourceContainer,
      GlobalManager globalManager)
    {
      this.diContainer = diContainer;
      this.soundSO = soundSO;
      this.audioSourceContainer = audioSourceContainer;
      bgmPlayer = new(audioSourceContainer.BGM, soundSO, globalManager.gameObject);

      audioSourceContainer.LeftUI.PlayOnce.ignoreListenerPause = true;
      audioSourceContainer.LeftUI.LoopPrefab.ignoreListenerPause = true;
      audioSourceContainer.CenterUI.PlayOnce.ignoreListenerPause = true;
      audioSourceContainer.CenterUI.LoopPrefab.ignoreListenerPause = true;
      audioSourceContainer.RightUI.PlayOnce.ignoreListenerPause = true;
      audioSourceContainer.RightUI.LoopPrefab.ignoreListenerPause = true;
      audioSourceContainer.BGM.ignoreListenerPause = true;
    }

    public void InitializeVolumes()
    {
      var masterVolume = GetNormalizedVolume(VolumeType.Master);
      var bgmVolume = GetNormalizedVolume(VolumeType.BGM);
      var sfxVolume = GetNormalizedVolume(VolumeType.SFX);
      SetVolume(VolumeType.Master, masterVolume);
      SetVolume(VolumeType.BGM, bgmVolume);
      SetVolume(VolumeType.SFX, sfxVolume);
    }

    #region ISFXController
    public AudioLoopHandle CreateLoopSource(
      CharacterPositionType positionType, 
      SFX sfx, 
      float pitch = 1.0f, 
      float volume = 1.0f,
      bool onlySingle = false)
    {
      if (onlySingle)
      {
        if(createdHandles.TryGetValue(sfx, out var existHandles) && existHandles.Count > 0)
          return null;
      }        

      var targetContainer = positionType switch
      {
        CharacterPositionType.Left => audioSourceContainer.Left,
        CharacterPositionType.Center => audioSourceContainer.Center,
        CharacterPositionType.Right => audioSourceContainer.Right,
        _ => throw new NotImplementedException(),
      };
      var audioSource = GameObject.Instantiate(targetContainer.LoopPrefab, targetContainer.Root);
      audioSource.clip = soundSO.Get(sfx);

      AudioLoopHandle handle = null;
      var model = new AudioLoopHandle.Model(
        audioSource.gameObject,
        audioSource,
        pitch,
        volume,
        onDispose: () =>
        {
          createdHandles[sfx].Remove(handle);
        });
      handle = diContainer.Instantiate<AudioLoopHandle>(new object[] { model });
     
      if(createdHandles.ContainsKey(sfx))
        createdHandles[sfx].Add(handle);
      else
        createdHandles[sfx] = new List<AudioLoopHandle>() { handle };
      return handle;
    }

    public void PlayOnce(AudioSourceType audioSourceType, SFX sfx, bool onlySingle = true, float volume = 1.0f)
    {
      var clip = soundSO.Get(sfx);
      PlayOnce(audioSourceType, clip, onlySingle, volume);
    }

    public void PlayOnce(AudioSourceType audioSourceType, AudioClip clip, bool onlySingle = true, float volume = 1.0f)
    {
      var currentFrame = Time.frameCount;
      if (!sfxPlayable ||
          (onlySingle && playOnceFrames.TryGetValue(clip, out var lastFrame) && lastFrame == Time.frameCount))
        return;

      playOnceFrames[clip] = currentFrame;
      audioSourceContainer.GetSources(audioSourceType).PlayOnce.PlayOneShot(clip, volume);
    }

    public void DisableAllSFX()
      => sfxPlayable = false;

    public void EnableAllSFX()
      => sfxPlayable = true;

    public void PauseAll(bool isPause)
      => AudioListener.pause = isPause;
    #endregion

    #region IVolumeProvider
    public float GetNormalizedVolume(VolumeType volumeType)
    {
      var name = GetVolumePlayerPrefabsName(volumeType);
      return PlayerPrefs.GetFloat(name, DefaultNormalizedVolume);
    }
    #endregion

    #region IVolumeController
    public void SetVolume(VolumeType volumeType, float normalized)
    {
      normalized = Mathf.Clamp(normalized, 0f, 1f);

      var name = GetVolumePlayerPrefabsName(volumeType);

      PlayerPrefs.SetFloat(name, normalized);

      onVolumeChanged?.Invoke(volumeType, normalized);

      float dB;

      if (normalized <= 0.0001f)
        dB = -80f;
      else
        dB = 20f * Mathf.Log10(normalized);

      soundSO.AudioMixer.SetFloat(name, dB);
      soundSO.AudioMixer.GetFloat(name, out var currentVolume);
    }
    #endregion

    #region IVolumeSubscriber
    public void SubscribeOnVolumeChanged(UnityAction<VolumeType, float> unityAction)
      => onVolumeChanged.AddListener(unityAction);

    public void UnsubscribeOnVolumeChanged(UnityAction<VolumeType, float> unityAction)
      => onVolumeChanged.RemoveListener(unityAction);
    #endregion

    #region IBGMController
    public float CurrentVolume => audioSourceContainer.BGM.volume;

    public async UniTask StopBGMAsync(bool isImmediately = false)
    {
      bgmCTS.Cancel();
      bgmCTS.Create();
      var token = bgmCTS.token;

      var bgmSource = audioSourceContainer.BGM;
      bgmSource.loop = false;
      var fadeDuration = isImmediately ? 0.0f : soundSO.BGMVolume.FadeDuration;
      var originVolume = bgmSource.volume;
      try
      {
        await
          DOTween
          .Sequence()
          .Append(bgmSource.DOFade(0.0f, fadeDuration))
          .AppendCallback(() =>
          {
            bgmSource.Stop();
          })
          .Append(bgmSource.DOFade(1.0f, fadeDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    public async UniTask PlayLobbyBGMAsync(bool isImmediately = false)
    {
      if (currentBGMState == BGMState.Lobby)
        return;

      var bgmSource = audioSourceContainer.BGM;
      bgmSource.loop = true;
      bgmPlayer.Stop();
      currentBGMState = BGMState.Lobby;
      await PlayBGMAsync(BGM.HazardJoy, isImmediately);
    }

    public async UniTask PlayGameBGMAsync(bool isImmediately = false)
    {
      if (currentBGMState == BGMState.Game)
        return;

      currentBGMState = BGMState.Game;

      bgmCTS.Cancel();
      bgmCTS.Create();
      var token = bgmCTS.token;

      var bgmSource = audioSourceContainer.BGM;
      bgmSource.loop = false;
      var fadeDuration = isImmediately ? 0.0f : soundSO.BGMVolume.FadeDuration;
      var originVolume = bgmSource.volume;
      try
      {
        await
          DOTween
          .Sequence()
          .Append(bgmSource.DOFade(0.0f, fadeDuration))
          .AppendCallback(() =>
          {
            bgmPlayer.Start();
          })
          .Append(bgmSource.DOFade(1.0f, fadeDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);        
      }
      catch (OperationCanceledException) { }
    }

    public async UniTask PlayBGMAsync(BGM bgm, bool isImmediately = false)
    {
      var bgmSource = audioSourceContainer.BGM;
      var targetbgm = soundSO.Get(bgm);

      if (bgmSource.clip == targetbgm)
        return;

      bgmCTS.Cancel();
      bgmCTS.Create();
      var token = bgmCTS.token;

      var fadeDuration = isImmediately ? 0.0f : soundSO.BGMVolume.FadeDuration;      
      var originVolume = bgmSource.volume;
      try
      {
        await
          DOTween
          .Sequence()
          .Append(bgmSource.DOFade(0.0f, fadeDuration))
          .AppendCallback(() =>
          {
            bgmSource.clip = targetbgm;
            bgmSource.Play();
          })
          .Append(bgmSource.DOFade(1.0f, fadeDuration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
      finally
      {
        if(bgmSource != null)
        {
          bgmSource.clip = targetbgm;
          bgmSource.volume = 1.0f;
          bgmSource.Play();
        }
      }
    }

    public void UpdateVolume(float volume)
      => audioSourceContainer.BGM.volume = volume;

    public void UpdateStero(float stero)
      => audioSourceContainer.BGM.panStereo = stero;

    public void UpdatePitch(float pitch)
      => audioSourceContainer.BGM.pitch = pitch;
    #endregion

    private string GetVolumePlayerPrefabsName(VolumeType volumeType)
      => volumeType switch
      {
        VolumeType.Master => PlayerPrefsName.Volume.Master,
        VolumeType.BGM => PlayerPrefsName.Volume.BGM,
        VolumeType.SFX => PlayerPrefsName.Volume.SFX,
        _ => throw new NotImplementedException(),
      };

    public void Dispose()
    {
      bgmCTS.Dispose();

      foreach (var handles in createdHandles.Values)
        foreach(var handle in handles.ToList())
          handle?.Dispose();
    }
  }
}
