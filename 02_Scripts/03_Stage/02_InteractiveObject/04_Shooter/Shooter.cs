using Cysharp.Threading.Tasks;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace LR.Stage.InteractiveObject
{
  public class Shooter : BaseInteractiveObject
  {
    [System.Serializable]
    public class DirectionSet
    {
      public Sprite sprite;
      public Vector2 offet;
      public Vector2 rayOffset;
    }
    [SerializeField] private Direction direction = Direction.Up;
    [SerializeField] private float interval;
    [SerializeField] private float beginDuration = 0.0f;
    [SerializeField] private float sfxRange;
    [SerializeField] private float sfxVolumeMin;
    [SerializeField] private ShooterSFXArea sfxArea;    
    [Space(10)]
    [SerializeField] private DirectionSet upDirectionSet;
    [SerializeField] private DirectionSet rightDirectionSet;
    [SerializeField] private DirectionSet downDirectionSet;
    [SerializeField] private DirectionSet leftDirectionSet;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer fillSpriteRenderer;
    [SerializeField] private SpriteRenderer fillOutlineSpriteRenderer;
    [Space(10)]
    [SerializeField] private ShooterBullet bulletPrefab;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float blinkBeginNormalized;
    [SerializeField] private float blinkDuration;
    [SerializeField] private float blinkIntensity;
    [Header("[ Gizmo ]")]
    [SerializeField] private float gizmoLength = 3.0f;

    private readonly List<ShooterBullet> activatedBullets = new();
    private readonly Queue<ShooterBullet> deactivatedBullets = new();

    private CTSContainer blinkCTS = new();
    private MaterialPropertyBlock glowMatBlock;
    private Transform bulletRoot;
    private IStageStateProvider stageStateProvider;
    private IPlayerGetter playerGetter;
    private ISFXController sfxController;
    private IPlayerView sfxTargetPlayer;

    private float duration = 0.0f;
    private bool isEnable;

    private DirectionSet GetDirectionSet(Direction direction)
      => direction switch
      {
        Direction.Up => upDirectionSet,
        Direction.Right => rightDirectionSet,
        Direction.Down => downDirectionSet,
        Direction.Left => leftDirectionSet,
        _ => throw new NotImplementedException(),
      };

    private void OnValidate()
    {
      transform.eulerAngles = new Vector3(0.0f, 0.0f, direction switch
      {
        Direction.Up => 0.0f,
        Direction.Right => 270.0f,
        Direction.Down => 180.0f,
        Direction.Left => 90.0f,
        _ => throw new NotImplementedException(),
      });

      if(animator != null)
      {
        animator.transform.localPosition = Vector3.up * (direction == Direction.Down ? 0.5f : 1.0f);
      }

      if (spriteRenderer != null)
      {
        spriteRenderer.sprite = GetDirectionSet(direction).sprite;
        spriteRenderer.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -transform.eulerAngles.z);
      }
      if (fillSpriteRenderer != null)
      {
        fillSpriteRenderer.transform.eulerAngles = new Vector3(0.0f, 0.0f, direction switch
        {
          Direction.Up => 0.0f,
          Direction.Right => 180.0f,
          Direction.Down => 0.0f,
          Direction.Left => 180.0f,
          _ => throw new NotImplementedException(),
        });
      }
      if (fillOutlineSpriteRenderer != null)
      {
        fillOutlineSpriteRenderer.transform.eulerAngles = new Vector3(0.0f, 0.0f, direction switch
        {
          Direction.Up => 0.0f,
          Direction.Right => 180.0f,
          Direction.Down => 0.0f,
          Direction.Left => 180.0f,
          _ => throw new NotImplementedException(),
        });
      }
    }

    public override void Initialize(StageManager stageManager, ISFXController sfxController)
    {
      stageStateProvider = stageManager;
      playerGetter = stageManager;
      bulletRoot = stageManager.GetGameObjectInstantiateRoot();
      this.sfxController = sfxController;
      duration = beginDuration;
      isEnable = true;
      UpdateFill(0.0f);

      glowMatBlock = new();
      spriteRenderer.GetPropertyBlock(glowMatBlock);
      UpdateGlowIntensity(0.0f);

      animator.Play(direction switch
      {
        Direction.Up => AnimatorHash.Shooter.UpIdle,
        Direction.Right => AnimatorHash.Shooter.RightIdle,
        Direction.Down => AnimatorHash.Shooter.DownIdle,
        Direction.Left => AnimatorHash.Shooter.LeftIdle,
        _ => throw new NotImplementedException(),
      });

      sfxArea.SubscribeOnPlayerEnter(playerView => this.sfxTargetPlayer = playerView);
      sfxArea.SubscribeOnPlayerExit(() => this.sfxTargetPlayer = null);
    }

    public override void Enable(bool isEnable)
    {
      this.isEnable = isEnable;
    }

    public override void Restart()
    {
      blinkCTS.Cancel();
      duration = beginDuration;
      foreach (var bullet in activatedBullets.ToList())
        bullet.DeactiveImmedieately();
    }

    private void Update()
    {
      if (!isEnable || !stageStateProvider.IsPlayingState)
        return;

      var prevT = duration / interval;
      duration += Time.deltaTime;
      var t = duration / interval;

      if (t >= 1.0f)
      {
        Shoot();
        duration = 0.0f;
      }

      if (prevT < blinkBeginNormalized && t >= blinkBeginNormalized)
        BlinkAsync().Forget();

      UpdateFill(duration / interval);
    }

    private void UpdateFill(float value)
    {
      fillSpriteRenderer.size = new Vector2(value, 1.0f);
    }

    private void Shoot()
    {
      var bullet = GetDisableBullet();
      var offset = GetDirectionSet(direction).offet;
      bullet.Shoot(transform.TransformPoint(offset), transform.rotation);

      activatedBullets.Add(bullet);

      animator.Play(direction switch
      {
        Direction.Up => AnimatorHash.Shooter.UpShoot,
        Direction.Right => AnimatorHash.Shooter.RightShoot,
        Direction.Down => AnimatorHash.Shooter.DownShoot,
        Direction.Left => AnimatorHash.Shooter.LeftShoot,
        _ => throw new NotImplementedException(),
      });

      PlaySFX();
    }

    private void PlaySFX()
    {
      if(sfxTargetPlayer != null && sfxRange > 0.0f)
      {
        var playerType = sfxTargetPlayer.GetPlayerType();
        var sfxPosition = playerType.ParseToAudioSourceType();
        var sfx = playerType.ParseToShooterSFX();
        var playerPosition = sfxTargetPlayer.Transform.position;
        var distance = Vector3.Distance(transform.position, playerPosition);
        var volume = 1.0f - Mathf.Abs(distance / sfxRange);
        volume = Mathf.Clamp(volume, sfxVolumeMin, 1.0f);
        sfxController.PlayOnce(sfxPosition, sfx, true, volume);
      }
    }

    private ShooterBullet GetDisableBullet()
    {
      if(deactivatedBullets.TryDequeue(out var deactvatedBullet))
      {
        return deactvatedBullet;
      }
      else
      {
        var newBullet = CreateNewBullet();
        newBullet.Initialize(
          stageStateProvider,
          playerGetter,
          obstacleLayer,
          onDestroyed: () => OnBulletDestroyed(newBullet),
          gameObject.GetHashCode(),
          sfxController);
        return newBullet;
      }
    }

    private ShooterBullet CreateNewBullet()
    {
      var offset = GetDirectionSet(direction).offet;
      var position = transform.TransformPoint(offset);
      var rotation = transform.rotation;

      return Instantiate(bulletPrefab, position, rotation, bulletRoot);
    }

    private void OnBulletDestroyed(ShooterBullet bullet)
    {
      activatedBullets.Remove(bullet);

      deactivatedBullets.Enqueue(bullet);
    }

    private async UniTask BlinkAsync()
    {
      blinkCTS.Cancel();
      blinkCTS.Create();
      var token = blinkCTS.token;
      try
      {
        UpdateGlowIntensity(blinkIntensity);
        var duration = 0.0f;
        while(duration < blinkDuration)
        {
          token.ThrowIfCancellationRequested();
          if (!stageStateProvider.IsPlayingState)
          {
            await UniTask.Yield();
            continue;
          }

          duration += Time.deltaTime;
          UpdateGlowIntensity(Mathf.Lerp(blinkIntensity, 0.0f, duration / blinkDuration));

          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
      finally
      {
        if (this != null)
          UpdateGlowIntensity(0.0f);
      }
    }

    private void UpdateGlowIntensity(float intensity)
    {
      glowMatBlock.SetFloat(ShaderHash.Glow._Intensity, intensity);
      spriteRenderer.SetPropertyBlock(glowMatBlock);
    }

    private void OnDestroy()
    {
      blinkCTS.Dispose();
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
      base.OnDrawGizmos();

      Gizmos.color = Color.red;

      var labelCenterStyle = new GUIStyle(EditorStyles.label)
      {
        alignment = TextAnchor.MiddleCenter
      };
      labelCenterStyle.normal.textColor = Color.red;
      Handles.Label(transform.position + Vector3.up, interval.ToString("F2"), labelCenterStyle);

      var offset = GetDirectionSet(direction).offet;
      var beginPositon = transform.TransformPoint(offset);
      Gizmos.DrawLine(beginPositon, beginPositon + transform.up * gizmoLength);      
    }

    private void OnDrawGizmosSelected()
    {
      DrawCircle(transform.position, sfxRange);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
      const int segments = 8; // 8각형

      float angleStep = 360f / segments;
      Vector3 prev = center + new Vector3(radius, 0f, 0f);

      for (int i = 1; i <= segments; i++)
      {
        float angle = angleStep * i * Mathf.Deg2Rad;
        Vector3 next = center + new Vector3(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius,
            0f);

        Gizmos.DrawLine(prev, next);
        prev = next;
      }
    }
#endif
  }
}