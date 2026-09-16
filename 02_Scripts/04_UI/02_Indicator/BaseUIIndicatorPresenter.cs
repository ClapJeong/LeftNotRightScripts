using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Device;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.Indicator
{
  public class BaseUIIndicatorPresenter : IUIIndicatorPresenter
  {
    public class Model
    {
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IResourceManager resourceManager;
      [Inject] public UISO uiSO;
      [Inject] public ISFXController sfxController;

      [Inject] public Transform root;
      [Inject] public RectTransform beginTarget;
      [Inject] public Transform disableRoot;
    }

    private readonly Model model;
    private readonly BaseUIIndicatorView view;

    private readonly CTSContainer moveCTS = new();
    private readonly CTSContainer leftCTS = new();
    private readonly CTSContainer rightCTS = new();
    
    private readonly UnityEvent<List<Direction>> leftInputEvent = new();
    private readonly UnityEvent<List<Direction>> rightInputEvent = new();

    private RectTransform prevTarget;
    private RectTransform currentTarget;
    private float followT = 1.0f;
    private bool ignoreMoveSFX = true;

    public BaseUIIndicatorPresenter(
      Model model, 
      BaseUIIndicatorView view)
    {
      this.model = model;
      this.view = view;

      currentTarget = model.beginTarget;
      view.transform.SetParent(model.root);
      UpdateRectTransform(model.beginTarget);

      var updateDisposable = view.UpdateAsObservable().Subscribe(_ => OnUpdate());
      view.OnDestroyAsObservable().Subscribe(_ => updateDisposable.Dispose());
    }

    public void ReInitialize(Transform root, RectTransform targetRect)
    {
      moveCTS.Cancel();
      followT = 1.0f;
      currentTarget = targetRect;

      view.transform.SetParent(root);
      UpdateRectTransform(targetRect);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      if (view)
        view.DestroySelf();

      moveCTS.Dispose();
      leftCTS.Dispose();
      rightCTS.Dispose();
    }

    public async UniTask MoveAsync(RectTransform targetRect, bool isImmediately = false)
    {
      moveCTS.Cancel();

      if (isImmediately)
      {
        currentTarget = targetRect;
        followT = 1.0f;

        UpdateRectTransform(targetRect);
      }
      else
      {
        if(!ignoreMoveSFX)
          model.sfxController.PlayOnce(AudioSourceType.LeftUI, SFX.IndicatorMove);
        ignoreMoveSFX = false;
        moveCTS.Create();
        var token = moveCTS.token;
        await ChangeFollowTargetAsync(targetRect, token);
      }
    }

    public async UniTask MoveAsync(BaseSubmitView submitView, bool isImmediately = false)
      => await MoveAsync(submitView.Content, isImmediately);

    public async UniTask MoveAsync(GameObject gameObject, bool isImmediately = false)
    {
      if (gameObject.TryGetComponent<BaseSubmitView>(out var baseSubmitView))
        await MoveAsync(baseSubmitView);
      else if(gameObject.TryGetComponent<RectTransform>(out var rectTransform))
        await MoveAsync(rectTransform);
    }


    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await view.ShowAsync(isImmediately, token);
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {      
      moveCTS.Cancel();
      leftCTS.Cancel();
      rightCTS.Cancel();
      await view.HideAsync(isImmediately, token);
      view.transform.SetParent(model.disableRoot);
    }

    private void UpdateRectTransform(RectTransform targetRectTransform)
    {
      view.RectTransform.position = targetRectTransform.GetCenterPosition();
      view.RectTransform.rotation = targetRectTransform.rotation;
      view.RectTransform.SetSize(targetRectTransform.rect.size);
      view.RectTransform.localScale = targetRectTransform.localScale;
    }

    #region LeftInputGuide
    public void SetLeftInputGuide(Direction direction)
    {
      SetLeftInputGuide(new List<Direction>() 
      { 
        direction,
      });
    }

    public void SetLeftInputGuide(Navigation navigation)
    {
      var set = new List<Direction>();

      if (navigation.selectOnUp != null)
        set.Add(Direction.Up);
      if (navigation.selectOnRight != null)
        set.Add(Direction.Right);
      if (navigation.selectOnDown != null)
        set.Add(Direction.Down);
      if (navigation.selectOnLeft != null)
        set.Add(Direction.Left);

      SetLeftInputGuide(set);
    }

    public void SetLeftInputGuide(List<Direction> directions)
    {
      leftInputEvent?.Invoke(directions);

      leftCTS.Cancel();
      leftCTS.Create();
      var token = leftCTS.token;

      foreach (var direction in directions)
        MoveLeftGuideAsync(direction, token).Forget();
    }
    #endregion

    #region RightInputGuide
    public void PlayGoodSubmitSFX(bool ignoreNextMoveSFX = true)
    {
      model.sfxController.PlayOnce(AudioSourceType.RightUI, SFX.UIGoodSubmit);
      if(ignoreNextMoveSFX)
        ignoreMoveSFX = true;
    }

    public void PlayBadSubmitSFX(bool ignoreNextMoveSFX = true)
    {
      model.sfxController.PlayOnce(AudioSourceType.RightUI, SFX.UIBadSubmit);
      if(ignoreNextMoveSFX)
        ignoreMoveSFX = true;
    }
    #endregion

    private async UniTask MoveLeftGuideAsync(Direction direction, CancellationToken token)
    {
      var guideRectTransform = view.GetLeftGuideRect(direction);
      guideRectTransform.gameObject.SetActive(false);
      await WaitUntilMove(token);

      var duration = model.uiSO.Indicator.GuideMoveDuration;
      var length = model.uiSO.Indicator.LeftGuideMoveLength;
      var targetAnchoredPosition = direction switch
      {
        Direction.Up => Vector2.up * length,
        Direction.Right => Vector2.right * length,
        Direction.Down => Vector2.down * length,
        Direction.Left => Vector2.left * length,
        _ => throw new NotImplementedException(),
      };
      var defaultSpace = model.uiSO.Indicator.LeftGuideDefaultSpace;
      var defaultAnchoredSpace = direction switch
      {
        Direction.Up => Vector2.up * defaultSpace,
        Direction.Right => Vector2.right * defaultSpace,
        Direction.Down => Vector2.down * defaultSpace,
        Direction.Left => Vector2.left * defaultSpace,
        _ => throw new NotImplementedException(),
      };
      guideRectTransform.gameObject.SetActive(true);

      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          await DOTween
          .Sequence()
          .Join(guideRectTransform.DOAnchorPos(targetAnchoredPosition, duration))
          .Append(guideRectTransform.DOAnchorPos(defaultAnchoredSpace, duration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
        }
      }
      catch (OperationCanceledException)
      {
        guideRectTransform.gameObject.SetActive(false);
        guideRectTransform.anchoredPosition = defaultAnchoredSpace;
      }
    }

    private async UniTask WaitUntilMove(CancellationToken token)
      => await UniTask.WaitUntil(() => followT == 1.0f, PlayerLoopTiming.Update, token);

    private void OnUpdate()
    {
      if (currentTarget == null)
        return;

      if (followT < 1.0f)
      {
        var position = Vector3.Lerp(prevTarget.GetCenterPosition(), currentTarget.GetCenterPosition(), followT);
        view.RectTransform.position = position;
        var rectSize = Vector2.Lerp(prevTarget.rect.size, currentTarget.rect.size, followT);
        view.RectTransform.SetSize(rectSize);
        var rotation = Quaternion.Lerp(prevTarget.rotation, currentTarget.rotation, followT);
        view.RectTransform.rotation = rotation;

        if(followT < model.uiSO.Indicator.RectTransformDecreaseNormalized)
        {
          var t = followT / model.uiSO.Indicator.RectTransformDecreaseNormalized;
          var scale = Vector3.Lerp(prevTarget.localScale, model.uiSO.Indicator.RectTransformDecreasedScale, t);
          view.RectTransform.localScale = scale;
        }
        else if(followT < model.uiSO.Indicator.RectTransformIncreaseNormalized)
        {
          
        }
        else
        {
          var t = (followT - model.uiSO.Indicator.RectTransformIncreaseNormalized) / (1.0f - model.uiSO.Indicator.RectTransformIncreaseNormalized);
          var scale = Vector3.Lerp(model.uiSO.Indicator.RectTransformDecreasedScale, currentTarget.localScale, t);
          view.RectTransform.localScale = scale;
        }        
      }
      else
      {
        var targetPos = currentTarget.GetCenterPosition();
        if (view.RectTransform.position != targetPos)
          view.RectTransform.position = targetPos;
        var targetSize = currentTarget.rect.size;
        if (view.RectTransform.rect.size != targetSize)
          view.RectTransform.SetSize(targetSize);
        var targetScale = currentTarget.localScale;
        if (view.RectTransform.localScale != targetScale)
          view.RectTransform.localScale = targetScale;
        var targetRotation = currentTarget.rotation;
        if (view.RectTransform.rotation != targetRotation)
          view.RectTransform.rotation = targetRotation;
      }
    }

    private async UniTask ChangeFollowTargetAsync(RectTransform newTarget, CancellationToken token)
    {
      try
      {
        followT = 0.0f;
        prevTarget = currentTarget;
        currentTarget = newTarget;
        var duration = 0.0f;
        while (duration < model.uiSO.Indicator.RectTransformMoveDuration)
        {
          duration += Time.deltaTime;
          followT = duration / model.uiSO.Indicator.RectTransformMoveDuration;
          await UniTask.Yield();
        }
        followT = 1.0f;
      }
      catch (OperationCanceledException) { }
    }

    public void SubscribeLeftInputGuide(UnityAction<List<Direction>> unityAction)
      => leftInputEvent.AddListener(unityAction);

    public void UnsubscribeLeftInputGuide(UnityAction<List<Direction>> unityAction)
      => leftInputEvent.RemoveListener(unityAction);

    public void SubscribeRightInputGuide(UnityAction<List<Direction>> unityAction)
      => rightInputEvent.AddListener(unityAction);

    public void UnsubscribeRightInputGuide(UnityAction<List<Direction>> unityAction)
      => rightInputEvent.RemoveListener(unityAction);
  }
}