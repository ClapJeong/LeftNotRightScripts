using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LR.UI
{
  public class BaseSubmitView : MonoBehaviour, IUISubmitView
  {
    #region [ Properties ]
    private RectTransform rectTransform;

    public RectTransform RectTransform
    {
      get
      {
        rectTransform ??= GetComponent<RectTransform>();
        return rectTransform;
      }
    }

    private RectTransform content;

    public RectTransform Content
    {
      get
      {
        content ??= transform.GetChild(0).GetComponent<RectTransform>();
        return content;
      }
    }

    private Image image;
    public Image Image
    {
      get
      {
        if (image == null) 
          image = GetComponentInChildren<Image>();
        return image;
      }
    }

    private TextMeshProUGUI tmp;
    public TextMeshProUGUI TMP
    {
      get
      {
        if (tmp == null)
          tmp = GetComponentInChildren<TextMeshProUGUI>();
        return tmp;
      }
    }

    private UISO uiSO = null;
    public UISO UISO
    {
      get
      {
        if (uiSO == null)
          uiSO = GlobalManager.instance.Table.UISO;
        return uiSO;
      }
    }

    private CanvasGroup canvasGroup;
    public CanvasGroup CanvasGroup
    {
      get
      {
        if (canvasGroup == null)
          canvasGroup = GetComponentInChildren<CanvasGroup>();
        return canvasGroup;
      }
    }

    private Selectable selectable;
    public Selectable Selectable
    {
      get
      {
        if (selectable == null)
          selectable = GetComponentInChildren<Selectable>();
        return selectable;
      }
    }
    #endregion

    [SerializeField] private bool enableIdleWiggle = true;

    private readonly UnityEvent inputEvent = new();
    private readonly CTSContainer interactCTS = new();
    private readonly CTSContainer idleCTS = new();
    private bool isEnable = true;

    private void Awake()
    {
      if (enableIdleWiggle)
      {
        var token = idleCTS.token;
        IdleAsync(token).Forget();
      }      
    }

    private void OnDestroy()
    {
      interactCTS.Dispose();
      idleCTS.Dispose();

      GlobalManager
        .instance
        .UIsubmitController
        .Release(this);      
    }

    public void Enable(bool isEnable)
      => this.isEnable = isEnable;

    public bool IsEnable()
      => isEnable;

    public void Perform(Direction direction)
    {     
      if (!isEnable)
        return;

      inputEvent?.Invoke();

      var duration = UISO.Indicator.SubmitPingpongDuration;
      var moveLength = UISO.Indicator.RightSubmitPingpongLength;
      interactCTS.Cancel();
      interactCTS.Create();
      var token = interactCTS.token;
      PingpongAsync(duration, direction.ParseVector2() * moveLength, token).Forget();
    }

    public void Perform()
    {
      if (!isEnable)
        return;

      inputEvent?.Invoke();

      var duration = UISO.Indicator.SubmitPingpongDuration;
      var scale = UISO.Indicator.UISubmitScaleValue;
      interactCTS.Cancel();
      interactCTS.Create();
      var token = interactCTS.token;
      ScaleAsync(duration, scale, token).Forget();
    }

    private async UniTask PingpongAsync(float duration, Vector2 length, CancellationToken token)
    {
      try
      {
        await
          DOTween
          .Sequence()
          .Join(Content.DOAnchorPos(length, duration))
          .SetLoops(2, LoopType.Yoyo)
          .SetLink(gameObject)
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException)
      {
        Content.anchoredPosition = Vector3.zero;
      }
    }

    private async UniTask ScaleAsync(float duration, float scale, CancellationToken token)
    {
      try
      {
        await
          DOTween
          .Sequence()
          .Join(Content.DOScale(scale, duration))
          .SetLoops(2, LoopType.Yoyo)
          .SetLink(gameObject)
          .ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException)
      {
        Content.localScale = Vector3.one;        
      }
    }

    public void OnSelect(Direction direction)
    {
      if (!isEnable)
        return;

      idleCTS.Cancel();

      if (direction == Direction.Up || direction == Direction.Down)
        return;

      interactCTS.Cancel();
      interactCTS.Create();
      var token = interactCTS.token;
      var eulear = UISO.Indicator.SelectEuler  *
        (direction == Direction.Left ? -1 : 1);
      var duration = UISO.Indicator.SelectRotateDuration;
      RotateAsync(eulear, duration, token).Forget();
    }

    public void OnSelect()
    {
      if (!isEnable)
        return;

      idleCTS.Cancel();
    }

    public void OnExit()
    {
      if (enableIdleWiggle && idleCTS.cts.IsCancellationRequested)
      {
        idleCTS.Create();
        var idleToken = idleCTS.token;
        IdleAsync(idleToken).Forget();
      }      
    }

    private async UniTask IdleAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          if (this == null || !gameObject.activeInHierarchy)
          {
            await UniTask.Yield();
            continue;
          }

          var currentPos = Content.anchoredPosition;

          var wiggleRange = UISO.Indicator.SubmitIdleWiggleRange;
          var randomPos = new Vector2(
            UnityEngine.Random.Range(-wiggleRange, wiggleRange),
            UnityEngine.Random.Range(-wiggleRange, wiggleRange));

          var distance = Vector2.Distance(currentPos, randomPos);
          var duration = distance / UISO.Indicator.SubmitIdleWiggleSpeed;

          await Content
            .DOAnchorPos(randomPos, duration)
            .SetEase(Ease.Linear)
            .SetLink(gameObject)
            .ToUniTask(TweenCancelBehaviour.Kill, token);
        }
      }
      catch (OperationCanceledException)
      {
        if (Content != null)
          Content.anchoredPosition = Vector2.zero;
      }
    }

    private async UniTask RotateAsync(Vector3 eulear, float duration, CancellationToken token)
    {
      try
      {
        for(int i = 0; i < UISO.Indicator.SelectEulerCount; i++)
        {
          var sign = i % 2 == 0 ? 1 : -1;
          var targetEuelr = (1.0f - UISO.Indicator.SelectEulerCurve.Evaluate(i/(float)UISO.Indicator.SelectEulerCount)) * sign * eulear;
          await DOTween
          .Sequence()
          .Join(Content.DOLocalRotate(targetEuelr, duration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
        }
        await DOTween
          .Sequence()
          .Join(Content.DOLocalRotate(Vector3.zero, duration))
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
      finally
      {
        if(this != null)
          Content.localEulerAngles = Vector3.zero;
      }
    }

    #region Subscribes
    public void Subscribe( UnityAction onPerformed)
      => inputEvent.AddListener(onPerformed);

    public void Unsubscribe(UnityAction onPerformed)
      => inputEvent.RemoveListener(onPerformed);

    public void UnsubscribeAll()
      => inputEvent.RemoveAllListeners();
    #endregion
  }
}