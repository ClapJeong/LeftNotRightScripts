using UnityEngine;

namespace LR.Manager.Sound
{
  public class AudioSourceContainer : MonoBehaviour
  {
    [System.Serializable]
    public class AudioSources
    {
      [field: SerializeField] public Transform Root { get; private set; }
      [field: SerializeField] public AudioSource PlayOnce { get; private set; }
      [field: SerializeField] public AudioSource LoopPrefab {  get; private set; }
    }
    [field: Header("[ SFX ]")]
    [field: SerializeField] public AudioSources Left {  get; private set; }
    [field: SerializeField] public AudioSources Center { get; private set; }
    [field: SerializeField] public AudioSources Right { get; private set; }
    [field: SerializeField] public AudioSources LeftUI { get; private set; }
    [field: SerializeField] public AudioSources CenterUI { get; private set; }
    [field: SerializeField] public AudioSources RightUI { get; private set; }
    public AudioSources GetSources(AudioSourceType audioSourceType)
      => audioSourceType switch
      {
        AudioSourceType.Left => Left,
        AudioSourceType.Center => Center,
        AudioSourceType.Right => Right,
        AudioSourceType.LeftUI => LeftUI,
        AudioSourceType.CenterUI => CenterUI,
        AudioSourceType.RightUI => RightUI,
        _ => throw new System.NotImplementedException(),
      };

    [field: Header("[ BGM ]")]

    [field: SerializeField] public AudioSource BGM { get; private set; }
  }
}
