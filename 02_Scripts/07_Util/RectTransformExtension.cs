using UnityEngine;

public static class RectTransformExtension
{
  public static Vector3 GetCenterPosition(this RectTransform rt)
  {
    rt.GetWorldCorners(_corners);
    return (_corners[0] + _corners[2]) * 0.5f;
  }

  static readonly Vector3[] _corners = new Vector3[4];

  public static void SetScale(this RectTransform rectTransform, float scale)
  {
    rectTransform.localScale = Vector3.one * scale;
  }

  public static void SetSize(this RectTransform rectTransform, Vector2 size)
  {
    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
  }
}
