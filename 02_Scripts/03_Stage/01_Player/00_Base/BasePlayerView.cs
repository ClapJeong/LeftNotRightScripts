using UnityEngine;
using LR.Stage.Player.Enum;
using UnityEngine.Events;

namespace LR.Stage.Player
{
  public class BasePlayerView : MonoBehaviour, IPlayerView
  {
    [SerializeField] private PlayerType playerType;

    [field: SerializeField] public Animator Animator {  get; private set; }
    [field: SerializeField] public Rigidbody2D Rigidbody2D { get; private set; }    
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    [field: SerializeField] public PlayerParticleSet ParticleSet { get; private set; }
    [field: SerializeField] public Transform GimmickGuideRoot {  get; private set; }

    public GameObject GameObject
      => gameObject;

    public Transform Transform
      => transform;

    private readonly UnityEvent<Collision2D> onCollisionEnter2D = new();
    private readonly UnityEvent<Collision2D> onCollisionExit2D = new();

    #region IPlayerView
    public PlayerType GetPlayerType()
      => playerType;

    public void SubscribeOnCollisionEnter2D(UnityAction<Collision2D> onCollisionEnter)
      => onCollisionEnter2D.AddListener(onCollisionEnter);

    public void UnsubscribeOnCollisionEnter2D(UnityAction<Collision2D> onCollisionEnter)
      => onCollisionEnter2D.RemoveListener(onCollisionEnter);

    public void SubscribeOnCollisionExit2D(UnityAction<Collision2D> onCollisionExit)
      => onCollisionExit2D.AddListener(onCollisionExit);

    public void UnsubscribeOnCollisionExit2D(UnityAction<Collision2D> onCollisionExit)
      => onCollisionExit2D.RemoveListener(onCollisionExit);

    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
      onCollisionEnter2D?.Invoke(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
      onCollisionExit2D?.Invoke(collision);
    }
  }
}