using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using UnityEngine.Events;
using Zenject;

namespace LR.Manager.Stage.Practice
{
  public class PracticeService : IPracticeSubscriber
  {
    [Inject] private readonly IPracticeService practiceService = null;
    [Inject] private readonly IBGMController bgmController = null;

    private readonly UnityEvent<bool> onPractice = new();

    public void ApplyPracticeMode()
    {
      onPractice?.Invoke(true);
      practiceService.IsPractice = true;

      bgmController.PlayLobbyBGMAsync().Forget();
    }

    public void RevertPracticeMode()
    {
      onPractice?.Invoke(false);
      practiceService.IsPractice = false;

      bgmController.PlayGameBGMAsync().Forget();
    }

    public void SubscribeOnPractice(UnityAction<bool> unityAction)
      => onPractice.AddListener(unityAction);

    public void UnsubscribeOnPractice(UnityAction<bool> unityAction)
      => onPractice.RemoveListener(unityAction);

    public void Dispose()
    {
      
    }
  }
}
