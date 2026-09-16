using LR.Stage.Player;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.InteractiveObject
{
  public class ShooterSFXArea : MonoBehaviour
  {
    private readonly UnityEvent<IPlayerView> onPlayerEnter = new();
    private readonly UnityEvent onPlayerExit = new();

    public void SubscribeOnPlayerEnter(UnityAction<IPlayerView> onEnter)
      => onPlayerEnter.AddListener(onEnter);

    public void SubscribeOnPlayerExit(UnityAction onExit)
      => onPlayerExit.AddListener(onExit);

    private void OnTriggerEnter2D(Collider2D collision)
    {
      var playerView = collision.GetComponentInParent<IPlayerView>();
      if (playerView == null)
        return;

      onPlayerEnter?.Invoke(playerView);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
      var playerView = collision.GetComponentInParent<IPlayerView>();
      if (playerView == null)
        return;

      onPlayerExit?.Invoke();
    }
  }
}
