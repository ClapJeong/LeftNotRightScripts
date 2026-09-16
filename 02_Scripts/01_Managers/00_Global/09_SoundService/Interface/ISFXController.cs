using UnityEngine;

namespace LR.Manager.Sound
{
  public interface ISFXController
  {
    public void PlayOnce(AudioSourceType audioSourceType, SFX sfx, bool onlySingle = true, float volume = 1.0f);
    public void PlayOnce(AudioSourceType audioSourceType, AudioClip clip, bool onlySingle = true, float volume = 1.0f);

    public AudioLoopHandle CreateLoopSource(CharacterPositionType positionType, SFX sfx, float pitch = 1.0f, float volume = 1.0f, bool onlySingle = false);

    public void DisableAllSFX();

    public void EnableAllSFX();

    public void PauseAll(bool isPause);
  }
}
