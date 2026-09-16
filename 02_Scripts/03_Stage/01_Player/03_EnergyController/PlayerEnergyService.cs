using Cysharp.Threading.Tasks;
using LR.Stage.Player.Enum;
using LR.Table.Player;
using System;
using UnityEngine;
using UnityEngine.Events;
using static LR.Stage.Player.IPlayerEnergySubscriber;

namespace LR.Stage.Player
{
  public class PlayerEnergyService : 
    IPlayerEnergyController, 
    IPlayerEnergySubscriber,
    IPlayerEnergyProvider, 
    IDisposable
  {
    private readonly BothPlayerEnergyContainer energyContainer;
    private readonly PlayerEnergyDataSO playerEnergyData;
    private readonly SpriteRenderer spriteRenderer;

    private readonly CTSContainer invincibleCTS = new();

    private bool isInvincible = false;

    #region IPlayerEnergyProvider
    public float StageMaxEnergy => energyContainer.StageMaxEnergy;
    public bool IsDead => energyContainer.IsDead;

    public bool IsFull => energyContainer.IsFull;

    public float TotalEnergy => energyContainer.TotalEnergy;

    public float LeftEnergy => energyContainer.LeftEnergy;

    public float RightEnergy => energyContainer.RightEnergy;

    public float TotalNormalized => energyContainer.TotalNormalized;

    public float LeftEnergyNormalized => energyContainer.LeftEnergyNormalized;

    public float RightEnergyNormalized => energyContainer.RightEnergyNormalized;
    #endregion

    public PlayerEnergyService(
      BothPlayerEnergyContainer energyContainer,
      PlayerEnergyDataSO playerEnergyData, 
      SpriteRenderer spriteRenderer)
    {
      this.energyContainer = energyContainer;
      this.playerEnergyData = playerEnergyData;
      this.spriteRenderer = spriteRenderer;
    }

    #region IPlayerEnergyController
    public void Damage(PlayerType hitPlayer, float value, DamageType damageType, bool ignoreInvincible = false)
    {
      if(ignoreInvincible || !isInvincible)
      {
        energyContainer.Damage(hitPlayer, value, damageType, ignoreInvincible);
        if (damageType != DamageType.WallBump)
          PlayInvincibleAsync().Forget();
      }
    }
    #endregion

    #region IPlayerEnergySubscriber
    public void SubscribeStateEvent(IPlayerEnergySubscriber.StateEvent type, UnityAction action)
      => energyContainer.SubscribeStateEvent(type, action);

    public void UnsubscribeStateEvent(IPlayerEnergySubscriber.StateEvent type, UnityAction action)
      => energyContainer.UnsubscribeStateEvent(type, action);

    public void SubscribeThreshhold(Threshhold type, float normalizedValue, UnityAction action)
      => energyContainer.SubscribeThreshhold(type, normalizedValue, action);    

    public void UnsubscribeThreshhold(Threshhold type, float normalizedValue, UnityAction action)
      => energyContainer.UnsubscribeThreshhold(type, normalizedValue, action);

    public void SubscribeValueEvent(ValueEvent valueEvent, UnityAction<float> action)
      => energyContainer.SubscribeValueEvent(valueEvent, action);

    public void UnsubscribeValueEvent(ValueEvent valueEvent, UnityAction<float> action)
      => energyContainer.UnsubscribeValueEvent(valueEvent, action);

    public void SubscribeOnHit(UnityAction<PlayerType, DamageType> onHit)
      => energyContainer.SubscribeOnHit(onHit);

    public void UnsubscribeOnHit(UnityAction<PlayerType, DamageType> onHit)
      => energyContainer.UnsubscribeOnHit(onHit);

    public void SubscribeOnDamageValue(UnityAction<PlayerType, float> onDamaged)
      => energyContainer.SubscribeOnDamageValue(onDamaged);

    public void UnsbscribeOnDamageValue(UnityAction<PlayerType, float> onDamaged)
      => energyContainer.UnsbscribeOnDamageValue(onDamaged);
    #endregion

    public void Restart()
    {
      invincibleCTS.Cancel();
    }

    public void Dispose()
    {
      invincibleCTS.Cancel();
      invincibleCTS.Dispose();
    }

    private async UniTask PlayInvincibleAsync()
    {
      isInvincible = true;
      invincibleCTS.Cancel();
      invincibleCTS.Create();
      var token = invincibleCTS.token;
      try
      {
        var durataion = 0.00f;
        var targetDuration = playerEnergyData.InvincibleDuration;
        var interval = playerEnergyData.InvincibleBlinkInterval;
        while (durataion< targetDuration)
        {
          spriteRenderer.SetAlpha(playerEnergyData.InvincibleBlinkAlphaMax);
          await UniTask.WaitForSeconds(interval, false, PlayerLoopTiming.Update, token);
          durataion += interval;
          token.ThrowIfCancellationRequested();

          spriteRenderer.SetAlpha(playerEnergyData.InvincibleBlinkAlphaMin);
          await UniTask.WaitForSeconds(interval, false, PlayerLoopTiming.Update, token);
          token.ThrowIfCancellationRequested();
          durataion += interval;
        }                
      }
      catch (OperationCanceledException) { }
      finally
      {
        if(this != null && spriteRenderer != null)
        {
          isInvincible = false;
          spriteRenderer.SetAlpha(1.0f);
        }
      }
    }    
  }
}