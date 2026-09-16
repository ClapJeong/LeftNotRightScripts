

using UnityEngine;

namespace LR.Manager.Local.CameraService
{
  public interface ICameraValueService
  {
    public void SetSize(float size, bool isInitialize = false);

    public Vector2 GetScreenPosition(Vector3 worldPosition);

    public Vector2 WorldToViewportPoint(Vector3 worldPosition);

    public void SetEuler(Vector3 euler);

    public void UpdateOffset(Vector3 offset);

    public void SetPosition(Vector3 position);

    public Vector3 GetPosition();

    public float GetCurrentOrthographizSize();

    public float GetInitializedOrthographizSize();
  }
}
