using LR.Stage.Player;
using LR.Stage.Player.Enum;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.TriggerTile.Portal
{
  public class PlayerDetectArea : MonoBehaviour
  {
    [SerializeField] private PlayerType targetPlayerType;
    private readonly UnityEvent<Collider2D> onTriggerEnter = new();
    private readonly UnityEvent<Collider2D> onTriggerExit = new();

    public void SubscribeOnTriggerEnter(UnityAction<Collider2D> onEnter)
      => onTriggerEnter.AddListener(onEnter);

    public void SubscribeOnTriggerExit(UnityAction<Collider2D> onExit)
      => onTriggerExit.AddListener(onExit);

    private void OnTriggerEnter2D(Collider2D collision)
    {
      var playerView = collision.GetComponentInParent<IPlayerView>();
      if (playerView == null || playerView.GetPlayerType() != targetPlayerType)
        return;

      onTriggerEnter?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
      var playerView = collision.GetComponentInParent<IPlayerView>();
      if (playerView == null || playerView.GetPlayerType() != targetPlayerType)
        return;

      onTriggerExit?.Invoke(collision);
    }
  }
}
