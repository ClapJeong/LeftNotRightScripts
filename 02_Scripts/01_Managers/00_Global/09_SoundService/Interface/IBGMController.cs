using Cysharp.Threading.Tasks;

namespace LR.Manager.Sound
{
  public interface IBGMController
  {
    public float CurrentVolume { get; }

    public UniTask PlayLobbyBGMAsync(bool isImmediately = false);

    public UniTask PlayGameBGMAsync(bool isImmediately = false);

    public UniTask PlayBGMAsync(BGM bgm, bool isImmediately = false);

    public UniTask StopBGMAsync(bool isImmediately = false);

    public void UpdateVolume(float volume);

    public void UpdateStero(float stero);

    public void UpdatePitch(float pitch);
  }
}
