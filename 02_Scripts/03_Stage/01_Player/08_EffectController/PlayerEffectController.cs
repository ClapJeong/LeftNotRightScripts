using LR.Stage.Player.Enum;
using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace LR.Stage.Player
{
  public class PlayerEffectController : IPlayerEffectController
  {
    private readonly PlayerParticleSet particleSet;

    public PlayerEffectController(PlayerParticleSet particleSet)
    {
      this.particleSet = particleSet;
    }

    public void PlayEffect(PlayerEffect effect)
      => particleSet.GetParticle(effect).Play();    

    public void StopEffect(PlayerEffect effect)
      => particleSet.GetParticle(effect).Stop();

    public void UpdateMoveDirection(Vector2 direction)
    {
      particleSet
        .GetParticle(PlayerEffect.Move)
        .transform
        .rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Mathf.Sign(direction.x) * 180.0f));
      particleSet
        .GetParticle(PlayerEffect.Run)
        .transform
        .rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Mathf.Sign(direction.x) * 180.0f));
    }

    public void StopAllEffects(params PlayerEffect[] except)
    {
      foreach (PlayerEffect effectType in System.Enum.GetValues(typeof(PlayerEffect)))
      {
        if (Array.IndexOf(except, effectType) != -1)
          continue;

        StopEffect(effectType);
      }        
    }
  }
}
