using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Table.Dialogue;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LR.UI.GameScene.Dialogue.Character
{
  public class PortraitController
  {
    private readonly CharacterPositionType positionType;
    private readonly UIPortraitData portraitData;
    private readonly Animator portraitAnimator;
    private readonly UITalkingCharacterView.ImageSet setA;
    private readonly UITalkingCharacterView.ImageSet setB;
    private readonly Animator emotionAnimator;

    private readonly CTSContainer portriatImageCTS = new();
    private readonly List<DialogueDataEnum.Portrait.AnimationType> ignoreSameAnimations = new()
    {
      DialogueDataEnum.Portrait.AnimationType.None,
      DialogueDataEnum.Portrait.AnimationType.Shaking,
      DialogueDataEnum.Portrait.AnimationType.Jumping,
      DialogueDataEnum.Portrait.AnimationType.Shaking,
      DialogueDataEnum.Portrait.AnimationType.Run,
    };

    private Sprite transparent;

    private bool useImageA;
    private bool isImageChanging = false;
    private UITalkingCharacterView.ImageSet forwardSet;
    private float forwardAlpha = 1.0f;
    private DialogueDataEnum.Portrait.AnimationType currentAnimationType = DialogueDataEnum.Portrait.AnimationType.None;
    private DialogueDataEnum.Portrait.Emotion currentEmotion;

    public PortraitController(
      CharacterPositionType positionType,
      UIPortraitData portraitData, 
      Animator portraitAnimator,
      UITalkingCharacterView.ImageSet setA,
      UITalkingCharacterView.ImageSet setB,
      Animator emotionAnimator)
    {
      this.positionType = positionType;
      this.portraitData = portraitData;
      this.portraitAnimator = portraitAnimator;
      this.setA = setA;
      this.setB = setB;
      this.emotionAnimator = emotionAnimator;
    }

    public void SetTransparent(Sprite transparent)
      => this.transparent = transparent;

    public void Clear()
    {
      PlayAnimation(0);
      setA.Image.sprite = transparent;
      setA.CanvasGroup.alpha = 0.0f;
      if(setA.HatView != null)
        setA.HatView.CanvasGroup.alpha = 0.0f;
      setB.Image.sprite = transparent;
      setB.CanvasGroup.alpha = 0.0f;
      if (setB.HatView != null)
        setB.HatView.CanvasGroup.alpha = 0.0f;
    }

    public void SetImage(
      Sprite sprite,
      bool isTransparent,
      DialogueDataEnum.Portrait.ChangeType changeType, 
      bool isFirstTalk = false)
    {
      portriatImageCTS.Cancel();
      portriatImageCTS.Create();
      SetImageAsync(sprite, 
        isFirstTalk ? DialogueDataEnum.Portrait.ChangeType.Fade : changeType, 
        portriatImageCTS.token,
        isTransparent).Forget();
    }

    public void PlayEmotion(DialogueDataEnum.Portrait.Emotion emotion)
    {
      if (currentEmotion == emotion)
        return;

      currentEmotion = emotion;
      emotionAnimator.Play(emotion.ToString());
    }

    public void PlayAnimation(DialogueDataEnum.Portrait.AnimationType animType)
    {
      if (ignoreSameAnimations.Contains(animType) && currentAnimationType == animType)
        return;

      var corssFade = currentAnimationType switch
      {
        DialogueDataEnum.Portrait.AnimationType.None => false,
        DialogueDataEnum.Portrait.AnimationType.Surprised => true,
        DialogueDataEnum.Portrait.AnimationType.Jump => false,
        DialogueDataEnum.Portrait.AnimationType.Jumping => true,
        DialogueDataEnum.Portrait.AnimationType.Shake => false,
        DialogueDataEnum.Portrait.AnimationType.Shaking => true,
        DialogueDataEnum.Portrait.AnimationType.Run => false,
        DialogueDataEnum.Portrait.AnimationType.Speak => false,
        _ => throw new NotImplementedException(),
      };

      currentAnimationType = animType;

      var hash = animType switch
      {
        DialogueDataEnum.Portrait.AnimationType.None => AnimatorHash.Dialogue.Portrait.Idle,
        DialogueDataEnum.Portrait.AnimationType.Surprised => AnimatorHash.Dialogue.Portrait.Surprised,
        DialogueDataEnum.Portrait.AnimationType.Jump => AnimatorHash.Dialogue.Portrait.JumpOnce,
        DialogueDataEnum.Portrait.AnimationType.Jumping => AnimatorHash.Dialogue.Portrait.JumpLoop,
        DialogueDataEnum.Portrait.AnimationType.Shake => AnimatorHash.Dialogue.Portrait.ShakeOnce,
        DialogueDataEnum.Portrait.AnimationType.Shaking => AnimatorHash.Dialogue.Portrait.ShakeLoop,
        DialogueDataEnum.Portrait.AnimationType.Run => AnimatorHash.Dialogue.Portrait.Run,
        DialogueDataEnum.Portrait.AnimationType.Speak => AnimatorHash.Dialogue.Portrait.Speak,
        _ => throw new System.NotImplementedException()
      };

      if (corssFade)
        portraitAnimator.CrossFade(hash, 0.5f);
      else
        portraitAnimator.Play(hash,0, 0.0f);
    }

    public void SetAlpha(DialogueDataEnum.Portrait.AlphaType alphaType)
    {
      forwardAlpha = portraitData.GetAlphaValue(alphaType);
      if (isImageChanging == false && forwardSet != null)
        forwardSet.CanvasGroup.alpha = forwardAlpha;
    }

    private void SwapImageOrder(out UITalkingCharacterView.ImageSet forwardSet, out UITalkingCharacterView.ImageSet backwardSet)
    {
      useImageA = !useImageA;

      forwardSet = useImageA ? setA : setB;
      backwardSet = useImageA ? setB : setA;

      backwardSet.Image.transform.SetAsFirstSibling();
    }

    private async UniTask SetImageAsync(
      Sprite sprite, 
      DialogueDataEnum.Portrait.ChangeType changeType, 
      CancellationToken token,
      bool isTransparent)
    {
      SwapImageOrder(out var forwardSet, out var backwardSet);

      if (forwardSet.HatView != null)
        forwardSet.HatView.CanvasGroup.alpha = isTransparent ? 0.0f : 1.0f;

      this.forwardSet = forwardSet;
      switch (changeType)
      {
        case DialogueDataEnum.Portrait.ChangeType.None:
          {
            forwardSet.Image.sprite = sprite;
            backwardSet.Image.sprite = transparent;
            backwardSet.CanvasGroup.alpha = 0.0f;
          }
          break;

        case DialogueDataEnum.Portrait.ChangeType.Fade:
          {
            try
            {
              isImageChanging = true;
              forwardSet.Image.sprite = sprite;
              forwardSet.CanvasGroup.alpha = 0.0f;
              await DOTween
                .Sequence()
                .Join(forwardSet.CanvasGroup.DOFade(forwardAlpha, portraitData.ChangeDuration))
                .Join(backwardSet.CanvasGroup.DOFade(0.0f, portraitData.ChangeDuration))
                .OnComplete(() =>
                {
                  isImageChanging = false;
                })
                .ToUniTask(TweenCancelBehaviour.Complete, token);
            }
            catch (OperationCanceledException) { }
          }
          break;

        case DialogueDataEnum.Portrait.ChangeType.Move:
          {
            try
            {
              isImageChanging = true;

              var beginPosition = positionType switch
              {
                CharacterPositionType.Left => new Vector3(-portraitData.MoveLength, 0.0f, 0.0f),
                CharacterPositionType.Center => new Vector3(0.0f, -portraitData.MoveLength, 0.0f),
                CharacterPositionType.Right => new Vector3(portraitData.MoveLength, 0.0f, 0.0f),
                _ => throw new NotImplementedException(),
              };
              var endPosition = Vector3.zero;

              forwardSet
                .Image
                .rectTransform
                .anchoredPosition = beginPosition;

              forwardSet.Image.sprite = sprite;
              forwardSet.CanvasGroup.alpha = forwardAlpha;
              backwardSet.Image.sprite = transparent;
              
              await DOTween
                .Sequence()
                .Join(forwardSet.Image.rectTransform.DOAnchorPos(endPosition, portraitData.ChangeDuration))
                .OnComplete(() =>
                {
                  isImageChanging = false;
                })                
                .ToUniTask(TweenCancelBehaviour.Complete, token);
            }
            catch (OperationCanceledException) { }
          }
          break;
      }
    }
  }
}
