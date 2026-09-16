using UnityEngine.UI;

public static class ImageExtension
{
  public static void SetAlpha(this Image image, float alpha)
  {
    var color = image.color;
    color.a = alpha;
    image.color = color;
  }

  public static void SetFillAmount(this Image image, float value)
  {
    image.fillAmount = value;
  }
}