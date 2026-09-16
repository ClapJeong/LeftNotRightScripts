using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using LR.Stage.Player.Enum;
using UnityEngine;
using LR.Stage.Player.GimmickGuide;
using LR.Stage.StageDataContainer;

namespace LR.Stage.Player
{
  public class BasePlayerPresenter : IPlayerPresenter
  {
    private readonly PlayerModel model;
    private readonly BasePlayerView view;

    private readonly PlayerStatus status;
    private readonly PlayerAnimatorController animatorController;
    private readonly PlayerMoveController moveController;
    private readonly PlayerReactionController reactionController;
    private readonly PlayerInputActionService inputActionService;
    private readonly PlayerStateService stateService;
    private readonly PlayerEnergyService energyService;
    private readonly PlayerEffectController effectController;
    private readonly WallHitAnimationTimer wallHitTimer;

    private readonly CompositeDisposable disposables = new();
    private string inputGuideKey;

    public BasePlayerPresenter(PlayerModel model, BasePlayerView view)
    {
      this.view = view;
      this.model = model;

      view.transform.position = model.beginPosition;

      status = new();
      wallHitTimer = new WallHitAnimationTimer(model.modelSO.GetCollisionData(model.playerType));
      stateService = new PlayerStateService(wallHitTimer)
        .AddTo(disposables);
      effectController = new(view.ParticleSet);
      animatorController = new(view.Animator);
      energyService = new PlayerEnergyService(
        model.energyContainer,
        model.energyDataSO,
        view.SpriteRenderer).AddTo(disposables);
      inputActionService = new PlayerInputActionService(
        model.playerType,
        model.stageStateProvider,
        model.inputActionSubscriber,
        model.inputActionProvider).AddTo(disposables);
      moveController = new PlayerMoveController(
        status,
        view.transform,
        view.Rigidbody2D, 
        inputActionService, 
        model,
        model.stageEventSubscriber,
        stateService,
        model.modelSO.ObstacleLayer).AddTo(disposables);

      reactionController = new PlayerReactionController(
        model.markPlacer,
        status,
        model.effectService,
        model.cameraEffectService,
        model.playerType,
        model.modelSO.GetCollisionData(model.playerType),
        moveController,
        stateService,
        stateService,
        energyService,
        energyService,
        view.Rigidbody2D,
        view.transform,
        model.sfxController,
        view.SpriteRenderer,
        model.modelSO.BlinkData,
        view.Animator,
        model.modelSO.Movement,
        effectController,
        model.modelSO.WallHitSizeData).AddTo(disposables);

      stateService.AddState(PlayerState.Idle, new PlayerIdleState(
        status,
        model.stageStateProvider,
        moveController, 
        inputActionService,
        inputActionService,
        stateService,
        stateService,
        animatorController,
        effectController,
        wallHitTimer));
      stateService.AddState(PlayerState.Move, new PlayerMoveState(
        status,
        moveController, 
        inputActionService,
        inputActionService,
        stateService,
        reactionController,
        animatorController,
        effectController,
        model.playerType,
        model.sfxController,
        model.modelSO.GetCollisionData(model.playerType),
        energyService,
        wallHitTimer,
        model.modelSO.Movement));
      stateService.AddState(PlayerState.Stun, new PlayerStunState(
        moveController,
        stateService,
        animatorController,
        inputActionService,
        effectController,
        model.modelSO.Stun,
        view.SpriteRenderer,
        model.playerType));
      stateService.AddState(PlayerState.Inputting, new PlayerInputState(
        status,
        moveController,
        stateService,
        animatorController,
        model.inputSequenceStopController,
        effectController));
      stateService.AddState(PlayerState.Clear, new PlayerClearState(
        moveController,
        animatorController,
        model.stageRecorderService,
        model.playerType));
      stateService.AddState(PlayerState.Exhausted, new PlayerExhaustedState(
        animatorController,
        moveController,
        effectController,
        wallHitTimer));

      stateService.ChangeState(PlayerState.Idle);

      view.SubscribeOnCollisionEnter2D(reactionController.OnWallBump);
      view.SubscribeOnCollisionExit2D(reactionController.OnWallExit);

      SubscribeEnergyService();
      SubscribeObservable();
    }

