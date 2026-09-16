using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace LR.UI.Lobby.DialogueReplayPanel
{
  public class UIDialogueReplayButtonSet : BaseUIView
  {
    [field: SerializeField] public RectTransform ContentContent { get; private set; }
    [field: SerializeField] public UISubmitDirectionSet SubmitDirectionSet { get; private set; }
    [field: SerializeField] public LocalizeStringEvent LocalizeStringEvent { get; private set; }
    [field: SerializeField] public Selectable Selectable { get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public Image ScrollImage { get; private set; }

    private CTSContainer cts = new();

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      cts.Cancel();
      ContentContent.anchoredPosition = -Vector2.right * UISO.Lobby.DialogueReplayHideLength;
      await UniTask.CompletedTask;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      cts.Cancel();
      cts.Create();
      var moveToken = cts.token;

      await ContentContent
        .DOAnchorPos(Vector2.zero, UISO.Lobby.DialogueReplayShowDuration)
        .ToUniTask(TweenCancelBehaviour.Kill, moveToken);
    }

    public void DeactivateScroll()
    {
      var newMat = Instantiate(ScrollImage.material);
      newMat.SetFloat(ShaderHash.DialogueReplayButton._ScrollSpeed, 0.0f);
      ScrollImage.material = newMat;
    }

    private void OnDestroy()
    {
      cts.Dispose();
    }
  }
}
