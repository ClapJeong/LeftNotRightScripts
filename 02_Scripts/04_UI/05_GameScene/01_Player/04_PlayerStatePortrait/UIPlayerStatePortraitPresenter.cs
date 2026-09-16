using Cysharp.Threading.Tasks;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using LR.UI.GameScene.Player.PlayerPortrait;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using UniRx.Triggers;
using UniRx;
using UnityEngine.U2D;
using LR.Table.Player;
using Zenject;
using LR.Manager.Stage;
using DG.Tweening;
using LR.Manager.UI;
using LR.Manager.GameDataManager;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerStatePortraitPresenter : IUIPresenter
  {
    private enum PortraitDamaged
    {
      None,
      WallHit,
      Damaged,
    }
    public class Model
    {
      [Inject] public IGameModeService gameModeService;
      [Inject] public UISO uiSO;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public IStageEventSubscriber stageEventSubscriber;
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public PlayerStatus playerStatus;
      [Inject] public PlayerCollisionData playerCollisionData;
      [Inject] public IPlayerStateProvider stateProvider;
      [Inject] public IPlayerEnergySubscriber energySubscriber;
      [Inject] public IPlayerEnergyProvider energyProvider;
      [Inject] public PlayerType playerType;
      [Inject] public IPlayerStateSubscriber stateSubscriber;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public IPracticeService practiceService;
    }

    private readonly Model model;
    private readonly UIPlayerStatePortraitView view;

    private readonly PortraitTweenPlayer tweenPlayer;
    private readonly ShaderController shaderController;
    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer blinkCTS = new();
    private readonly CTSContainer vignetteCTS = new();
    private readonly CTSContainer anchoredPositionCTS = new();
    private readonly IDisposable viewUpdateDisposable;
    private SpriteAtlas atlas;    
    private Portrait prevPortrait;
    private PortraitDamaged portraitDamaged = PortraitDamaged.None;

    public UIPlayerStatePortraitPresenter(Model model, UIPlayerStatePortraitView view)
    {
      this.model = model;
      this.view = view;

      view.EasyHatView.Initialize(model.difficultyService.CurrentDifficulty == IDifficultyService.Difficulty.Easy);

      tweenPlayer = new(
        model.playerCollisionData,
        model.uiSO,
        originPortriatAnchoredPos: view.PortraitImageRectTransform.anchoredPosition,
        view.ContentRectTransform,
        view.PortraitImageRectTransform,
        view.VignetteImage,
        view.PortraitImage);
      viewUpdateDisposable =
        view
        .UpdateAsObservable()
        .Subscribe(_ =>
        {
          UpdatePortrait();
        });

      shaderController = new(view.PortraitImage, model.uiSO);
      shaderController.OnIdle();

      var clearEventType = model.playerType switch
      {
        PlayerType.Left => IStageEventSubscriber.StageEventType.LeftClearEnter,
        PlayerType.Right => IStageEventSubscriber.StageEventType.RightClearEnter,
        _ => throw new NotImplementedException(),
      };

      subscribeHandle = new(() =>
      {
        model.energySubscriber.SubscribeOnHit(OnHit);
        model.stateSubscriber.SubscribeOnEnter(PlayerState.Stun, OnStunEnter);
        model.stateSubscriber.SubscribeOnExit(PlayerState.Stun, OnStunExit);
        model.stateSubscriber.SubscribeOnEnter(PlayerState.Exhausted, OnExhaustEnter);
        model.stateSubscriber.SubscribeOnExit(PlayerState.Exhausted, OnExhaustExit);
        model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);        
        model.stageEventSubscriber.SubscribeOnEvent(clearEventType, OnClearEnter);
      },
      () =>
      {
        model.energySubscriber.UnsubscribeOnHit(OnHit);
        model.stateSubscriber.UnsubscribeOnEnter(PlayerState.Stun, OnStunEnter);
        model.stateSubscriber.UnsubscribeOnExit(PlayerState.Stun, OnStunExit);
        model.stateSubscriber.UnsubscribeOnEnter(PlayerState.Exhausted, OnExhaustEnter);
        model.stateSubscriber.UnsubscribeOnExit(PlayerState.Exhausted, OnExhaustExit);
        model.stageEventSubscriber.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);
        model.stageEventSubscriber.UnsubscribeOnEvent(clearEventType, OnClearEnter);
      });

      subscribeHandle.Subscribe();

      model.presenterContainer.Add(this);
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      if (atlas == null)
        await LoadAtlasAsync();

      portraitDamaged = PortraitDamaged.None;
      ChangePortrait(Portrait.Idle);
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      model.presenterContainer.Remove(this);
      ReleaseAtlas();
      shaderController.Dispose();
      vignetteCTS.Dispose();
      anchoredPositionCTS.Dispose();
      blinkCTS.Dispose();
      viewUpdateDisposable.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public async UniTask HideForSpeedrunAsync()
    {
      await view.PortraitImageRectTransform.DOAnchorPosY(model.uiSO.Player.SpeedrunHidePosition, model.uiSO.Player.SpeedrunHideDuration);
      view.PortraitImage.SetAlpha(0.0f);
    }

    private void OnStunEnter()
    {
      shaderController.OnStun();
      blinkCTS.Cancel();
      blinkCTS.Create();
      var token = blinkCTS.token;
      BlinkPortraitAsync(token).Forget();
    }

    private async UniTask BlinkPortraitAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          view.PortraitImage.SetAlpha(model.uiSO.Player.StunBlinkAlpha);
          await UniTask.WaitForSeconds(model.uiSO.Player.StunBlinkDuration, false, PlayerLoopTiming.Update, token);
          token.ThrowIfCancellationRequested();

          view.PortraitImage.SetAlpha(1.0f);
          await UniTask.WaitForSeconds(model.uiSO.Player.StunBlinkDuration, false, PlayerLoopTiming.Update, token);
          token.ThrowIfCancellationRequested();
        }
      }
      catch (OperationCanceledException)
      {
        if(view)
          view.PortraitImage.SetAlpha(1.0f);
      }
    }

    private void OnStunExit()
    {
      shaderController.OnIdle();
      blinkCTS.Cancel();
    }

    private void OnExhaustEnter()
    {
      if (model.gameModeService.IsSpeedRun)
        return;
      
      ChangePortrait(Portrait.Exhausted);
      anchoredPositionCTS.Cancel();
      anchoredPositionCTS.Create();
      var token = anchoredPositionCTS.token;
      tweenPlayer.ExhaustedAsync(token).Forget();
    }

    private void OnRestart()
    {
      anchoredPositionCTS.Cancel();
      tweenPlayer.ResetExhaust();
      shaderController.OnIdle();
    }

    private void OnExhaustExit()
    {
      view.RectTransform.anchoredPosition = Vector3.zero;
    }

    private void UpdatePortrait()
    {
      if (atlas == null)
        return;

      if (model.energyProvider.IsDead)
        return;

      var isPractice = model.practiceService.IsPractice;

      var portrait = isPractice ? Portrait.Practice
        : model.stateProvider.GetCurrentState() switch
      {
        PlayerState.Idle => GetIdlePortrait(),
        PlayerState.Move => GetIdlePortrait(),
        PlayerState.None => GetIdlePortrait(),
        PlayerState.Stun => Portrait.Stun,
        PlayerState.Inputting => Portrait.Inputting,
        PlayerState.Clear => Portrait.Clear,
        PlayerState.Exhausted => Portrait.Exhausted,

        _ => throw new NotImplementedException(),
      };

      if (portrait != prevPortrait)
        ChangePortrait(portrait);
    }

    private void ChangePortrait(Portrait portrait)
    {
      if (prevPortrait != Portrait.Electric && portrait == Portrait.Electric)
      {
        anchoredPositionCTS.Cancel();
        anchoredPositionCTS.Create();
        var token = anchoredPositionCTS.token;
        tweenPlayer.ElectricShakeAsync(token).Forget();
        shaderController.OnElectric();
      }
      else if (prevPortrait == Portrait.Electric && portrait != Portrait.Electric)
      {
        shaderController.OnIdle();
        anchoredPositionCTS.Cancel();
      }
      else if (portrait == Portrait.Clear)
        shaderController.OnComplete();

      view.EasyHatView.UpdateHatPosition((int)portrait);
      view.PortraitImage.sprite = atlas.GetSprite(portrait.ToString());
      prevPortrait = portrait;
    }

    private async UniTask LoadAtlasAsync()
    {
      atlas = await model.resourceManager.LoadAssetAsync<SpriteAtlas>(
        model.addressableKeySO.Path.SpriteAtlas +
        model.addressableKeySO.AtlasName.GetStatePortrait(model.playerType));
    }

    private void ReleaseAtlas()
    {
      model.resourceManager.ReleaseAsset(
        model.addressableKeySO.Path.SpriteAtlas +
        model.addressableKeySO.AtlasName.GetStatePortrait(model.playerType));
    }

    private Portrait GetIdlePortrait()
    {
      if (model.playerStatus.IsElectric.Value)
        return Portrait.Electric;

      if (portraitDamaged != PortraitDamaged.None)
        return portraitDamaged switch
        {
          PortraitDamaged.None => throw new NotImplementedException(),
          PortraitDamaged.WallHit => Portrait.WallHit,
          PortraitDamaged.Damaged => Portrait.Damaged,
          _ => throw new NotImplementedException(),
        };

      var energyNormalized = model.energyProvider.TotalNormalized;
      return energyNormalized <= 0.0f ? Portrait.Exhausted
                                      : energyNormalized <= model.uiSO.Player.PortraitLowEnergy ? Portrait.Low
                                                                                                : Portrait.Idle;
    }

    private void OnClearEnter()
    {
      anchoredPositionCTS.Cancel();
      anchoredPositionCTS.Create();
      var token = anchoredPositionCTS.token;
      tweenPlayer.ClearEnterAsync(token).Forget();
    }

    private void OnHit(PlayerType playerType, DamageType damageType)
    {
      if (playerType != model.playerType)
        return;

      switch (damageType)
      {
        case DamageType.Strong:
          OnStrongDamaged();
          break;

        case DamageType.WallBump:
          OnWallDamaged();
          break;
      }
    }

    private void OnWallDamaged()
    {
      vignetteCTS.Cancel();
      vignetteCTS.Create();
      var vignetteToken = vignetteCTS.token;
      tweenPlayer.WallDamageVignetteAsync(vignetteToken).Forget();

      if (model.playerStatus.IsElectric.Value)
        return;

      shaderController.OnWallHit();
      anchoredPositionCTS.Cancel();
      anchoredPositionCTS.Create();
      var token = anchoredPositionCTS.token;
      
      portraitDamaged = PortraitDamaged.WallHit;
      tweenPlayer.WallDamageShakeAsync(() =>
      {
        shaderController.OnIdle();
        portraitDamaged = PortraitDamaged.None;
      }, token).Forget();
    }

    private void OnStrongDamaged()
    {
      vignetteCTS.Cancel();
      vignetteCTS.Create();
      var vignetteToken = vignetteCTS.token;
      tweenPlayer.StrongDamageVignetteAsync(vignetteToken).Forget();

      if (model.playerStatus.IsElectric.Value)
        return;

      shaderController.OnStrongHit();
      anchoredPositionCTS.Cancel();
      anchoredPositionCTS.Create();
      var token = anchoredPositionCTS.token;

      portraitDamaged = PortraitDamaged.Damaged;
      tweenPlayer.StrongDamageShakeAsync(() =>
      {
        shaderController.OnIdle();
        portraitDamaged = PortraitDamaged.None;
      }, token).Forget();
    }
  }
}
