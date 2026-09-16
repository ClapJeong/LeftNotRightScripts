using LR.Stage.Player;
using LR.Table.TriggerTile;
using UnityEngine;
using LR.Manager.Stage;

namespace LR.Stage.TriggerTile
{
  public class EnergyItemTriggerPresenter : ITriggerTilePresenter
  {
    public class Model
    {
      public DefaultEnergyItemTriggerData data;
      public IPlayerGetter playerGetter;
      public TableContainer table;

      public Model(
        DefaultEnergyItemTriggerData data,
        IPlayerGetter playerGetter, 
        TableContainer table)
      {
        this.data = data;
        this.playerGetter = playerGetter;
        this.table = table;
      }
    }

    private readonly Model model;
    private readonly EnergyItemTriggerView view;

    private bool isEnable;

    public EnergyItemTriggerPresenter(Model model, EnergyItemTriggerView view)
    {
      this.model = model;
      this.view = view;

      view.SubscribeOnEnter(OnEnter);
    }

    public void Enable(bool enable)
    {
      if (isEnable == enable)
        return;

      isEnable = enable;
    }

    public void Restart()
    {
      Enable(true);
      view.gameObject.SetActive(true);
    }

    private void OnEnter(Collider2D collider2D)
    {
      if (isEnable == false)
        return;
      if (collider2D.CompareTag(Tag.Player) == false)
        return;

      var playerType = collider2D
              .GetComponentInParent<IPlayerView>()
              .GetPlayerType();

      var playerPresenter = model
        .playerGetter
        .GetPlayer(playerType.ParseOpposite());

      var reactionController = playerPresenter
        .GetReactionController();


      Enable(false);
      view.gameObject.SetActive(false);
    }

  }
}