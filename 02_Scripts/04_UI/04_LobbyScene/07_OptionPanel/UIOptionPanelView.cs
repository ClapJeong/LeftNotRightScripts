using LR.UI.DifficultySettting;
using LR.UI.Lobby.Option;
using LR.UI.VolumeControl;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace LR.UI.Lobby
{
  public class UIOptionPanelView : UIBaseLobbyPanelView
  {
    [field: SerializeField] public UIVolumeControlView VolumeControlView { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public UISubmitDirectionSet ExitSubmitDirectionSet { get; private set; }
    [field: SerializeField] public UIDifficultyView DifficultyView { get; private set; }
    [field: Space(5)]
    [field: SerializeField] public UISubmitDirectionSet DataResetButton { get; private set; }
    [field: SerializeField] public UIResetWarningView ResetWarningView { get; private set; }

    [field: Space(5)]
    [field: SerializeField] public UISubmitDirectionSet AllAchievementButtonSet { get; private set; }
    [field: SerializeField] public CanvasGroup AllAchievementGuideCanvasGroup { get; private set; }
    [field: SerializeField] public LocalizeStringEvent AllAchievementText { get; private set; }
  }
}
