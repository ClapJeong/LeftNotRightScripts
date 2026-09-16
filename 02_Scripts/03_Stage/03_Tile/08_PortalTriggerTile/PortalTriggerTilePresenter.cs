using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Stage;
using LR.Stage.Player;
using LR.Table.TriggerTile;
using System;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace LR.Stage.TriggerTile
{
  public class PortalTriggerTilePresenter : ITriggerTilePresenter
  {
    public class Model
    {
      [Inject] public IPlayerGetter playerGetter;
      [Inject] public PortalTriggerData data;
    }

    private readonly Model model;
    private readonly PortalTriggerTileView view;

    private readonly MaterialPropertyBlock glowMaterialPropertyBlock = new();
    private readonly CTSContainer glowCTS = new();
    private readonly CTSContainer moveCTS = new();

    private bool enable;

    public PortalTriggerTilePresenter(Model model, PortalTriggerTileView view)
    {
      this.model = model;
      this.view = view;

      view.SubscribeOnEnter(OnTileEnter2D);

      view.DetectArea.SubscribeOnTriggerEnter(OnDetectEnter2D);
      view.DetectArea.SubscribeOnTriggerExit(OnDetectExit2D);
      view.SpriteRenderer.GetPropertyBlock(glowMaterialPropertyBlock);
      view.EffectAnimator.Play(AnimatorHash.PortalEffect.Idle);

      var token = glowCTS.token;
      GlowAsync(token).Forget();
      view
        .OnDestroyAsObservable()
        .Subscribe(_ =>
        {
          glowCTS.Dispose();
        });
    }

    public void Enable(bool enable)
    {
      this.enable = enable;
    }

    public void Restart()
    {
      view.InnerEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      view.TargetPortal.OutterEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

      moveCTS.Cancel();
      view.EffectAnimator.Play(AnimatorHash.PortalEffect.Idle);
    }

    private async UniTask GlowAsync(CancellationToken token)
    {
      try
      {
        var duration = 0.0f;
        while (true)
        {
          token.ThrowIfCancellationRequested();
          while(duration < model.data.GlowDuration)
          {
            token.ThrowIfCancellationRequested();
            var t = duration / model.data.GlowDuration;
            var intensity = Mathf.Lerp(model.data.GlowMin, model.data.GlowMax, t);
            glowMaterialPropertyBlock.SetFloat(ShaderHash.Glow._Intensity, intensity);
            view.SpriteRenderer.SetPropertyBlock(glowMaterialPropertyBlock);
            duration += Time.deltaTime;
            await UniTask.Yield();
          }
          while (duration > 0.0f)
          {
            token.ThrowIfCancellationRequested();
            var t = duration / model.data.GlowDuration;
            var intensity = Mathf.Lerp(model.data.GlowMin, model.data.GlowMax, t);
            glowMaterialPropertyBlock.SetFloat(ShaderHash.Glow._Intensity, intensity);
            view.SpriteRenderer.SetPropertyBlock(glowMaterialPropertyBlock);
            duration -= Time.deltaTime;
            await UniTask.Yield();
          }
        }
      }
      catch (OperationCanceledException) { }
    }    

    private void OnTileEnter2D(Collider2D collider2D)
    {
      if (!IsEnable(collider2D))
        return;

      var playerType = collider2D.GetComponent<IPlayerView>().GetPlayerType();
      var playerPresenter = model.playerGetter.GetPlayer(playerType);

      if (!playerPresenter.GetPlayerStatus().IsTeleported)
      {
        view.EnterEffect.Play();

        moveCTS.Cancel();
        moveCTS.Create();
        var token = moveCTS.token;
        MoveEffectAsync(token).Forget();
      }        
      playerPresenter
        .GetReactionController()
        .Teleport(view.TargetPortal.transform.position + Vector3.up * 0.1f);      
    }

    private async UniTask MoveEffectAsync(CancellationToken token)
    {
      try
      {
        view.MoveEffect.Play();
        await view
          .MoveEffect
          .transform
          .DOMove(view.TargetPortal.transform.position, model.data.MoveEffectMoveDuration)
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
      finally
      {
        if (this != null)
        {          
          view.MoveEffect.Stop();
          view.MoveEffect.transform.localPosition = Vector3.zero;
        }          
      }
    }

    private void OnDetectEnter2D(Collider2D collider2D)
    {
      if (!IsEnable(collider2D))
        return;

      view.EffectAnimator.Play(AnimatorHash.PortalEffect.Inner);
      view.TargetPortal.EffectAnimator.Play(AnimatorHash.PortalEffect.Outter);
      view.InnerEffect.Play();
      view.TargetPortal.OutterEffect.Play();
    }

    private void OnDetectExit2D(Collider2D collider2D)
    {
      if (!IsEnable(collider2D))
        return;

      view.EffectAnimator.Play(AnimatorHash.PortalEffect.Idle);
      view.TargetPortal.EffectAnimator.Play(AnimatorHash.PortalEffect.Idle);

      view.InnerEffect.Stop();
      view.TargetPortal.OutterEffect.Stop();
    }

    private bool IsEnable(Collider2D collider2D)
    {
      if (!enable)
        return false;

      if (!collider2D.CompareTag(Tag.Player))
        return false;

      if (!collider2D.TryGetComponent<IPlayerView>(out var playerView))
        return false;

      return true;
    }
  }
}
