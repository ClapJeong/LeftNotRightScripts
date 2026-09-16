using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace LR.Manager.Sound
{
  public class GameBGMPlayer : IDisposable
  {
    private readonly AudioSource bgmSource;
    private readonly SoundSO soundSO;
    private readonly GameObject observeTarget;

    private readonly Dictionary<BGM, AudioClip> cache = new();
    private List<BGM> bgms = new();
    private IDisposable updateObserver;
    private int bgmIndex;

    public GameBGMPlayer(
      AudioSource bgmSource, 
      SoundSO soundSO,
      GameObject observeTarget)
    {
      this.bgmSource = bgmSource;
      this.soundSO = soundSO;
      this.observeTarget = observeTarget;
    }
    

    private AudioClip GetCachedClip(BGM bgm)
    {
      if (!cache.TryGetValue(bgm, out var clip))
      {
        clip = soundSO.Get(bgm);
        cache[bgm] = clip;
      }
      return clip;
    }

    public void Start()
    {
      bgms = GetShuffledBGMList();

      foreach (var bgm in bgms)
        GetCachedClip(bgm); // 미리 로드

      bgmIndex = 0;
      PlayNextBGM();

      updateObserver =
          observeTarget
          .UpdateAsObservable()
          .Subscribe(_ => OnUpdateLoop());
    }

    public void Stop()
    {
      updateObserver?.Dispose();
    }

    private void OnUpdateLoop()
    {
      if (bgmSource.isPlaying)
        return;

      PlayNextBGM();
    }

    private void PlayNextBGM()
    {
      var nextClip = GetCachedClip(bgms[bgmIndex]);

      bgmSource.clip = nextClip;
      bgmSource.Play();

      bgmIndex++;
      if (bgmIndex == bgms.Count)
        bgmIndex = 0;
    }

    private List<BGM> GetShuffledBGMList()
    {
      var list = System.Enum.GetValues(typeof(BGM))
          .Cast<BGM>()
          .Where(bgm => bgm != BGM.HazardJoy)
          .ToList();

      // Fisher–Yates shuffle
      for (int i = list.Count - 1; i > 0; i--)
      {
        int j = UnityEngine.Random.Range(0, i + 1);
        (list[i], list[j]) = (list[j], list[i]);
      }

      return list;
    }

    public void Dispose()
    {
      updateObserver?.Dispose();
    }
  }
}
