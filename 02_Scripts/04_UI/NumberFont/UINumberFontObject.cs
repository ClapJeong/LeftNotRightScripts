using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINumberFontObject : MonoBehaviour
{
  [field: SerializeField] public RectTransform RectTransform { get; private set; }
  [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

  [SerializeField] private List<Sprite> sprites; // 0~9
  [SerializeField] private Image minus;
  [SerializeField] private Image hundred;
  [SerializeField] private Image ten;
  [SerializeField] private Image one;
  [SerializeField] private GameObject dotRoot;
  [SerializeField] private List<Image> dots;
  [SerializeField] private Image down1;
  [SerializeField] private Image down2;

  public void UpdateScale(float scale)
    => RectTransform.localScale = Vector3.one * scale;

  public void UpdateColor(Color color)
  {
    hundred.color = color;
    minus.color = color;
    ten.color = color;
    one.color = color;
    foreach(var dot in dots)
      dot.color = color;
    down1.color = color;
    down2.color = color;
  }

  public void UpdateText(float value)
  {
    minus.gameObject.SetActive(value < 0.0f);

    int rounded = Mathf.RoundToInt(Mathf.Abs(value) * 100f);

    int integer = rounded / 100;
    int tenth = (rounded / 10) % 10;
    int hundredth = rounded % 10;

    int hundDigit = (integer / 100) % 10;
    int tenDigit = (integer / 10) % 10;
    int oneDigit = integer % 10;

    hundred.gameObject.SetActive(hundDigit != 0);
    if (hundDigit != 0)
      hundred.sprite = sprites[hundDigit];

    // 10의 자리
    ten.gameObject.SetActive(tenDigit != 0);
    if (tenDigit != 0)
      ten.sprite = sprites[tenDigit];

    // 1의 자리(항상 표시)
    one.sprite = sprites[oneDigit];

    // 소수 둘째 자리
    bool showHundredth = hundredth != 0;
    down2.gameObject.SetActive(showHundredth);
    if (showHundredth)
      down2.sprite = sprites[hundredth];

    // 소수 첫째 자리 (뒤가 전부 0이면 숨김)
    bool showTenth = showHundredth || tenth != 0;
    down1.gameObject.SetActive(showTenth);
    dotRoot.gameObject.SetActive(showTenth);

    if (showTenth)
      down1.sprite = sprites[tenth];

    Canvas.ForceUpdateCanvases();
    LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
  }
}
