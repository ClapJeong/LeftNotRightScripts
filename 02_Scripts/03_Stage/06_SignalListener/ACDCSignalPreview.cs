using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace LR.Stage.SignalListener
{
  public class ACDCSignalPreview :  BaseSignalPreview
  {
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    public override void Initialize(Vector3 worldPosition, Color color)
    {
      transform.position = worldPosition;
      spriteRenderer.color = color;
      spriteRenderer.SetAlpha(0.5f);
    }

    public override void Activate()
    {
      animator.Play(AnimatorHash.Signal.Activate);
      spriteRenderer.SetAlpha(1.0f);
    }

    public override void Deactivate()
    {
      animator.Play(AnimatorHash.Signal.Deactivate);
      spriteRenderer.SetAlpha(0.5f);
    }

    public override void Restart()
    {
      animator.Play(AnimatorHash.Signal.Deactivate);
      spriteRenderer.SetAlpha(0.5f);
    }
  }
}
