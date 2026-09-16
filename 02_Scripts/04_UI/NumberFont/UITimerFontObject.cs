using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI
{
  public class UITimerFontObject : MonoBehaviour
  {
    [field: SerializeField] public RectTransform RectTransform { get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

    [SerializeField] private List<Sprite> sprites; // 0~9
    [SerializeField] private Image minuteTen;
    [SerializeField] private Image minuteOne;
    [SerializeField] private Image secondTen;
    [SerializeField] private Image secondOne;
    [SerializeField] private Image centiTen;
    [SerializeField] private Image centiOne;
    [SerializeField] private List<Image> dots;

    public void UpdateScale(float scale)
      => RectTransform.localScale = Vector3.one * scale;

    public void UpdateColor(Color color)
    {
      minuteTen.color = color;
      minuteOne.color = color;
      secondTen.color = color;
      secondOne.color = color;
      centiTen.color = color;
      centiOne.color = color;

      foreach (var dot in dots)
        dot.color = color;
    }

    public void UpdateText(float value)
    {
      value = Mathf.Clamp(value, 0f, 5999.99f); // 99:59:99까지

      int totalCentiseconds = Mathf.RoundToInt(value * 100f);

      int minutes = totalCentiseconds / 6000;
      int seconds = (totalCentiseconds / 100) % 60;
      int centis = totalCentiseconds % 100;

      minuteTen.sprite = sprites[minutes / 10];
      minuteOne.sprite = sprites[minutes % 10];

      secondTen.sprite = sprites[seconds / 10];
      secondOne.sprite = sprites[seconds % 10];

      centiTen.sprite = sprites[centis / 10];
      centiOne.sprite = sprites[centis % 10];

      Canvas.ForceUpdateCanvases();
      LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    }
  }
}