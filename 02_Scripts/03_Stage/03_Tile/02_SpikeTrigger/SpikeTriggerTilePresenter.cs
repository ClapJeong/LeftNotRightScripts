using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Table.TriggerTile;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace LR.Stage.TriggerTile
{
  public class SpikeTriggerTilePresenter : ITriggerTilePresenter
  {
    public class Model
    {
      public SpikeTriggerData data;
      public IPlayerGetter playerGetter;
      public IEffectService effectService;
      public ISFXController sfxController;

      public Model(
        SpikeTriggerData data, 
        IPlayerGetter playerGetter, 
        IEffectService effectService,
        ISFXController sfxController)
      {
        this.data = data;
        this.playerGetter = playerGetter;
        this.effectService = effectService;
        this.sfxController = sfxController;
      }
    }

    private readonly Model model;
    private readonly SpikeTriggerTileView view;

    private bool isEnable = true;

    public SpikeTriggerTilePresenter(Model model, SpikeTriggerTileView view)
    {
      this.model = model;
      this.view = view;

      view.SubscribeOnEnter(OnSpikeEnter);
      view.SubscribeOnExit(OnSpikeExit);
    }

    public void Enable(bool enabled)
    {
      isEnable = enabled;
    }

    public void Restart()
    {
      isEnable = true;
    }

    private void OnSpikeEnter(Collider2D collider2D)
    {
      if (collider2D.CompareTag(Tag.Player) == false)
        return;

      if (!isEnable)
        return;

      var playerView = collider2D.gameObject.GetComponentInParent<IPlayerView>();
      var playerType = playerView.GetPlayerType();
      var playerPresenter = model.playerGetter.GetPlayer(playerType);

      var reactionController = playerPresenter.GetReactionController();
      reactionController
        .EnterElectric(view.GetHashCode());
    }

    private void OnSpikeExit(Collider2D collider2D)
    {
      if (collider2D.CompareTag(Tag.Player) == false)
        return;

      if (!isEnable)
        return;

      var playerView = collider2D.gameObject.GetComponentInParent<IPlayerView>();
      var playerType = playerView.GetPlayerType();
      var playerPresenter = model.playerGetter.GetPlayer(playerType);

      var reactionController = playerPresenter.GetReactionController();
      reactionController
        .ExitElectric(view.GetHashCode());
    }
  }
}