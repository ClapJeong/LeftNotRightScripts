using System.Collections.Generic;
using UnityEngine;
using LR.Manager.Stage;

namespace LR.Stage.InteractiveObject.AutoMover
{
  public class StraightMoveController : BaseMoveController
  {
    private readonly Transform transform;
    private readonly List<Vector3> waypoints;
    private readonly AnimationCurve animationCurve;
    private float duration;

    private float straightTime;

    private int straightIndex = 0;
    private float segmentT = 0f;

    private bool straightFinished = false;
    private Vector3 from;
    private Vector3 to;

    public StraightMoveController(
      Transform transform, 
      List<Vector3> waypoints, 
      AnimationCurve animationCurve, 
      Vector3 initializedPosition,
      float duration)
    {
      this.transform = transform;
      this.waypoints = waypoints;
      this.animationCurve = animationCurve;
      this.duration = duration;

      from = initializedPosition;
      to = from + waypoints[straightIndex];
    }

    public override void UpdateDuration(float duration)
    {
      this.duration = duration;
    }

    public override void OnFixedUpdate(out Vector2 moveDirection)
    {
      if (straightFinished || waypoints.Count == 0)
      {
        moveDirection = Vector2.zero;
        return;
      }

      straightTime += Time.fixedDeltaTime;
      segmentT = Mathf.Clamp01(straightTime / duration);

      var prevPosition = transform.position;
      float evalT = animationCurve.Evaluate(segmentT);
      transform.position = Vector3.Lerp(from, to, evalT);

      moveDirection = (transform.position - prevPosition).normalized;

      if(segmentT >= 1.0f)
      {
        from = to;
        straightTime = 0f;
        segmentT = 0f;

        straightIndex++;
        if (straightIndex >= waypoints.Count)
          straightFinished = true;
      }
    }

    public override void Reset()
    {
      straightTime = 0.0f;
      straightIndex = 0;
      segmentT = 0.0f;
      straightFinished = false;
    }
  }
}
