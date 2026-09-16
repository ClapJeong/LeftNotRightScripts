using LR.Stage.TriggerTile.Enum;
using LR.Stage.TriggerTile.Portal;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LR.Stage.TriggerTile
{
  public class PortalTriggerTileView : BaseTriggerTileView
  {
    [SerializeField] private Color gizmoColor;

    [field: SerializeField] public PortalTriggerTileView TargetPortal { get; private set;}
    [field: SerializeField] public PlayerDetectArea DetectArea { get; private set; }
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    [field: SerializeField] public Animator EffectAnimator { get; private set; }
    [field: SerializeField] public ParticleSystem EnterEffect { get; private set; }
    [field: SerializeField] public ParticleSystem MoveEffect { get; private set; }
    [field: SerializeField] public ParticleSystem InnerEffect { get; private set; }
    [field: SerializeField] public ParticleSystem OutterEffect { get; private set; }

    private readonly UnityEvent<Collider2D> onEnter = new();
    private readonly UnityEvent<Collider2D> onExit = new();

    private void OnValidate()
    {
      if (Application.isPlaying)
        return;

#if UNITY_EDITOR
      if (PrefabUtility.IsPartOfPrefabAsset(this))
        return;
#endif
      if (TargetPortal != null && TargetPortal.TargetPortal == null)
        TargetPortal.TargetPortal = this;
    }

    public override TriggerTileType GetTriggerType()
      => TriggerTileType.Portal;

    public override void SubscribeOnEnter(UnityAction<Collider2D> onEnter)
    {
      this.onEnter.RemoveListener(onEnter);
      this.onEnter.AddListener(onEnter);
    }

    public override void SubscribeOnExit(UnityAction<Collider2D> onExit)
    {
      this.onExit.RemoveListener(onExit);
      this.onExit.AddListener(onExit);
    }

    public override void UnsubscribeOnEnter(UnityAction<Collider2D> onEnter)
    {
      this.onEnter.AddListener(onEnter);
    }

    public override void UnsubscribeOnExit(UnityAction<Collider2D> onExit)
    {
      this.onExit.AddListener(onExit);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      onEnter?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
      onExit?.Invoke(collision);
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
      base.OnDrawGizmos();

      if(TargetPortal != null)
      {
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(transform.position, TargetPortal.transform.position);
      }
    }
#endif
  }
}

