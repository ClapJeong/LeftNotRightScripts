using LR.Manager.Stage;
using LR.Stage.Player;
using LR.Table.TriggerTile;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace LR.Stage.TriggerTile
{
  public class ConveyorTriggerTilePresenter : ITriggerTilePresenter
  {
    public class Model
    {
      public IPlayerGetter playerGetter;
      public ConveyorTriggerTileData data;

      public Model(IPlayerGetter playerGetter, ConveyorTriggerTileData data)
      {
        this.playerGetter = playerGetter;
        this.data = data;
      }
    }

    private readonly Model model;
    private readonly ConveyorTriggerTileView view;
    private readonly MaterialPropertyBlock mpb = new();

    private bool enable;

    public ConveyorTriggerTilePresenter(Model model, ConveyorTriggerTileView view)
    {
      this.model = model;
      this.view = view;

      view.SubscribeOnEnter(OnEnter);
      view.SubscribeOnExit(OnExit);
    }

    public void Enable(bool enable)
    {
      this.enable = enable;
    }

    public void Restart()
    {
      Enable(true);
    }

    private void OnEnter(Collider2D collider2D)
    {
      if (!enable ||
        !collider2D.CompareTag(Tag.PlayerTileTriggerCollider) ||
        !collider2D.transform.parent.TryGetComponent<IPlayerView>(out var playerView))
        return;

      model
        .playerGetter
        .GetPlayer(playerView.GetPlayerType())
        .GetReactionController()
        .EnterConveyor(view.GetInstanceID(), -1.0f * model.data.Speed * view.transform.up);
    }

    private void OnExit(Collider2D collider2D)
    {
      if (!enable ||
        !collider2D.CompareTag(Tag.PlayerTileTriggerCollider) ||
        !collider2D.transform.parent.TryGetComponent<IPlayerView>(out var playerView))
        return;

      model
        .playerGetter
        .GetPlayer(playerView.GetPlayerType())
        .GetReactionController()
        .ExitConveyor(view.GetInstanceID());
    }
  }
}