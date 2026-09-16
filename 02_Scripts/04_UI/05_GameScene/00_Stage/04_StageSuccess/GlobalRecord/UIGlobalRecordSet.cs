using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.GlobalRecord
{
  public class UIGlobalRecordSet : MonoBehaviour
  {
    [field: SerializeField] public RectTransform RectTransform { get; private set; }
    [field: SerializeField] public TextMeshProUGUI Record { get; private set; }
    [field: SerializeField] public TextMeshProUGUI NickName { get; private set; }
    [field: SerializeField] public Image Portrait { get; private set; }
    [field: SerializeField] public UINumberFontObject FontObject { get; private set; }

    public void UpdateColor(Color color)
    {
      Record.color = color;
      NickName.color = color;
      FontObject.UpdateColor(color);
    }
  }
}