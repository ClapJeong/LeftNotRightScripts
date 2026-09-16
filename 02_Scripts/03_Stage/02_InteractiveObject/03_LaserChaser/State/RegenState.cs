using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.InteractiveObject.LaserChaserState
{
  public class RegenState : IState
  {
    private readonly SpriteRenderer fillSpriteRenderer;
    private readonly UnityAction<float> onFillAmount;
    private readonly LaserChaser.Model model;

    private float duration;

    public RegenState(
      SpriteRenderer fillSpriteRenderer,
      UnityAction<float> onFillAmount, 
      LaserChaser.Model model)
    {
      this.fillSpriteRenderer = fillSpriteRenderer;
      this.onFillAmount = onFillAmount;
      this.model = model;

      duration = model.RegenDuration;
    }

    public void OnEnter()
    {
      fillSpriteRenderer.SetAlpha(0.4f);
      duration = model.RegenDuration;
      onFillAmount?.Invoke(1.0f);
    }

    public void OnExit()
    {
      fillSpriteRenderer.SetAlpha(1.0f);
      onFillAmount?.Invoke(0.0f);
    }

    public void OnUpdate(UnityAction onComplete)
    {
      duration = Mathf.Max(0.0f, duration - Time.deltaTime);

      onFillAmount?.Invoke(duration / model.RegenDuration);

      if (duration <= 0.0f)
        onComplete?.Invoke();
    }

    public void OnPuase()
    {

    }

    public void UpdateTarget(Transform target)
    {
      
    }
  }
}
