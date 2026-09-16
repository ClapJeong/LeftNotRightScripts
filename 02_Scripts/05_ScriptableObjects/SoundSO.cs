using LR.Manager.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SoundSO", menuName = "SO/Sound")]
public class SoundSO : ScriptableObject
{
  [System.Serializable]
  public class BGMVolumeData
  {
    [field: SerializeField] public float FirstCutsceneFadeDuration { get; private set; }
    [field: SerializeField] public float FadeDuration { get; private set; }
    [field: SerializeField] public float LoadingVolume { get; private set; }
    [field: SerializeField] public float ExhaustedVolume { get; private set; }
    [field: SerializeField] public float ExhaustedPitch {  get; private set; }
    [field: SerializeField] public float GameBGMInterval { get; private set; }
    [field: SerializeField] public float DialogueVolume { get; private set; }
    [field: SerializeField] public float EpilogueFadeDuration {  get; private set; }
  }

  [SerializeField] private AudioMixer audioMixer;
  public AudioMixer AudioMixer => audioMixer;

  [Header("SFX")]
  [SerializeField] private List<AudioClip> sfxClips = new();

  [Header("BGM")]
  [SerializeField] private List<AudioClip> bgmClips = new();
  [SerializeField] private BGMVolumeData bgmVolumeData;
  public BGMVolumeData BGMVolume => bgmVolumeData;

  [Header("Talking")]
  public List<AudioClip> leftTalkings = new();
  public List<AudioClip> centerTalkings = new();
  public List<AudioClip> rightTalkings = new();
  

  public AudioClip Get(SFX sfx)
  {
    return sfxClips[(int)sfx];
  }

  public AudioClip Get(BGM bgm)
  {
    return bgmClips[(int)bgm];
  }

  public AudioClip GetRandomTalkingSFX(CharacterPositionType characterPositionType)
  {
    var clips = characterPositionType switch
    {
      CharacterPositionType.Left => leftTalkings,
      CharacterPositionType.Center => centerTalkings,
      CharacterPositionType.Right => rightTalkings,
      _ => throw new System.NotImplementedException(),
    };

    return clips[Random.Range(0, clips.Count)];
  }
}