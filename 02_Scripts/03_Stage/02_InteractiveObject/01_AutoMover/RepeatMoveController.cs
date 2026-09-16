using System.Collections.Generic;
using UnityEngine;

namespace LR.Stage.InteractiveObject.AutoMover
{
  public class RepeatMoveController : BaseMoveController
  {
    private readonly Transform transform;
    private readonly List<Vector3> waypoints = new();
    private readonly AnimationCurve animationCurve;
    private float duration;

    private float repeatTime;

    public RepeatMoveController(
      Transform transform, 
      List<Vector3> waypoints, 
      AnimationCurve animationCurve,
      Vector3 initializedPosition,
      float duration)
    {
      this.transform = transform;
      this.animationCurve = animationCurve;
      this.duration = duration;

      this.waypoints.Add(initializedPosition);
      foreach (var waypoint in waypoints)
        this.waypoints.Add(transform.TransformPoint(waypoint));
    }

    public override void UpdateDuration(float duration)
    {
      this.duration = duration;
    }

    public override void OnFixedUpdate(out Vector2 moveDirection)
    {
      moveDirection = Vector2.zero;

      if (waypoints.Count == 0)
        return;

      repeatTime += Time.fixedDeltaTime;
      float t = repeatTime / duration;

      float pingPong = Mathf.PingPong(t, 1f);
      float curveT = animationCurve.Evaluate(pingPong);

      float totalLength = 0f;
      for (int i = 0; i < waypoints.Count - 1; i++)
        totalLength += Vector3.Distance(waypoints[i], waypoints[i + 1]);

      float remain = curveT * totalLength;

      for (int i = 0; i < waypoints.Count - 1; i++)
      {
        float segLen = Vector3.Distance(waypoints[i], waypoints[i + 1]);

        if (remain <= segLen)
        {
          var prevPosition = transform.position;
          float localT = remain / segLen;
          transform.position = Vector3.Lerp(waypoints[i], waypoints[i + 1], localT);

          moveDirection = (transform.position - prevPosition).normalized;
          break;
        }
        remain -= segLen;
      }
    }

    public override void Reset()
    {
      repeatTime = 0.0f;
    }
  }
}
