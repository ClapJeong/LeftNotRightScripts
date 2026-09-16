using UnityEngine;

namespace LR.Stage.InteractiveObject.AutoMover
{
  public class CircleLoopMoveController : BaseMoveController
  {
    private readonly Transform transform;
    private readonly AnimationCurve animationCurve;
    private readonly Vector2 initializePosition;
    private readonly float radius;
    private readonly Vector3 circleCenter;    
    private float duration;

    private readonly float beginRad;

    private float normalizedTime;
    private float circleAngleRad;

    public CircleLoopMoveController(
      Transform transform, 
      AnimationCurve animationCurve, 
      Vector2 initializePosition,
      float radius, 
      float duration, 
      Vector3 circleCenter, 
      float beginAngle)
    {
      this.transform = transform;
      this.animationCurve = animationCurve;
      this.initializePosition = initializePosition;
      this.radius = radius;
      this.duration = duration;
      this.circleCenter = circleCenter;

      beginRad = beginAngle * Mathf.Deg2Rad;
      normalizedTime = 0f;
      circleAngleRad = 0.0f;
    }

    public override void UpdateDuration(float duration)
    {
      this.duration = duration;
    }

    public override void OnFixedUpdate(out Vector2 moveDirection)
    {
      float prevT = normalizedTime;
      float prevCurve = animationCurve.Evaluate(prevT);

      normalizedTime += Time.fixedDeltaTime / duration;
      normalizedTime = Mathf.Repeat(normalizedTime, 1f);

      float currCurve = animationCurve.Evaluate(normalizedTime);
      float deltaCurve = currCurve - prevCurve;

      float angleDelta = deltaCurve * Mathf.PI * 2f;

      circleAngleRad -= angleDelta;

      float finalAngle = beginRad + circleAngleRad;

      Vector3 offset = new Vector3(
        Mathf.Cos(finalAngle),
        Mathf.Sin(finalAngle),
        0f
      ) * radius;

      var prevPosition = transform.position;
      transform.position = circleCenter + offset;

      moveDirection = (transform.position - prevPosition).normalized;
    }

    public override void Reset()
    {
      normalizedTime = 0f;
      circleAngleRad = 0f;
    }
  }
}
