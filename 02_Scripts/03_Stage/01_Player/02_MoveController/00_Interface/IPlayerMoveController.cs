using System;
using System.Collections.Generic;
using UnityEngine;

namespace LR.Stage.Player
{
  public interface IPlayerMoveController : IDisposable
  {
    public void SetLinearVelocity(Vector3 velocity);

    public void ApplyMoveAcceleration();

    public void ApplyMoveDeceleration();

    public Vector2 GetCurrentDirection();

    public Vector2 GetCurrentVelocity();

    public void DecreaseVelocity(float multiplyValue);

    public float GetCurrentInputVelocityNormalized();

    public void MovePosition(Vector3 position);

    public void SetModify(Vector3 modify);

    public List<Vector3> GetCurrentInputDirections();

    public void SetInputDirections(List<Vector3> inputDirections);

    public void ResetAllDirection();

    public Vector2 GetCurrentPosition();

    public bool IsConveyorCrossing();
  }
}