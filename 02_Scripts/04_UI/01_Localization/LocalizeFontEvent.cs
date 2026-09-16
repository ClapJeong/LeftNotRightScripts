using TMPro;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizeFontEvent : MonoBehaviour
{
  private TextMeshProUGUI tmp;
  private TextMeshProUGUI TMP
  {
    get
    {
      if(tmp == null)
        tmp = GetComponent<TextMeshProUGUI>();
      return tmp;
    }
  }


  private void Start()
    => GlobalManager.instance.LocaleService.Register(this);

  private void OnDestroy()
    => GlobalManager.instance.LocaleService.Unregister(this);

  public void UpdateFont(TMP_FontAsset fontAsset)
  {
    if (TMP.font == fontAsset)
      return;

    TMP.font = fontAsset;
    TMP.ForceMeshUpdate();
  }
}
