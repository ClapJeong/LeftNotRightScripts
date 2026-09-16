using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace LR.UI.GameScene.Dialogue.Shake
{
  public class BackgroundShakeFallObject : MonoBehaviour, IDisposable
  {
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    private readonly CTSContainer cts = new();

    private void Awake()
    {
      gameObject.SetActive(false);
      canvasGroup.alpha = 0.0f;
    }

    public async UniTask PlayAsync(
      Vector3 screenPosition, 
      Vector3 size,
      float rotate,
      float beginAlpha,
      float minSpeed,
      float maxSpeed,
      float duration,
      UnityAction onComplete)
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;

      if (this == null ||
        rectTransform == null)
        return;

      rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f));
      rectTransform.position = screenPosition;
      rectTransform.localScale = size;
      canvasGroup.alpha = beginAlpha;

      gameObject.SetActive(true);

      var time = 0.0f;
      try
      {
        while (time < duration)
        {
          token.ThrowIfCancellationRequested();

          var t = time / duration;
          var speed = Mathf.Lerp(minSpeed, maxSpeed, t);
          var alpha = Mathf.Lerp(beginAlpha, 0.0f, t);
          rectTransform.position += speed * Time.deltaTime * Vector3.down;
          rectTransform.eulerAngles += rotate * Time.deltaTime * Vector3.forward;
          canvasGroup.alpha = alpha;

          time += Time.deltaTime;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        if (this != null)
        {
          onComplete?.Invoke();
          canvasGroup.alpha = 0.0f;
          gameObject.SetActive(false);
        }
      }
    }

    public void Dispose()
    {
      cts.Dispose();
    }

    private void OnDestroy()
    {
      cts.Dispose();
    }
  }
}
