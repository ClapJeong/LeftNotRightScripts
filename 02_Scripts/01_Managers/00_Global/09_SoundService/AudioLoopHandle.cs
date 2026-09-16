using Cysharp.Threading.Tasks;
using LR.Manager.Stage;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Manager.Sound
{
  public class AudioLoopHandle : IDisposable
  {
    public class Model
    {
      public GameObject gameObject;
      public AudioSource audioSource;
      public float pitch;
      public float volume;
      public UnityAction onDispose;

      public Model(GameObject gameObject, AudioSource audioSource, float pitch, float volume, UnityAction onDispose)
      {
        this.gameObject = gameObject;
        this.audioSource = audioSource;
        this.pitch = pitch;
        this.volume = volume;
        this.onDispose = onDispose;
      }
    }

    private Model model;
    private readonly IStageEventSubscriber stageEventSubscriber;

    private bool isStopCalled = false;
    private CTSContainer stopCTS;

    public AudioLoopHandle(Model model)
    {
      this.model = model;
      Pitch = model.pitch;
      Volume = model.volume;
      model.audioSource.Play();

      if(LocalManager.instance != null && LocalManager.instance.stageManager != null)
      {
        this.stageEventSubscriber = LocalManager.instance.stageManager;
        stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Exhausted, Dispose);
        stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Complete, Dispose);
      }      
    }

    public float Pitch
    {
      get {  return model.audioSource.pitch; }
      set 
      {
        if (model.audioSource != null)
          model.audioSource.pitch = value; 
      }
    }

    public float Volume
    {
      get { return model.audioSource.volume; }
      set 
      { 
        if(model.audioSource != null)
          model.audioSource.volume = value;
      }
    }

    public void Pause()
      => model.audioSource?.Pause();

    public void Play()
      => model.audioSource?.Play();

    public bool IsPlaying
      => model.audioSource.isPlaying;

    public void Stop(bool isImmediately = true)
    {
      if (isStopCalled)
        return;

      if (isImmediately)
        Dispose();
      else
      {
        isStopCalled = true;
        stopCTS = new();
        var token = stopCTS.token;
        StopWhenFinishedAsync(token).Forget();
      }
    }

    private async UniTask StopWhenFinishedAsync(CancellationToken token)
    {
      model.audioSource.loop = false;

      try
      {
        await UniTask.WaitWhile(() => model.audioSource.isPlaying, PlayerLoopTiming.Update, token);
        GameObject.Destroy(model.gameObject);
      }
      catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
      if(stageEventSubscriber != null)
      {
        stageEventSubscriber.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.Exhausted, Dispose);
        stageEventSubscriber.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.Complete, Dispose);
      }

      model.onDispose?.Invoke();
      stopCTS?.Dispose();
      GameObject.Destroy(model.gameObject);
    }
  }
}
