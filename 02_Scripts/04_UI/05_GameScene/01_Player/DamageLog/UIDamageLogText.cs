using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace LR.UI.GameScene.Player
{
  public class UIDamageLogText : MonoBehaviour
  {
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    private readonly UnityEvent onComplete = new();
    private UISO uiSO;
    private CTSContainer cts = new();    

    public void Initialize(UISO uiSO, Color color, UnityAction onComplete)
    {
      this.uiSO = uiSO;
      tmp.color = color;
      this.onComplete.AddListener(onComplete);
    }

    public void Play(float damage)
    {
      cts.Cancel();
      cts.Create();      

      var randomX = UnityEngine.Random.Range(-uiSO.Player.DamageLog.RandomXRange, uiSO.Player.DamageLog.RandomXRange);
      var randomY = UnityEngine.Random.Range(-uiSO.Player.DamageLog.RandomYRange, uiSO.Player.DamageLog.RandomYRange);
      rectTransform.anchoredPosition = new Vector2(randomX, randomY);

      tmp.text = $"-{damage:f2}";

      var unit = (damage / uiSO.Player.DamageLog.DamageUnit);
      var length = unit * uiSO.Player.DamageLog.LengthPerUnit;
      var duration = unit * uiSO.Player.DamageLog.SecondPerUnit;

      canvasGroup.alpha = 1.0f;

      PlayAsync(length, duration).Forget();
    }

    private async UniTask PlayAsync(float length, float duration)
    {
      try
      {
        var token = cts.token;
        var targetPos = rectTransform.anchoredPosition + Vector2.up * length;
        var fadeBeginDuration = duration * uiSO.Player.DamageLog.FadeBeginDurationRatio;
        var fadeDuration = duration - fadeBeginDuration;
        await DOTween
          .Sequence()
          .Join(rectTransform.DOAnchorPos(targetPos, duration))
          .AppendInterval(fadeBeginDuration)
          .Append(canvasGroup.DOFade(0.0f, fadeDuration))
          .AppendCallback(() =>
          {
            onComplete?.Invoke();
          })
          .ToUniTask(TweenCancelBehaviour.Kill, token);        
      }
      catch (OperationCanceledException)
      {
        if (this != null)
          canvasGroup.alpha = 0.0f;
      }
    }

    private void OnDestroy()
    {
      cts.Dispose();
    }
  }
}