    public async UniTask CreateGimmickGuideAsync(StageGimmick stageGimmick)
    {
      if(model.addressableKeySO.GameObjectName.TryGetGimmickGuideViewKey(stageGimmick, out var viewKey))
      {
        inputGuideKey = model.addressableKeySO.Path.GameObjects + viewKey;
        var baseGimmickView = await model.resourceManager.CreateAssetAsync<BaseGimmickGuideView>(inputGuideKey, view.GimmickGuideRoot);
        await baseGimmickView.InitializeAsync(model.playerType, model.diContainer);
      }      
    }

    private void SubscribeEnergyService()
    {
      energyService.SubscribeStateEvent(IPlayerEnergySubscriber.StateEvent.OnExhausted, OnExhaust);
    }

    private void OnExhaust()
    {
      model.markPlacer.MarkDeadPaint(model.playerType, view.transform.position);
      inputActionService.EnableAllInputActions(false);
      stateService.ChangeState(PlayerState.Exhausted);
      model.inputSequenceStopController.Stop();
    }

    private void SubscribeObservable()
    {
      view
        .FixedUpdateAsObservable()
        .Subscribe(_ => OnFixedUpdate()).AddTo(disposables);
      view
        .OnDestroyAsObservable()
        .Subscribe(_ => disposables.Dispose());
    }

    private void OnFixedUpdate()
    {
      if (!model.stageStateProvider.IsPlayingState)
        return;

      stateService.FixedUpdate();
    }


    #region IStageObjectController
    public void Enable(bool enable)
    {
      GetInputActionController().
        EnableAllInputActions(enable);
    }

    public void Restart()
    {
      wallHitTimer.Restart();
      view.transform.position = model.beginPosition;      
      stateService.ChangeState(PlayerState.Idle);      
      moveController.ResetAllDirection();
      moveController.SetLinearVelocity(Vector3.zero);
      energyService.Restart();

      reactionController.IgnoreReactionOnce();
      reactionController.ClearElectric();
      reactionController.ClearConveyor();
    }
    #endregion

    #region Get Interfacees
    public InputRequireGuideView GetInputRequireGuideView()
      => view.GimmickGuideRoot.GetComponentInChildren<InputRequireGuideView>();

    public QTEGuideView GetQTEGuideView()
      => view.GimmickGuideRoot.GetComponentInChildren<QTEGuideView>();

    public SwapGuideView GetSwapGuideView()
      => view.GimmickGuideRoot.GetComponentInChildren<SwapGuideView>();

    public PlayerStatus GetPlayerStatus()
      => status;

    public IPlayerAnimatorController GetAnimatorController()
      => animatorController;

    public IPlayerMoveController GetMoveController()
      => moveController;

    public IPlayerInputActionController GetInputActionController()
      => inputActionService;

    public IPlayerInputActionSubscriber GetInputActionSubscriber()
      => inputActionService;

    public IPlayerInputStateProvider GetInputStateProvider()
      => inputActionService;

    public IPlayerReactionController GetReactionController()
      => reactionController;

    public IPlayerEnergySubscriber GetEnergySubscriber()
      => energyService;

    public IPlayerEnergyProvider GetEnergyProvider()
      => energyService;

    public IPlayerStateProvider GetStateProvider()
      => stateService;

    public IPlayerStateSubscriber GetStateSubscriber()
      => stateService;
    #endregion

    public Transform GetTransform()
      => view.transform;

    public void Dispose()
    {
      model.resourceManager.ReleaseAsset(inputGuideKey);
      disposables.Dispose();
    }    
  }
}