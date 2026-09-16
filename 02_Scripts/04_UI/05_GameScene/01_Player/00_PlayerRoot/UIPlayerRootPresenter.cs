using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using LR.Manager.Stage;
using LR.Manager.UI;
using Zenject;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerRootPresenter : IUIPresenter
  {
    public class Model
    {
      public DiContainer diContainer;
      public TableContainer table;
      public StageManager stageManager;
      public PlayerType playerType;
      public IPlayerGetter playerGetter;
      public IStageEventSubscriber stageEventSubscriber;
      public IUIPresenterContainer presenterContainer;

      public Model(
        DiContainer diContainer,
        TableContainer table, 
        StageManager stageManager, 
        PlayerType playerType, 
        IPlayerGetter playerGetter,
        IStageEventSubscriber stageEventSubscriber,
        IUIPresenterContainer presenterContainer)
      {
        this.diContainer = diContainer;
        this.table = table;
        this.stageManager = stageManager;
        this.playerType = playerType;
        this.playerGetter = playerGetter;
        this.stageEventSubscriber = stageEventSubscriber;
        this.presenterContainer = presenterContainer;
      }

      public IPlayerPresenter GetPlayer()
        => playerGetter.GetPlayer(playerType);
    }

    private readonly Model model;
    private readonly UIPlayerRootView view;

    private bool isAllPresentersCreated = false;

    private UIPlayerInputPresenter inputActionPresenter;
    private UIPlayerStatePortraitPresenter statePortraitPresenter;

    public UIPlayerRootPresenter(Model model, UIPlayerRootView view)
    {
      this.model = model;
      this.view = view;

      model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.BeforeShowComplete, () =>
      {
        ActivateAsync().Forget();
      });
      model.presenterContainer.Add(this);

      UniTask.WhenAll(
        CreateInputPresenterAsync(),
        CreatePortraitPresenterAsync())
        .ContinueWith(() => isAllPresentersCreated = true)
        .Forget();
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      model.presenterContainer.Remove(this);

      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      if (isAllPresentersCreated == false)
        await UniTask.WaitUntil(() => isAllPresentersCreated);

      await UniTask.WhenAll(
        inputActionPresenter.DeactivateAsync(isImmediately, token),
        statePortraitPresenter.DeactivateAsync(isImmediately, token),
        view.HideAsync(isImmediately, token));
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      if (isAllPresentersCreated == false)
        await UniTask.WaitUntil(() => isAllPresentersCreated);

      await UniTask.WhenAll(
        inputActionPresenter.ActivateAsync(isImmediately, token),
        statePortraitPresenter.ActivateAsync(isImmediately, token),
        view.ShowAsync(isImmediately, token));
    }

    private async UniTask CreateInputPresenterAsync()
    {
      await UniTask.WaitUntil(() => this.model.playerGetter.IsAllPlayerExist());

      var presenter = this.model.GetPlayer();

      var model = this.model.diContainer.Instantiate<UIPlayerInputPresenter.Model>(new object[]
      {
        this.model.playerType,
        this.model.stageManager.StageDataContainer.stageGimmick,
        presenter.GetInputActionSubscriber(),
        presenter.GetStateSubscriber()
      });
      var view = this.view.InputView;

      inputActionPresenter = new UIPlayerInputPresenter(model, view);
      inputActionPresenter.AttachOnDestroy(this.view.gameObject);
      await inputActionPresenter.DeactivateAsync(true);
    }

    private async UniTask CreatePortraitPresenterAsync()
    {
      await UniTask.WaitUntil(() => this.model.playerGetter.IsAllPlayerExist());

      var player = this.model.GetPlayer();
      var model = this.model.diContainer.Instantiate<UIPlayerStatePortraitPresenter.Model>(new object[]
      {
        player.GetPlayerStatus(),
        this.model.table.PlayerModelSO.GetCollisionData(this.model.playerType),
        player.GetStateProvider(),
        player.GetEnergySubscriber(),
        player.GetEnergyProvider(),
        this.model.playerType,
        player.GetStateSubscriber(),
      });
      var view = this.view.StatePortraitView;

      statePortraitPresenter = new UIPlayerStatePortraitPresenter(model, view);
      statePortraitPresenter.AttachOnDestroy(this.view.gameObject);
      await statePortraitPresenter.DeactivateAsync(true);
    }
  }
}