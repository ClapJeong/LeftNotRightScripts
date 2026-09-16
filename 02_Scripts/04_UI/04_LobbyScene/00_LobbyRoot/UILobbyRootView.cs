using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using LR.UI.Enum;
using UnityEngine.UI;
using LR.Manager.Store;

namespace LR.UI.Lobby
{
  public class UILobbyRootView : BaseUIView
  {
    [field: SerializeField] public RectTransform JustImageRectTrasnform { get; private set; }
    [field: SerializeField] public Image BackgroundImage { get; private set; }
    [field: SerializeField] public UIMainPanelView MainPanelView { get; private set; }
    [field: SerializeField] public UIChapterPanelView ChapterPanelView {  get; private set; }
    [field: SerializeField] public UIDialogueReplayPanelView DialogueReplayView {  get; private set; }
    [field: SerializeField] public UISpeedRunPanelView SpeedRunPanelView { get; private set; }
    [field: SerializeField] public UIOptionPanelView OptionPanelView { get; private set; }
    [field: SerializeField] public UILocalizePanelView LocalizePanelView { get; private set; }
    [field: SerializeField] public UILobbyInputGuideView InputGuideView { get; private set; }
    [field: SerializeField] public UILobbyLogoView LogoView { get; private set; }
    [field: SerializeField] public Transform IndicatorRoot { get; private set; }

    [field: Header("[ Store ]")]
    [field: SerializeField] public string SteamLink { get; private set; }
    [field: SerializeField] public string StoveLink { get; private set; }
    [field: SerializeField] public GameObject steamIcon { get; private set; }
    [field: SerializeField] public GameObject stoveIcon { get; private set; }

    public string GetDemoLink(StoreType storeType)
      => storeType switch
      {
        StoreType.None => string.Empty,
        StoreType.Steam => SteamLink,
        StoreType.Stove => StoveLink,
        _ => throw new System.NotImplementedException(),
      };


    public override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Hidden;
      gameObject.SetActive(false);
      return UniTask.CompletedTask;
    }

    public override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = VisibleState.Showen;
      gameObject.SetActive(true);
      return UniTask.CompletedTask;
    }
  }
}