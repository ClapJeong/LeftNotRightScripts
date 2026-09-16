using TMPro;
using UnityEngine;

namespace LR.UI.Lobby
{
  public class UISpeedRunPanelView : UIBaseLobbyPanelView
  {
    [System.Serializable]
    public class DataTMPSet
    {
      [field: SerializeField] public TextMeshProUGUI Time { get; private set; }
      [field: SerializeField] public TextMeshProUGUI Restart { get; private set; }
    }
    [field: SerializeField] public GameObject EnableRoot { get; private set; }
    [field: SerializeField] public DataTMPSet FastestDataTMPSet { get; private set; }
    [field: SerializeField] public DataTMPSet SafestDataTMPSet {  get; private set; }    
    [field: SerializeField] public GameObject DisableRoot {  get; private set; }
    [field: SerializeField] public UISubmitDirectionSet PlayDirectionSet {  get; private set; }
    [field: SerializeField] public UISubmitDirectionSet ResumedirectionSet { get; private set; }
    [field: SerializeField] public CanvasGroup PlayButtonCanvasGroup {  get; private set; }
    [field: SerializeField] public UISubmitDirectionSet ExitDirectionSet {  get; private set; }
    [field: SerializeField] public GameObject ResumePreviewPanel { get; private set; }
    [field: SerializeField] public TextMeshProUGUI ResumePreviewText { get; private set; }
    [field: SerializeField] public UISpeedrunLeaderboardView FastestLeaderboardView { get; private set; }
    [field: SerializeField] public UISpeedrunLeaderboardView SafestLeaderboardView { get; private set; }
  }
}