using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Device;
using LR.Manager.Input;
using LR.Manager.Sound;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.U2D;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.Preloading
{
  public class UIVeryFirstCutscene : MonoBehaviour
  {
    [Inject] private readonly IInputActionSubscriber inputActionSubscriber = null;
    [Inject] private readonly IBGMController bgmController = null;
    [Inject] private readonly SoundSO soundSO = null;
    [Inject] private readonly AddressableKeySO addressableKeySO = null;
    [Inject] private readonly IResourceManager resourceManager = null;
    [Inject] private readonly IDeviceEvnetSubscriber deviceEvnetSubscriber = null;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private PlayableDirector director;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float fadeBeginDuration;
    [SerializeField] private float fadeEndDuration;
    [Header("[ Skip ]")]
    [SerializeField] private Image skipIcon;
    [SerializeField] private RectTransform skipInputRectTransform;
    [SerializeField] private Image skipProgressImage;
    [SerializeField] private RectTransform skipProgressRectTransform;    
    [SerializeField] private float skipDuration;

    private readonly CTSContainer skipMoveCTS = new();
    private readonly CTSContainer skipDurationCTS = new();
    private Dictionary<LRDeviceType, SpriteAtlas> atlasese = new();    
    private SubscribeHandle subscribeHandle;
    private bool isFadeOut = false;

    public async void PlayCutscene(UnityAction onStopped)
    {      
      skipIcon.enabled = false;
      atlasese = await InputAtlasProvider.GetInputAtlasesAsync(addressableKeySO, resourceManager);

      subscribeHandle = new(
        () =>
        {
          deviceEvnetSubscriber.SubscribeDeviceEvent(UpdateIcon);
          skipIcon.enabled = true;
          inputActionSubscriber.Subscribe(LRInputType.DialogueSkip, OnSkipInput);
        },
        () =>
        {
          deviceEvnetSubscriber.UnsubscribeDeviceEvent(UpdateIcon);
          inputActionSubscriber.Unsubscribe(LRInputType.DialogueSkip, OnSkipInput);
        });      

      subscribeHandle.Subscribe();

      director.stopped += playable=>
      {
        if (isFadeOut)
          return;

        subscribeHandle.Unsubscribe();
        onStopped?.Invoke();
      };

      await canvasGroup.DOFade(1.0f, fadeBeginDuration);
      director.Play();      
    }

    public async UniTask DestroyAsync()
    {
      isFadeOut = true;
      await DOTween
        .Sequence()
        .Join(canvasGroup.DOFade(0.0f, fadeEndDuration))
        .OnComplete(() => resourceManager.ReleaseInstance(gameObject))
        .ToUniTask();
    }

    private void OnDestroy()
    {
      skipMoveCTS.Dispose();
      skipDurationCTS.Dispose();
      subscribeHandle?.Dispose();
    }

    private void UpdateIcon(LRDeviceType deviceType)
    {
      if (atlasese.TryGetValue(deviceType, out var atlas))
      {
        skipIcon.sprite = atlas.GetSprite(InputIconAssetName.GetInputTypeName(Manager.Input.LRInputType.DialogueSkip, true));
      }
    }

    private void SkipProgress(float value)
      => skipProgressImage.SetFillAmount(value);

    private void OnSkipInput(InputPhase inputPhase)
    {
      switch (inputPhase)
      {
        case InputPhase.Performed: OnSkipPerformed(); break;

        case InputPhase.Canceled: OnSkipCanceled(); break;
      }      
    }

    private void OnSkipPerformed()
    {
      skipMoveCTS.Cancel();
      skipMoveCTS.Create();
      var token = skipMoveCTS.token;
      var duration = 0.1f;

      skipInputRectTransform.DOAnchorPosY(-60.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
      skipProgressRectTransform.DOAnchorPosY(0.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();

      skipDurationCTS.Cancel();
      skipDurationCTS.Create();
      var skipDurationToken = skipDurationCTS.token;
      SkipAsync(skipDurationToken).Forget();
    }

    private void OnSkipCanceled()
    {
      skipMoveCTS.Cancel();
      skipMoveCTS.Create();
      var token = skipMoveCTS.token;
      var duration = 0.1f;

      skipInputRectTransform.DOAnchorPosY(0.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();
      skipProgressRectTransform.DOAnchorPosY(-60.0f, duration).ToUniTask(TweenCancelBehaviour.Complete, token).Forget();

      skipDurationCTS.Cancel();
    }

    private async UniTask SkipAsync(CancellationToken token)
    {
      var duration = 0.0f;
      try
      {
        while (duration < skipDuration)
        {
          token.ThrowIfCancellationRequested();
          SkipProgress(duration / skipDuration);

          duration += Time.deltaTime;
          await UniTask.Yield();
        }
        SkipProgress(1.0f);

        director.Stop();
        //subscribeHandle.Unsubscribe();
        //DestroyAsync().Forget();
      }
      catch (OperationCanceledException)
      {
        SkipProgress(0.0f);
      }
    }

    #region SFXs
    public void PlayBell()
    {
      var bellSFX = soundSO.Get(SFX.FirstCutsceneBell);
      sfxSource.PlayOneShot(bellSFX);
    }

    public void PlayRain()
    {
      var rainSFX = soundSO.Get(SFX.FirstCutsceneRain);
      sfxSource.PlayOneShot(rainSFX);
    }

    public void PlayNightAnimal()
    {
      var nightAnimalSFX = soundSO.Get(SFX.FirstNightAnimal);
      sfxSource.PlayOneShot(nightAnimalSFX);
    }

    public void PlayThunder()
    {
      var thunerSFX = soundSO.Get(SFX.FirstCutsceneThunder);
      sfxSource.PlayOneShot(thunerSFX);
    }

    public void PlayShovel()
    {
      var shovelSFX = soundSO.Get(SFX.FirstCutsceneShovel);
      sfxSource.PlayOneShot(shovelSFX);
    }

    public void PlayLevver()
    {
      var levverSFX = soundSO.Get(SFX.FirstLever);
      sfxSource.PlayOneShot(levverSFX);
    }

    public void PlayElectric()
    {
      var electricSFX = soundSO.Get(SFX.FirstElectric);
      sfxSource.PlayOneShot(electricSFX);
    }

    public void PlayElectricStick()
    {
      var electricStickSFX = soundSO.Get(SFX.FirstElectricStick);
      sfxSource.PlayOneShot(electricStickSFX);
    }

    public void PlayMachineOne()
    {
      var machineSFX = soundSO.Get(SFX.FirstMachineOn);
      sfxSource.PlayOneShot(machineSFX);
    }

    public void PlayBGM()
    {
      PlayBGMAsync().Forget();
    }

    public void PlayAlert()
    {
      var alertSFX = soundSO.Get(SFX.FirstAlert);
      sfxSource.PlayOneShot(alertSFX);
    }

    private async UniTask PlayBGMAsync()
    {
      bgmController.UpdateVolume(0.0f);
      bgmController.PlayGameBGMAsync().Forget();
      var duration = 0.0f;
      var targetDuration = soundSO.BGMVolume.FirstCutsceneFadeDuration;
      while(duration < targetDuration)
      {
        bgmController.UpdateVolume(duration / targetDuration);

        duration += Time.deltaTime;
        await UniTask.Yield();
      }    
    }
    #endregion
  }
}
