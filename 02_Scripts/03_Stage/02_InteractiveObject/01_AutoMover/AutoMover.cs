using System.Collections.Generic;
using UnityEngine;
using LR.Manager.Stage;
using LR.Manager.Sound;

namespace LR.Stage.InteractiveObject.AutoMover
{
  public class AutoMover : BaseInteractiveObject
  {
    public enum Type
    {
      Straight,
      Repeat,
      CircleLoop,
    }

    public Type type;
    [SerializeField] private ParticleSystem moveParticle;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0.0f,0.0f,1.0f,1.0f);
    [SerializeField] private bool startOnAwake;
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private List<Vector3> waypoints = new();
    [SerializeField] private float radius = 1.0f;
    [SerializeField] private float angle = 0.0f;

    private IStageStateProvider stageStateProvider;
    private BaseMoveController moveController;
    private bool isMove;

    private Vector3 circleCenter;
    private Vector3 initializePosition;

    private void OnValidate()
    {
      if (duration <= 0.0f)
        duration = 0.1f;

      moveController?.UpdateDuration(duration);
    }

    public override void Initialize(StageManager stageManager, ISFXController sfxController)
    {
      this.stageStateProvider = stageManager;
      initializePosition = transform.position;
      circleCenter = transform.TransformPoint(new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle)) * radius);

      moveController = type switch
      {
        Type.Straight => new StraightMoveController(transform, waypoints, animationCurve, initializePosition, duration),
        Type.Repeat => new RepeatMoveController(transform, waypoints, animationCurve, initializePosition, duration),
        Type.CircleLoop => new CircleLoopMoveController(transform, animationCurve, initializePosition, radius, duration, circleCenter, angle - 180.0f),
        _ => throw new System.NotImplementedException(),
      };

      if (startOnAwake)
        ActivateMove();
    }

    private void FixedUpdate()
    {
      if (!isMove || !stageStateProvider.IsPlayingState)
      {
        moveParticle?.Stop();
        return;
      }

      if (moveController != null)
      {
        if (moveParticle.isStopped)
          moveParticle?.Play();

        moveController.OnFixedUpdate(out var moveDirection);
        if (moveParticle != null)
        {
          var angle = Mathf.Atan2(moveDirection.y, moveDirection.x);
          var shapeModule = moveParticle.shape;
          shapeModule.rotation = new Vector3(angle * Mathf.Rad2Deg - transform.eulerAngles.z, 270, 0.0f);
        }        
      }      
    }

    public void ActivateMove()
    {
      isMove = true;
    }

    public void DeactivateMove()
    {
      isMove = false;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
      base.OnDrawGizmos();

      switch (type)
      {
        case Type.Straight:
          {
            DrawPositionsLine(false);
          }
          break;

        case Type.Repeat:
          {
            DrawPositionsLine(false);
          }
          break;

        case Type.CircleLoop:
          {
            var currentCircleCenter = transform.TransformPoint(new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle)) * radius);
            DrawCircleLines(currentCircleCenter, radius);
          }
          break;
      }
    }
#endif
    private void DrawPositionsLine(bool isLoop)
    {
      Gizmos.color = Color.red;
      var positionList = new List<Vector3>() { transform.position };
      foreach (var position in waypoints)
        positionList.Add(transform.TransformPoint(position));
      Gizmos.DrawLineStrip(new System.ReadOnlySpan<Vector3>(positionList.ToArray()), isLoop);
    }

    private void DrawCircleLines(Vector3 center, float radius, int segments = 32)
    {
      Gizmos.color = Color.red;
      Vector3 prevPoint = center + Vector3.right * radius;

      for (int i = 1; i <= segments; i++)
      {
        float angle = i * Mathf.PI * 2f / segments;
        Vector3 newPoint = center + new Vector3(
            Mathf.Cos(angle),
            Mathf.Sin(angle),
            0f            
        ) * radius;

        Gizmos.DrawLine(prevPoint, newPoint);
        prevPoint = newPoint;
      }
    }

    public override void Enable(bool enable)
    {
      if (enable && startOnAwake)
        ActivateMove();
      else if (!enable)
        DeactivateMove();
    }

    public override void Restart()
    {
      Enable(false);
      moveController?.Reset();
      transform.position = initializePosition;
      Enable(true);
    }
  }
}
