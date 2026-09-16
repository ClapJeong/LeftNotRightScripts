using LR.UI.LocaleSet;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.Lobby
{
  public class UILocalizePanelView : UIBaseLobbyPanelView
  {
    [field: SerializeField] public UILocaleButtonsView LocaleButtonsView { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public RectTransform ExitRectTransform { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet ExitSubmitDirectionSet { get; private set; }
    [field: SerializeField] public Selectable ExitSelectable { get; private set; }
  }
}
