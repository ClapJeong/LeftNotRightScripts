using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.Lobby
{
  public class UIChapterPanelView : UIBaseLobbyPanelView
  {
    [field: SerializeField] public CanvasGroup PreviewCanvasGroup { get; private set; }
    [field: SerializeField] public RectTransform PreviewRectTransform { get; private set; }
    [field: SerializeField] public RawImage PreviewRawImage { get; private set; }

    [field: SerializeField] public UIChapterPanelExitButtonView ExitView { get; private set; }
    [field: SerializeField] public RectTransform StageButtonSetCenterPosition {  get; private set; }
    [field: SerializeField] public HorizontalLayoutGroup StageButtonSetVertialLayoutGroup { get; private set; }

    [field: SerializeField] public Animator GimmickPreviewAnimator { get; private set; }

    [field: Header("[ Root ]")]
    [field: SerializeField] public RectTransform ChapterButtonSetRoot { get; private set; }    
  }
}