using LR.Stage.Player;
using LR.Table.Input;
using LR.Table.TriggerTile;
using UnityEngine;
using LR.Manager.Stage;

namespace LR.Stage.TriggerTile
{
  public class InputtingEnergyItemTriggerPresenter : ITriggerTilePresenter
  {
    public class Model
    {
      public InputtingEnergyItemTriggerData data;
      public IInputQTEService inputQTEService;
      public IInputProgressService inputProgressService;
      public IPlayerGetter playerGetter;
      public TableContainer table;

      public Model(
        InputtingEnergyItemTriggerData data,
        IInputQTEService inputQTEService,
        IInputProgressService inputProgressService,
        IPlayerGetter playerGetter,
        TableContainer table)
      {
        this.data = data;
        this.inputQTEService = inputQTEService;
        this.inputProgressService = inputProgressService;
        this.playerGetter = playerGetter;
        this.table = table;
      }
    }

    private readonly Model model;
    private readonly InputtingEnergyItemTriggerView view;

    private bool isEnable;

    public InputtingEnergyItemTriggerPresenter(Model model, InputtingEnergyItemTriggerView view)
    {
      this.model = model;
      this.view = view;
      view.CaptchaAnimator.Play(view.Input switch
      {
        InputtingEnergyItemTriggerView.EnergyItemInput.QTE => CaptchaAnimator.Animation.SetLeftSprite,
        InputtingEnergyItemTriggerView.EnergyItemInput.Progress => CaptchaAnimator.Animation.SetRightSprite,
        _ => throw new System.NotImplementedException(),
      });

      view.SubscribeOnEnter(OnEnter);
    }

    public void Enable(bool enable)
    {
      if (isEnable == enable)
        return;

      view.SpriteRenderer.enabled = enable;
      isEnable = enable;
    }

    public void Restart()
    {
      Enable(true);
      view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Idle);
    }

    private void OnEnter(Collider2D collider2D)
    {
      if (isEnable == false)
        return;
      if (collider2D.CompareTag(Tag.Player) == false)
        return;
      view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Resolving);

      var playerType = collider2D
              .GetComponentInParent<IPlayerView>()
              .GetPlayerType();

      var inputtingPlayerPresenter = model
        .playerGetter
        .GetPlayer(playerType);
      var inputtingPlayerReactionContrller = inputtingPlayerPresenter
        .GetReactionController();

      var restorePlayerPresenter = model
        .playerGetter
        .GetPlayer(playerType.ParseOpposite());
      var restoreReactionController = restorePlayerPresenter
        .GetReactionController();

      switch (view.Input)
      {
        case InputtingEnergyItemTriggerView.EnergyItemInput.QTE:
          {
            PlayQTE(inputtingPlayerReactionContrller, restoreReactionController);
          }
          break;

        case InputtingEnergyItemTriggerView.EnergyItemInput.Progress:
          {
            PlayProgress(inputtingPlayerReactionContrller, restoreReactionController);
          }
          break;
      }
    }

    private void PlayQTE(
      IPlayerReactionController inputtingReactionController,
      IPlayerReactionController restoreReactionController)
    {
      inputtingReactionController.SetInputting(true);
      model.inputQTEService.Play(
        model.data.QTEData,
        view.transform,
        onSuccess: () =>
        {
          view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Resolved);
          //RestorePlayer(restoreReactionController);          
          inputtingReactionController.SetInputting(false);
          Enable(false);
        },
        () => OnFail(inputtingReactionController));
    }

    private void PlayProgress(
      IPlayerReactionController inputtingReactionController,
      IPlayerReactionController restoreReactionController)
    {
      inputtingReactionController.SetInputting(true);
      model.inputProgressService.Play(
        model.data.InputProgressData,
        view.transform,
        null,
        onComplete: () =>
        {
          view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Resolved);
          //RestorePlayer(restoreReactionController);
          inputtingReactionController.SetInputting(false);
          Enable(false);
        },
        () => OnFail(inputtingReactionController));
    }
    
    private void OnFail(IPlayerReactionController inputtingReactionController)
    {
      view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Idle);
      inputtingReactionController.SetInputting(false);
      Enable(false);
    }

    //private void RestorePlayer(IPlayerReactionController restoreReactionController)
    //  => restoreReactionController.RestoreEnergy(model.data.RestoreValue);
  }
}
