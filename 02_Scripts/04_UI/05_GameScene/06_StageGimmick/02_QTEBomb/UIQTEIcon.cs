using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.StageGimmick.QTE
{
  public class UIQTEIcon : MonoBehaviour
  {
    [SerializeField] private Image image;
    [field: SerializeField] public RectTransform RectTransfrom { get; private set; }

    public void UpdateSprite(Sprite sprite)
      => image.sprite = sprite;

    public void UpdateAlpha(float alpha)
      => image.SetAlpha(alpha);

    public void UpdateColor(Color color)
      => image.color = color;
  }
}
