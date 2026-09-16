using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Manager.Stage.Signal;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Table.TriggerTile;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace LR.Stage.TriggerTile
{
  public class InputSignalTriggerPresenter : ITriggerTilePresenter
  {
    public class Model
    {
      [Inject] public SignalTriggerData data;

      [Inject] public ISignalKeyRegister signalKeyRegister;
      [Inject] public ISignalConsumer signalConsumer;
      [Inject] public IInputProgressService inputProgressService;
      [Inject] public IInputQTEService inputQTEService;
      [Inject] public IPlayerGetter playerGetter;
      [Inject] public ISFXController sfxController;
      [Inject] public ColorSO colorSO;
    }

    private readonly Model model;
    private readonly InputSignalTriggerView view;

    private readonly ACDCGlowPlayer glowPlayer;
    private bool enable;
    private bool isSignalAcquired = false;
    private AudioLoopHandle acdcLoopHandle;

    public InputSignalTriggerPresenter(Model model, InputSignalTriggerView view)
    {
      this.model = model;
      this.view = view;
      view.CaptchaAnimator.InitializePlayerType(view.Input switch
      {
        Enum.SignalInput.QTE => PlayerType.Left,
        Enum.SignalInput.Progress => PlayerType.Right,
        _ => throw new NotImplementedException(),
      });
      view.CaptchaAnimator.Play(view.Input switch
      {
        Enum.SignalInput.QTE => CaptchaAnimator.Animation.SetLeftSprite,
        Enum.SignalInput.Progress => CaptchaAnimator.Animation.SetRightSprite,
        _ => throw new NotImplementedException(),
      });

      view.SubscribeOnEnter(OnEnter);
      view.SubscribeOnExit(OnExit);
      view
        .OnDestroyAsObservable()
        .Subscribe(_ =>
        {
          DisposeLoopHandle();
        });
      RegisterKeys();

      if(view.SignalLife == Enum.SignalLife.ActivateAndDeactivate)
      {
        glowPlayer = new(view.IconSpriteRenderer, model.data);
        var mainModule = view.ACDCEffect.main;
        mainModule.startColor = model.colorSO.SignalColors[view.Key];
      }
    }

    public void Enable(bool enable)
    {
      this.enable = enable; 
      if(!enable)
        DisposeLoopHandle();
    }

    public void Restart()
    {
      Enable(true);
      isSignalAcquired = false;
      view.Animator.Play(AnimatorHash.Signal.Deactivate);
      view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Idle);
      if(view.ACDCEffect != null)
        view.ACDCEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      view.IdleEffect.Play();
      DisposeLoopHandle();
      glowPlayer?.Stop();
    }

    private void RegisterKeys()
    {
      if(view.IsEnterKeyExist)
        model.signalKeyRegister.RegisterKey(view.Key, view.GetHashCode(), view.SignalLife);
    }

    private void OnEnter(Collider2D collider2D)
    {
      if (!enable ||
          !collider2D.CompareTag(Tag.PlayerTileTriggerCollider) ||
          !view.IsEnterKeyExist)
        return;

      var contactPoint = collider2D.ClosestPoint(view.transform.position);
      var bounceDirection = -((Vector2)view.transform.position - contactPoint).normalized;
      view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Resolving);
      var enterPlayerType = collider2D.GetComponentInParent<IPlayerView>().GetPlayerType();

      model
        .playerGetter
        .GetPlayer(enterPlayerType)
        .GetTransform()
        .position = view.transform.position;

      var reactionController = model
        .playerGetter
        .GetPlayer(enterPlayerType)
        .GetReactionController();
      reactionController.SetInputting(true);

      switch (view.Input)
      {
        case Enum.SignalInput.QTE:
          {
            PlayQTE(enterPlayerType, bounceDirection, reactionController);
          }
          break;

        case Enum.SignalInput.Progress:
          {
            PlayProgress(enterPlayerType, bounceDirection, reactionController);
          }
          break;
      }
    }

    private void PlayQTE(PlayerType playerType, Vector3 bounceDirection, IPlayerReactionController reactionController)
    {
      model.inputQTEService.Play(
        model.data.QTEData,
        view.transform,
        onSuccess: () =>
        {
          OnSignalSuccess(playerType);          
        },
        onFail: () =>
        {
          OnInputFail(bounceDirection, reactionController);
        });
    }

    private void PlayProgress(PlayerType playerType, Vector3 bounceDirection, IPlayerReactionController reactionController)
    {
      model.inputProgressService.Play(
        model.data.ProgressData,
        view.transform,
        onProgress: null,
        onComplete: () =>
        {
          OnSignalSuccess(playerType);
        },
        onFail: () =>
        {
          OnInputFail(bounceDirection.normalized, reactionController);
        });
    }

    private void OnSignalSuccess(PlayerType playerType)
    {
      model
        .playerGetter
        .GetPlayer(playerType)
        .GetReactionController()
        .SetInputting(false);
      model.signalConsumer.AcquireSignal(view.Key, view.GetHashCode(), out var isFinalSignal);
      isSignalAcquired = true;
      view.Animator.Play(AnimatorHash.Signal.Activate);
      view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Resolved);
      view.IdleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

      switch (view.SignalLife)
      {
        case Enum.SignalLife.OnlyActivate:
          {
            view.ACActivateParticle.Play();
            Enable(false);
            if (!isFinalSignal)
              model.sfxController.PlayOnce(playerType.ParseToAudioSourceType(), SFX.SignalTriggerEnter);
          }
          break;

        case Enum.SignalLife.ActivateAndDeactivate:
          {
            glowPlayer.Play();
            model
              .playerGetter
              .GetPlayer(playerType)
              .GetMoveController()
              .MovePosition(view.transform.position);
            view.ACDCEffect.Play();
          }
          break;
      }
    }

    private void OnInputFail(Vector3 bounceDirection, IPlayerReactionController reactionController)
    {
      if (view == null)
        return;

      reactionController
        .SetInputting(false);

      DisposeLoopHandle();
      view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Idle);
      switch (view.InputFail)
      {
        case Enum.SignalInputFail.Bounce:
          {            
            reactionController.Bounce(model.data.FailBounceData, bounceDirection.normalized);
          }
          break;

        case Enum.SignalInputFail.BounceAndStun:
          {
            reactionController.Stun();
            reactionController.Bounce(model.data.FailBounceData, bounceDirection.normalized);            
          }
          break;
      }
    }

    private void OnExit(Collider2D collider2D)
    {
      if (!enable ||
          !isSignalAcquired ||
          !collider2D.CompareTag(Tag.PlayerTileTriggerCollider) ||
          !view.IsEnterKeyExist)
        return;

      switch (view.SignalLife)
      {
        case Enum.SignalLife.OnlyActivate:
          {
            
          }
          break;

        case Enum.SignalLife.ActivateAndDeactivate:
          {
            glowPlayer.Stop();
            model.signalConsumer.ReleaseSignal(view.Key, view.GetHashCode());
            view.transform.eulerAngles = Vector3.zero;
            view.Animator.Play(AnimatorHash.Signal.Deactivate);
            view.CaptchaAnimator.Play(CaptchaAnimator.Animation.Reanimated);
            DisposeLoopHandle();
            view.IdleEffect.Play();
            view.ACDCEffect.Stop();
          }
          break;
      }
    }

    private void DisposeLoopHandle()
    {
      glowPlayer?.Dispose();
      acdcLoopHandle?.Dispose();
      acdcLoopHandle = null;
    }
  }
}
