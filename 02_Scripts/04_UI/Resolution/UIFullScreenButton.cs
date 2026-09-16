using UnityEngine;
using UnityEngine.Localization.Components;

namespace LR.UI.Resolution
{
  public class UIFullScreenButton : MonoBehaviour
  {
    [field: SerializeField] public LocalizeStringEvent LocalizeStringEvent { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet SubmitDirectionSet { get; private set; }

    private readonly string FullScreenKey = "ui_FullScreen";
    private readonly string BorderLessScreenKey = "ui_BorderLess";

    public void Start()
    {
      UpdateText();
      SubmitDirectionSet.Subscribe(OnSubmit);
    }

    private void OnSubmit()
    {
      FlipData();

      var isFullScreen = IsFullScreen();

      var mode = isFullScreen ? FullScreenMode.ExclusiveFullScreen
                              : FullScreenMode.FullScreenWindow;
      var width = Screen.currentResolution.width;
      var height = Screen.currentResolution.height;

      Screen.SetResolution(width, height, mode);

      PlayerPrefs.Save();

      UpdateText();
    }

    private void UpdateText()
    {
      var isFullScreen = IsFullScreen();
      var key = isFullScreen ? FullScreenKey : BorderLessScreenKey;
      LocalizeStringEvent.SetEntry(key);
    }

    private bool IsFullScreen()
      => PlayerPrefs.GetInt(PlayerPrefsName.FullScreen) == 0;

    private void FlipData()
    {
      var origin = PlayerPrefs.GetInt(PlayerPrefsName.FullScreen, 0);
      PlayerPrefs.SetInt(PlayerPrefsName.FullScreen, Mathf.Abs(origin - 1));
    }      
  }
}
