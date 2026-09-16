using LR.UI.Lobby.DialogueReplayPanel;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.Lobby
{
  public class UIDialogueReplayPanelView : UIBaseLobbyPanelView
  {
    [field: SerializeField] public UIDialogueReplayButtonSet ButtonSetPrefab { get; private set; } 
    [field: SerializeField] public RectTransform ButtonRoot { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet ExitSubmitDirectionSet { get; private set; }
    [field: SerializeField] public Selectable ExitSelectable {  get; private set; }
    [field: SerializeField] public GridLayoutGroup GridLayoutGroup {  get; private set; }
  }
}
