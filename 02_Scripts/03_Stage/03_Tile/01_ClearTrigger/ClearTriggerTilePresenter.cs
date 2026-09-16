using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Stage;
using LR.Stage.Player;
using LR.Stage.TriggerTile.Enum;
using LR.Table.TriggerTile;
using System;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace LR.Stage.TriggerTile
{
  public class ClearTriggerTilePresenter : ITriggerTilePresenter
  {
    public class Model
    {      
      [Inject] public IPlayerGetter playerGetter;
      [Inject] public IStageResultHandler stageResultHandler;
      [Inject] public IEffectService effectService;
      [Inject] public IPracticeService practiceService;

      [Inject] public ClearTriggerData data;
    }

    private readonly Model model;
    private readonly ClearTriggerTileView view;

    private readonly MaterialPropertyBlock matBlock = new();
    private readonly CTSContainer blinkCTS = new();
    private bool isEnter;
    private bool isEnable = true;    

    public ClearTriggerTilePresenter(Model model, ClearTriggerTileView view)
    {
      this.model = model;
      this.view = view;

      view.SubscribeOnEnter(OnEnter);
      view.SubscribeOnExit(OnExit);
      view.SpriteRenderer.GetPropertyBlock(matBlock);

      view
        .OnDestroyAsObservable()
        .Subscribe(_ =>
        {
          blinkCTS.Dispose();
        });
      var token = blinkCTS.token;
      BlinkAsync(token).Forget();
    }

    public void Enable(bool enabled)
    {
      isEnable = enabled;
    }

    public void Restart()
    {
      isEnable = true;
    }

    private void OnEnter(Collider2D collider2D)
    {
      var isPractice = model.practiceService.IsPractice;
      if (isPractice)
        return;
      if (collider2D.CompareTag(Tag.PlayerTileTriggerCollider) == false)
        return;
      if (!isEnable)
        return;

      var playerType = collider2D.GetComponentInParent<IPlayerView>().GetPlayerType();
      var player = model
        .playerGetter
        .GetPlayer(playerType);

      if (player.GetEnergyProvider().IsDead)
        return;

      switch (view.GetTriggerType())
      {
        case TriggerTileType.LeftClear:
          model.stageResultHandler.LeftClearEnter();
          break;

        case TriggerTileType.RightClear:
          model.stageResultHandler.RightClearEnter();
          break;

        default: throw new System.NotImplementedException();
      }

      isEnter = true;

      player
        .GetReactionController()
        .Clear();

      collider2D.transform.parent.transform.position = view.transform.position;

      view.IdleParticle.Stop();
      view.ActivateParticle.Play();
    }

    private void OnExit(Collider2D collider2D)
    {
      var isPractice = model.practiceService.IsPractice;
      if (isPractice)
        return;
      if (collider2D.CompareTag(Tag.PlayerTileTriggerCollider) == false)
        return;
      if (!isEnable)
        return;

      isEnter = false;
      view.IdleParticle.Play();
      view.ActivateParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private async UniTask BlinkAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          var duration = 0.0f;
          var t = 0.0f;
          var minIntensity = 0.0f;
          var maxIntensity = 1.0f;
          var targetIntensity = 0.0f;
          while(t < 1.0f)
          {
            token.ThrowIfCancellationRequested();
            t = duration / model.data.GlowBlinkDuration;

            minIntensity = isEnter ? model.data.ActivateGlowIntensityMin 
                                   : model.data.IdleGlowIntensityMin;
            maxIntensity = isEnter ? model.data.ActivateGlowIntensityMax
                                   : model.data.IdleGlowIntensityMax;
           
            var blinkCount = isEnter ? model.data.ActivateBlinkCount
                                     : model.data.IdleBlinkCount;

            float wave = Mathf.PingPong(t * blinkCount, 1f);

            targetIntensity = view.EnableLight ? Mathf.Lerp(minIntensity, maxIntensity, wave)
                                               : 0.0f;

            matBlock.SetFloat(ShaderHash.Glow._Intensity, targetIntensity);
            view.SpriteRenderer.SetPropertyBlock(matBlock);

            duration += UnityEngine.Time.deltaTime;
            await UniTask.Yield();
          }
        }
      }
      catch (OperationCanceledException) { }
    }
  }
}