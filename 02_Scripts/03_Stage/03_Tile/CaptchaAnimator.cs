using LR.Stage.Player.Enum;
using UnityEngine;

namespace LR.Stage.TriggerTile
{
  public class CaptchaAnimator : MonoBehaviour
  {
    public enum Animation
    {
      SetLeftSprite,
      SetRightSprite,
      Idle,
      Resolving,
      Resolved,
      Reanimated,
    }

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void InitializePlayerType(PlayerType playerType)
    {
      if(playerType == PlayerType.Right)
      {
        transform.localScale = new Vector3(transform.localScale.x * -1.0f, transform.localScale.y, transform.localScale.z);
        spriteRenderer.flipX = true;
      }
    }

    public void Play(Animation animation)
      => animator.Play(animation switch
      {
        Animation.SetLeftSprite => AnimatorHash.CaptchaObject.SetLeftSprite,
        Animation.SetRightSprite => AnimatorHash.CaptchaObject.SetRightSprite,
        Animation.Idle => AnimatorHash.CaptchaObject.Idle,
        Animation.Resolving => AnimatorHash.CaptchaObject.Resolving,
        Animation.Resolved => AnimatorHash.CaptchaObject.Resolved,
        Animation.Reanimated => AnimatorHash.CaptchaObject.Reanimated,
        _ => throw new System.NotImplementedException(),
      },0, 0.0f);
  }
}
