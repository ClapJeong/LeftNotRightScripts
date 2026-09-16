using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Stage.StageDataContainer;
using LR.Stage.TriggerTile;
using LR.Stage.TriggerTile.Enum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.Manager.Stage.StageObject
{
  public class TriggerTileService :
  IStageObjectSetupService,
  IStageObjectControlService,
  ITriggerTileEventSubscriber
  {
    [Inject] private readonly DiContainer diContainer = null;
    [Inject] private readonly TriggerTileModelSO triggerTileModelSO = null;
    [Inject] private readonly IDifficultyService difficultyService = null;
    private readonly List<ITriggerTilePresenter> cachedTriggers = new();

    private readonly Dictionary<PlayerType, Dictionary<TriggerTileType, UnityEvent>> onEnterEvents = new();
    private readonly Dictionary<PlayerType, Dictionary<TriggerTileType, UnityEvent>> onExitEvents = new();

    private bool isSetupComplete = false;

    public async UniTask SetupAsync(StageDataContainer stageDataContainer, bool isEnableImmediately = false)
    {
      var presenters = new List<ITriggerTilePresenter>();
      var views = stageDataContainer.TriggerTiles;

      onEnterEvents[PlayerType.Left] = new();
      onEnterEvents[PlayerType.Right] = new();
      onExitEvents[PlayerType.Left] = new();
      onExitEvents[PlayerType.Right] = new();

      var currentDifficulty = difficultyService.CurrentDifficulty;
      ITriggerTilePresenter presenter = null;

      foreach (var view in views)
      {
        if (!view.IsEnableDifficulty(currentDifficulty))
          continue;

        var param = new object[1];
        var tileType = view.GetTriggerType();
        switch (tileType)
        {
          case TriggerTileType.LeftClear:
            {
              param[0] = triggerTileModelSO.ClearTrigger;
              var model = diContainer.Instantiate<ClearTriggerTilePresenter.Model>(param);
              var clearTriggerTileView = view as ClearTriggerTileView;
              presenter = new ClearTriggerTilePresenter(model, clearTriggerTileView);
            }
            break;

          case TriggerTileType.RightClear:
            {
              param[0] = triggerTileModelSO.ClearTrigger;
              var model = diContainer.Instantiate<ClearTriggerTilePresenter.Model>(param);
              var clearTriggerTileView = view as ClearTriggerTileView;
              presenter = new ClearTriggerTilePresenter(model, clearTriggerTileView);
            }
            break;

          case TriggerTileType.Spike:
            {
              param[0] = triggerTileModelSO.SpikeTrigger;
              var model = diContainer.Instantiate<SpikeTriggerTilePresenter.Model>(param);
              var spikeTriggerTileView = view as SpikeTriggerTileView;
              presenter = new SpikeTriggerTilePresenter(model, spikeTriggerTileView);
            }
            break;

          case TriggerTileType.DefaultEnergy:
            {
              param[0] = triggerTileModelSO.DefaultEnergyItemTriggerData;
              var model = diContainer.Instantiate<EnergyItemTriggerPresenter.Model>(param);
              var defaultEnergyItemView = view as EnergyItemTriggerView;
              presenter = new EnergyItemTriggerPresenter(model, defaultEnergyItemView);
            }
            break;

          case TriggerTileType.InputtingEnergy:
            {
              param[0] = triggerTileModelSO.InputtingEnergyItemTriggerData;
              var model = diContainer.Instantiate<InputtingEnergyItemTriggerPresenter.Model>(param);
              var inputtingEnergyItemTriggerView = view as InputtingEnergyItemTriggerView;
              presenter = new InputtingEnergyItemTriggerPresenter(model, inputtingEnergyItemTriggerView);
            }
            break;

          case TriggerTileType.DefaultSignal:
            {
              param[0] = triggerTileModelSO.SignalTriggerData;
              var model = diContainer.Instantiate<SignalTriggerPresenter.Model>(param);
              var defaultSignalView = view as SignalTriggerView;
              presenter = new SignalTriggerPresenter(model, defaultSignalView);
            }
            break;

          case TriggerTileType.InputSignal:
            {
              param[0] = triggerTileModelSO.SignalTriggerData;
              var model = diContainer.Instantiate<InputSignalTriggerPresenter.Model>(param);
              var inputSignalView = view as InputSignalTriggerView;
              presenter = new InputSignalTriggerPresenter(model, inputSignalView);
            }
            break;

          case TriggerTileType.Conveyor:
            {
              param[0] = triggerTileModelSO.ConveyorTriggerTileData;
              var model = diContainer.Instantiate<ConveyorTriggerTilePresenter.Model>(param);
              var decayView = view as ConveyorTriggerTileView;
              presenter = new ConveyorTriggerTilePresenter(model, decayView);
            }
            break;

          case TriggerTileType.Portal:
            {
              param[0] = triggerTileModelSO.PortalTriggerData;
              var model = diContainer.Instantiate<PortalTriggerTilePresenter.Model>(param);
              var portalView = view as PortalTriggerTileView;
              presenter = new PortalTriggerTilePresenter(model, portalView);
            }
            break;

          default: throw new System.NotImplementedException();
        }

        if (presenter != null)
        {
          presenter.Enable(isEnableImmediately);
          presenters.Add(presenter);
          cachedTriggers.Add(presenter);

          view.SubscribeOnEnter(OnTriggerEnter);
          view.SubscribeOnEnter(OnTriggerExit);


          void OnTriggerEnter(Collider2D collider2D)
          {
            if (collider2D.CompareTag(Tag.PlayerTileTriggerCollider) == false)
              return;

            var playerType = collider2D
              .GetComponentInParent<IPlayerView>()
              .GetPlayerType();
            onEnterEvents[playerType].TryInvoke(tileType);
          }

          void OnTriggerExit(Collider2D collider2D)
          {
            if (collider2D.CompareTag(Tag.PlayerTileTriggerCollider) == false)
              return;

            var playerType = collider2D
              .GetComponentInParent<IPlayerView>()
              .GetPlayerType();
            onExitEvents[playerType].TryInvoke(tileType);
          }
        }
      }
      await UniTask.CompletedTask;
      isSetupComplete = true;
    }

    public void Release()
    {
      throw new System.NotImplementedException();
    }

    public void EnableAll(bool isEnable)
    {
      foreach (var presenter in cachedTriggers)
        presenter.Enable(isEnable);
    }

    public void RestartAll()
    {
      foreach (var presenter in cachedTriggers)
        presenter.Restart();
    }

    public async UniTask AwaitUntilSetupCompleteAsync()
    {
      await UniTask.WaitUntil(() => isSetupComplete);
    }

    #region ITriggerTileEventHandler
    public void SubscribeOnEnter(PlayerType playerType, TriggerTileType type, UnityAction onEnter)
      => onEnterEvents[playerType].AddEvent(type, onEnter);

    public void SubscribeOnExit(PlayerType playerType, TriggerTileType type, UnityAction onExit)
      => onExitEvents[playerType].AddEvent(type, onExit);

    public void UnsubscribeOnEnter(PlayerType playerType, TriggerTileType type, UnityAction onEnter)
      => onEnterEvents[playerType].RemoveEvent(type, onEnter);

    public void UnsubscribeOnExit(PlayerType playerType, TriggerTileType type, UnityAction onExit)
      => onEnterEvents[playerType].RemoveEvent(type, onExit);
    #endregion
  }
}