using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Manager.Stage.Signal;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Table.TriggerTile;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace LR.Stage.TriggerTile
{
  public class SignalTriggerPresenter : ITriggerTilePresenter
  {
    public class Model
    {
      [Inject] public SignalTriggerData data;

      [Inject] public TableContainer table;
      [Inject] public ISignalKeyRegister signalKeyRegister;
      [Inject] public ISignalConsumer signalConsumer;
      [Inject] public IPlayerGetter playerGetter;
      [Inject] public ISFXController sfxController;
      [Inject] public ColorSO colorSO;
    }

    private readonly Model model;
    private readonly SignalTriggerView view;

    private readonly ACDCGlowPlayer glowPlayer;
    private bool enable;
    private bool isSignalAcquired = false;
    private AudioLoopHandle acdcLoopHandle;

    public SignalTriggerPresenter(Model model, SignalTriggerView view)
    {
      this.model = model;
      this.view = view;

      view.SubscribeOnEnter(OnEnter);
      view.SubscribeOnExit(OnExit);
      RegisterKeys();

      view
        .OnDestroyAsObservable()
        .Subscribe(_ =>
        {
          DisposeLoopHandle();
        });

      if (view.SignalLife == Enum.SignalLife.ActivateAndDeactivate)
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
      {
        DisposeLoopHandle();
      }
    }

    public void Restart()
    {
      Enable(true);
      isSignalAcquired = false;
      view.Animator.Play(AnimatorHash.Signal.Deactivate);
      view.transform.eulerAngles = Vector3.zero;
      DisposeLoopHandle();
      view.IdleEffect.Play();
      if (view.ACDCEffect != null)
        view.ACDCEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

      OnSignalSuccess(collider2D.GetComponentInParent<IPlayerView>().GetPlayerType());
    }

    private void OnSignalSuccess(PlayerType playerType)
    {
      model.signalConsumer.AcquireSignal(view.Key, view.GetHashCode(), out var isFinalSignal);
      isSignalAcquired = true;
      view.Animator.Play(AnimatorHash.Signal.Activate);
      view.IdleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

      switch (view.SignalLife)
      {
        case Enum.SignalLife.OnlyActivate:
          {
            view.ACActivateParticle.Play();

            if(!isFinalSignal)
              model.sfxController.PlayOnce(playerType.ParseToAudioSourceType(), SFX.SignalTriggerEnter);
            Enable(false);
          }
          break;

        case Enum.SignalLife.ActivateAndDeactivate:
          {
            view.IdleEffect.Stop();
            view.ACDCEffect.Play();
            glowPlayer.Play();
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
            if (this == null)
              return;

            glowPlayer.Stop();
            model.signalConsumer.ReleaseSignal(view.Key, view.GetHashCode());
            view.IdleEffect.Play();
            view.ACDCEffect.Stop();
            view.transform.eulerAngles = Vector3.zero;
            view.Animator.Play(AnimatorHash.Signal.Deactivate);
            DisposeLoopHandle();
            model.sfxController.PlayOnce(
              collider2D.
              GetComponentInParent<IPlayerView>()
              .GetPlayerType()
              .ParseToAudioSourceType(), 
              SFX.SignalTriggerACDCExit);
            view.IdleEffect.Play();
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
