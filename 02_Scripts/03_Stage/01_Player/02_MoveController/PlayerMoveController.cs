using LR.Manager.Stage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LR.Stage.Player
{
  public class PlayerMoveController : IPlayerMoveController
  {
    private readonly PlayerStatus playerStatus;
    private readonly PlayerModel model;
    private readonly IPlayerStateProvider stateProvider;
    private readonly Transform transform;
    private readonly Rigidbody2D rigidbody2D;

    private readonly SubscribeHandle inputActionSubscribeHandle;
    private readonly List<Vector3> inputDirections = new();
    private readonly LayerMask obstacleLayerMask;
    private float currentInputVelocityNormalized;
    private Vector3 directionModify;
    private Vector3 pausedVelocity;

    public PlayerMoveController(
      PlayerStatus playerStatus,
      Transform transform,
      Rigidbody2D rigidbody2D, 
      IPlayerInputActionSubscriber inputActionSubscriber, 
      PlayerModel model,
      IStageEventSubscriber stageEventSubscriber,
      IPlayerStateProvider stateProvider,
      LayerMask obstacleLayerMask)
    {
      this.playerStatus = playerStatus;
      this.transform = transform;
      this.rigidbody2D = rigidbody2D;
      this.model = model;
      this.stateProvider = stateProvider;
      this.obstacleLayerMask = obstacleLayerMask;

      inputActionSubscribeHandle = new(
        () =>
        {
          inputActionSubscriber.SubscribePerformed(OnPerformed);
          inputActionSubscriber.SubscribeCanceled(OnCanceled);
        },
        () =>
        {
          inputActionSubscriber.UnsubscribePerformed(OnPerformed);
          inputActionSubscriber.UnsubscribeCanceled(OnCanceled);
        });

      inputActionSubscribeHandle.Subscribe();

      stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Pause, OnPause);
      stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Resume, OnResume);
    }    

    public Vector2 GetCurrentPosition()
      => transform.position;

    public void SetLinearVelocity(Vector3 velocity)
    {
      if (stateProvider.GetCurrentState() == Enum.PlayerState.Exhausted)
        rigidbody2D.linearVelocity = Vector3.zero;
      else
        rigidbody2D.linearVelocity = velocity;
    }

    public List<Vector3> GetCurrentInputDirections()
      => inputDirections.ToList();

    public void SetInputDirections(List<Vector3> inputDirections)
    {
      this.inputDirections.Clear();
      this.inputDirections.AddRange(inputDirections);
    }

    public void ApplyMoveAcceleration()
    {
      var movement = model.modelSO.Movement;

      var electricModifier = playerStatus.IsElectric.Value
          ? movement.ElectricModifier
          : 1f;
      Vector3 currentVel = rigidbody2D.linearVelocity;
      Vector3 desiredVel =
        electricModifier * 
        model.modelSO.Movement.MaxSpeed * 
        GetInputVelocity().normalized;

      currentVel = Vector3.MoveTowards(
            currentVel,
            desiredVel,
            electricModifier *
            model.modelSO.Movement.Acceleration * 
            Time.fixedDeltaTime);

      SetLinearVelocity(currentVel);

      var finalVelocity = currentVel;

      if (directionModify != Vector3.zero && TryApplyConveyorModify(currentVel, out var modifyVelocity))
        finalVelocity += modifyVelocity;

      currentInputVelocityNormalized = Mathf.InverseLerp(
        0.0f,
        model.modelSO.Movement.MaxSpeed,
        finalVelocity.magnitude);
    }

    public void ApplyMoveDeceleration()
    {
      var movement = model.modelSO.Movement;

      var electricModifier = playerStatus.IsElectric.Value
          ? movement.ElectricModifier
          : 1f;
      Vector3 currentVel = rigidbody2D.linearVelocity;

      currentVel = Vector3.MoveTowards(
      currentVel,
      Vector3.zero,
      electricModifier *
      model.modelSO.Movement.Decceleration *
      Time.fixedDeltaTime);      

      SetLinearVelocity(currentVel);

      var finalVelocity = currentVel;

      if (directionModify != Vector3.zero && TryApplyConveyorModify(currentVel, out var modifyVelocity))
        finalVelocity += modifyVelocity;

      currentInputVelocityNormalized = Mathf.InverseLerp(
        0.0f,
        model.modelSO.Movement.MaxSpeed,
        finalVelocity.magnitude);
    }

    private bool TryApplyConveyorModify(Vector3 velocity, out Vector3 modifyVelocity)
    {
      modifyVelocity = Vector3.zero;
      var origin = transform.position;
      var direction = directionModify.normalized;
      var length = 0.5f;// (directionModify.magnitude + Mathf.Max(1.0f, velocity.magnitude)) * Time.fixedDeltaTime;
      var hit = Physics2D.Raycast(origin, direction, length, obstacleLayerMask);
      var isConveyable = hit.collider == null || !hit.collider.isTrigger || !hit.collider.enabled;
      if (isConveyable)
      {
        modifyVelocity = directionModify * Time.fixedDeltaTime;
        transform.position += modifyVelocity;
      }

      return isConveyable;
    }

    public float GetCurrentInputVelocityNormalized()
      => currentInputVelocityNormalized;

    public void ResetAllDirection()
      => inputDirections.Clear();

    public Vector2 GetCurrentDirection()
      => GetInputVelocity();

    public void DecreaseVelocity(float applyValue)
    {
      var currentVel = rigidbody2D.linearVelocity;
      rigidbody2D.linearVelocity = currentVel * applyValue;
    }

    public bool IsConveyorCrossing()
    {
      var currentDirection = rigidbody2D.linearVelocity.normalized;
      if(currentDirection != Vector2.zero)
      {
        var conveyorDirection = directionModify.normalized;
        if(conveyorDirection.x != 0.0f)
        {
          var isCrossing = conveyorDirection.x > 0.0f && currentDirection.x < 0.0f ||
                           conveyorDirection.x < 0.0f && currentDirection.x > 0.0f;
          return isCrossing;
        }
        else if(conveyorDirection.y != 0.0f)
        {
          var isCrossing = conveyorDirection.y > 0.0f && currentDirection.y < 0.0f ||
                           conveyorDirection.y < 0.0f && currentDirection.y > 0.0f;
          return isCrossing;
        }
      }

      return false;
    }

    public void Dispose()
    {
      inputActionSubscribeHandle.Dispose();
    }

    private void OnPerformed(Direction direction)
    {
      var velocity = model.ParseDirection(direction);
      if(!inputDirections.Contains(velocity))
        inputDirections.Add(velocity);
    }

    private void OnCanceled(Direction direction)
    {
      var velocity = model.ParseDirection(direction);
      if(inputDirections.Contains(velocity))
        inputDirections.Remove(velocity);
    }

    public void MovePosition(Vector3 position)
    {
      transform.position = position;
    }

    public void SetModify(Vector3 modify)
      => directionModify = modify;

    public Vector2 GetCurrentVelocity()
      => rigidbody2D.linearVelocity;

    private void OnPause()
    {
      pausedVelocity = rigidbody2D.linearVelocity;
      SetLinearVelocity(Vector3.zero);
    }

    private void OnResume()
    {
      SetLinearVelocity(pausedVelocity);
      pausedVelocity = Vector3.zero;
    }

    private Vector3 GetInputVelocity()
    {
      Vector3 sum = Vector3.zero;

      for (int i = 0; i < inputDirections.Count; i++)
        sum += inputDirections[i];

      return sum;
    }
  }
}