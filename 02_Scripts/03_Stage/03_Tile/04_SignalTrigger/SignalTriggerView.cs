#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Text;
using UnityEngine;
using UnityEngine.Events;
using LR.Stage.TriggerTile.Enum;

namespace LR.Stage.TriggerTile
{
  public class SignalTriggerView : BaseTriggerTileView
  {
    [SerializeField] private ColorSO colorSO;
    [field: SerializeField] public ParticleSystem IdleEffect { get; private set; }
    [field: SerializeField] public ParticleSystem ACDCEffect { get; private set; }
    [field: SerializeField] public SpriteRenderer IconSpriteRenderer { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public ParticleSystem ACActivateParticle {  get; private set; }
    [field: Header("[ Key ]")]
    [field: SerializeField] public int Key { get; private set; } = -1;

    [field: Header("[ Life ]")]
    [field: SerializeField] public SignalLife SignalLife { get; private set; }

    public bool IsEnterKeyExist
      => Key >= 0;

    private const float DebuggingTextSpace = 0.6f;

    private readonly UnityEvent<Collider2D> onEnter = new();
    private readonly UnityEvent<Collider2D> onExit = new();

    private void OnValidate()
    {
      if (IsEnterKeyExist && colorSO != null)
      {
        if(IconSpriteRenderer != null)
          IconSpriteRenderer.color = colorSO.SignalColors[Key];
        if (ACActivateParticle != null)
        {
          var mainModule = ACActivateParticle.main;
          mainModule.startColor = colorSO.SignalColors[Key];
        }
        if(IdleEffect != null)
        {
          var mainModule = IdleEffect.main;
          mainModule.startColor = colorSO.SignalColors[Key];
        }
      }             
    }

    protected override void Awake()
    {
      base.Awake();

      var color = colorSO.SignalColors[Key];
      IconSpriteRenderer.color = color;
    }

    public override TriggerTileType GetTriggerType()
      => TriggerTileType.DefaultSignal;

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

      if (IsEnterKeyExist)
      {        
        var stb = new StringBuilder("{ " + Key +" }");
        var index = 0;
        
        var labelCenterStyle = new GUIStyle(EditorStyles.label)
        {
          alignment = TextAnchor.MiddleCenter
        };
        labelCenterStyle.normal.textColor = colorSO.SignalColors[Key];
        Handles.Label(transform.position + DebuggingTextSpace * index * Vector3.up, stb.ToString(), labelCenterStyle);
      }        
    }
#endif
  }
}
