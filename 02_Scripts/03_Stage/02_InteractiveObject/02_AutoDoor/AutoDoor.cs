using Cysharp.Threading.Tasks;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.Player;
using LR.Table.TriggerTile;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.InteractiveObject.AutoMover
{
  public class AutoDoor : BaseInteractiveObject
  {
    [SerializeField] private new Collider2D collider2D;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isOpened = false;
    [SerializeField] private ParticleSystem openEffect;
    [SerializeField] private List<ParticleSystem> closeEffects;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Direction collaspeDirection;
    [SerializeField] private BounceData collaspeBounceData;

    private readonly Collider2D[] overlapResults = new Collider2D[4];
    private readonly CTSContainer cts = new();
    private bool isEnable = true;
    private IStageStateProvider stageStateProvider;
    private IPlayerGetter playerGetter;

    public override void Initialize(StageManager stageManager, ISFXController sfxController)
    {
      this.stageStateProvider = stageManager;
      this.playerGetter = stageManager;
      InitializeState();
    }

    public override void Enable(bool isEnable)
    {
      this.isEnable = isEnable;
    }    

    public override void Restart()
    {
      isEnable = true;
      spriteRenderer.SetAlpha(1.0f);
      cts.Cancel();
      animator.speed = 1.0f;

      InitializeState();
    }

    private void InitializeState()
    {
      if (isOpened)
      {
        animator.Play(AnimatorHash.AutoDoor.Opened, 0, 0.0f);
        collider2D.isTrigger = true;
      }
      else
      {
        animator.Play(AnimatorHash.AutoDoor.Closed, 0, 0.0f);
        collider2D.isTrigger = false;
      }
    }

    public void Open(bool isACDC)
    {
      if (!isEnable)
        return;

      cts.Cancel();
      cts.Create();
      var token = cts.token;
      ChangeAsync(
        AnimatorHash.AutoDoor.Opening,
        token, () =>
        {
          if(!isACDC)
            spriteRenderer.SetAlpha(0.5f);
          collider2D.isTrigger = true;
          openEffect.Play();
        }).Forget();
    }

    public void Close()
    {
      if (!isEnable)
        return;

      spriteRenderer.SetAlpha(1.0f);

      foreach (var closeEffect in closeEffects)
        closeEffect.Play();

      if (stageStateProvider.IsPlayingState && IsPlayerOverlapping(out var player))
      {
        var isOverPosition = collaspeDirection switch
        {
          Direction.Up => player.transform.position.y < transform.position.y,
          Direction.Right => player.transform.position.x < transform.position.x,
          Direction.Down => player.transform.position.y > transform.position.y,
          Direction.Left => player.transform.position.x > transform.position.x,
          _ => throw new NotImplementedException(),
        };

        var movePosition =
          (isOverPosition ? -1.0f : 1.0f)
          * 0.5f
          * collaspeDirection switch
          {
            Direction.Up => new Vector3(0.0f, collider2D.bounds.size.y, 0.0f),
            Direction.Right => new Vector3(collider2D.bounds.size.x, 0.0f, 0.0f),            
            Direction.Down => new Vector3(0.0f, -collider2D.bounds.size.y, 0.0f),
            Direction.Left => new Vector3(-collider2D.bounds.size.x, 0.0f, 0.0f),            
            _ => throw new NotImplementedException(),
          };

        playerGetter
          .GetPlayer(player.GetComponentInParent<IPlayerView>().GetPlayerType())
          .GetMoveController()
          .MovePosition(player.transform.position + movePosition);
      }

      collider2D.isTrigger = false;
      cts.Cancel();
      cts.Create();
      var token = cts.token;
      ChangeAsync(
        AnimatorHash.AutoDoor.Closing, 
        token, 
        onComplete: null).Forget();
    }

    private bool IsPlayerOverlapping(out GameObject player)
    {
      player = null;

      var filter = new ContactFilter2D();
      filter.SetLayerMask(playerLayer);
      filter.useLayerMask = true;
      filter.useTriggers = true;

      int count = collider2D.Overlap(filter, overlapResults);

      if (count <= 0)
        return false;

      player = overlapResults[0].gameObject;
      return true;
    }

    private void OnDestroy()
    {
      cts.Dispose();
    }

    private async UniTask ChangeAsync(int targetHash, CancellationToken token, UnityAction onComplete)
    {
      try
      {
        animator.speed = 1.0f;
        animator.Play(targetHash, 0, 0.0f);
        await UniTask.WaitForEndOfFrame();
        
        while (true)
        {
          token.ThrowIfCancellationRequested();
          
          if (animator?.GetCurrentAnimatorStateInfo(0).shortNameHash != targetHash)
            break;

          token.ThrowIfCancellationRequested();
          UpdateAnimatorSpeed();

          await UniTask.Yield();
        }
        onComplete?.Invoke();
      }
      catch (OperationCanceledException) { }
    }

    private void UpdateAnimatorSpeed()
      => animator.speed = stageStateProvider.GetState() == StageEnum.State.Pause ? 0.0f : 1.0f;

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
      base.OnDrawGizmos();

      Gizmos.color = Color.red;
      Gizmos.DrawLine(transform.position, transform.TransformPoint(collaspeDirection.ParseVector2()));
    }
#endif
  }
}