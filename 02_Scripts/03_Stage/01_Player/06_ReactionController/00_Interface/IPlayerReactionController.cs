using LR.Table.TriggerTile;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.Player
{
  public interface IPlayerReactionController : IDisposable
  {
    public void Bounce(BounceData data, Vector3 direction);

    public void SetInputting(bool isInputting);

    public void Stun();

    public void Clear();

    public void DamageEnergy(float value, bool ignoreInvincible = false);

    public void OnWallBump(Collision2D collision2D);

    public void Teleport(Vector3 targetPosition);

    public void EnterConveyor(int id, Vector3 direction);

    public void ExitConveyor(int id);

    public void IgnoreReactionOnce();

    public void UpdateMoveDuration(float moveDuration);

    public void EnterElectric(int id);

    public void ExitElectric(int id);

    public void SubscribeOnWallContact(UnityAction unityAction);

    public void UnsubscribeOnWallContact(UnityAction unityAction);
  }
}