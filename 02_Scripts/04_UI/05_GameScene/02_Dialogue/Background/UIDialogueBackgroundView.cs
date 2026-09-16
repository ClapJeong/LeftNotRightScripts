using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.GameScene.Dialogue.Shake;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.Dialogue
{
  public class UIDialogueBackgroundView : BaseUIView
  {
    [field: SerializeField] public RectTransform TopLetterBoxRectTransform { get; private set; }
    [field: SerializeField] public RectTransform BottomLetterBoxRectTransform { get; private set; }
    [field: SerializeField] public CanvasGroup LobbyBackgroundCavasGroup { get; private set; }
    [field: Header("[ Shadow ]")]
    [field: SerializeField] public Image DoctorShadow { get; private set; }
    [field: SerializeField] public Image LRShadow { get; private set; }
    [field: Header("[ Shake Fall ]")]
    [field: SerializeField] public Transform ShakeFallRoot { get; private set; }
    [field: SerializeField] public BackgroundShakeFallObject ShakeFallObjectPrefab { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Hiding;
      var letterBoxDuration = isImmediately ? 0.0f : UISO.Dialogue.LetterBoxDuration;
      var letterBoxSize = new Vector2(-Screen.width, TopLetterBoxRectTransform.rect.height);
      var backgroundFadeDuration = isImmediately ? 0.0f : UISO.Dialogue.BackgroundFadeDuration;
      var backgroundAlpha = 0.0f;      
      try
      {
        await DOTween
          .Sequence()
          .Join(TopLetterBoxRectTransform.DOSizeDelta(letterBoxSize, letterBoxDuration))
          .Join(BottomLetterBoxRectTransform.DOSizeDelta(letterBoxSize, letterBoxDuration))          
          .Join(LobbyBackgroundCavasGroup.DOFade(backgroundAlpha, backgroundFadeDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);
        visibleState = Enum.VisibleState.Hidden;
      }
      catch(OperationCanceledException) { }
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      visibleState = Enum.VisibleState.Showing;
      var letterBoxDuration = isImmediately ? 0.0f : UISO.Dialogue.LetterBoxDuration;
      var letterBoxSize = new Vector2(0.0f, TopLetterBoxRectTransform.rect.height);
      var backgroundFadeDuration = isImmediately ? 0.0f : UISO.Dialogue.BackgroundFadeDuration;
      var backgroundAlpha = 1.0f;
      var delay = isImmediately ? 0.0f : UISO.Dialogue.BackgroundFadeDelay;
      try
      {
        await UniTask.WhenAll(
          DOTween
          .Sequence()
          .Join(TopLetterBoxRectTransform.DOSizeDelta(letterBoxSize, letterBoxDuration).SetEase(Ease.OutCirc))
          .Join(BottomLetterBoxRectTransform.DOSizeDelta(letterBoxSize, letterBoxDuration).SetEase(Ease.OutCirc))
          .ToUniTask(TweenCancelBehaviour.Complete, token),
          DOTween
          .Sequence()
          .AppendInterval(delay)
          .Append(LobbyBackgroundCavasGroup.DOFade(backgroundAlpha, backgroundFadeDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token));

        await UniTask.WaitForSeconds(isImmediately ? 0.0f : UISO.Dialogue.BackgroundShowDelay);

        visibleState = Enum.VisibleState.Showen;
      }
      catch (OperationCanceledException) { }
    }
  }
}
