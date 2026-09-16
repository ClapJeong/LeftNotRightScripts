using UnityEngine;
using UnityEngine.Events;
using LR.Stage.TriggerTile.Enum;

namespace LR.Stage.TriggerTile
{
  public class ClearTriggerTileView : BaseTriggerTileView
  {
    public bool EnableLight = true;

    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    [field: SerializeField] public ParticleSystem IdleParticle { get; private set;  }
    [field: SerializeField] public ParticleSystem ActivateParticle { get; private set; }
    [SerializeField] private TriggerTileType triggerTileType;
    private readonly UnityEvent<Collider2D> onEnter = new();
    private readonly UnityEvent<Collider2D> onExit = new();

    private void OnValidate()
    {
      if(triggerTileType!=TriggerTileType.LeftClear &&
        triggerTileType!=TriggerTileType.RightClear)
        triggerTileType = TriggerTileType.LeftClear;
    }

    public override TriggerTileType GetTriggerType()
      => triggerTileType;

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
  }
}