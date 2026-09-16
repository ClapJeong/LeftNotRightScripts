using LR.Table.Player;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.Player
{
  public class WallHitAnimationTimer
  {
    public bool IsWallHitTimerWorking => duration > 0.0f;

    private readonly PlayerCollisionData collisionData;
    private readonly UnityEvent onTimerComplete = new();
    private float duration;

    public WallHitAnimationTimer(PlayerCollisionData collisionData)
    {
      this.collisionData = collisionData;
    }

    public void OnFixedUpdate()
    {
      if(duration > 0.0f)
      {
        duration -= Time.fixedDeltaTime;
        if (duration <= 0.0f)
          onTimerComplete?.Invoke();
      }
    }

    public void Restart()
    {
      if (duration > 0.0f)
        onTimerComplete?.Invoke();
      duration = 0.0f;
    }

    public void OnWallHit()
    {
      duration = collisionData.WallBumpAnimationDuration;
    }

    public void SubscribeOnComplete(UnityAction unityAction)
      => onTimerComplete.AddListener(unityAction);

    public void UnsubscribeOnComplete(UnityAction unityAction)
      => onTimerComplete.RemoveListener(unityAction);

    public void UnsubscribeAll()
      => onTimerComplete.RemoveAllListeners();
  }
}
