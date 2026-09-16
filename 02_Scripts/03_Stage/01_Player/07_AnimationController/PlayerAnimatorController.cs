using UnityEngine;

namespace LR.Stage.Player
{
  public class PlayerAnimatorController : IPlayerAnimatorController
  {
    private readonly Animator animator;

    public PlayerAnimatorController(Animator animator)
    {
      this.animator = animator;
    }

    public void Play(int hash)
    {
      animator.Play(hash);
    }

    public void Play(string name)
      => animator.Play(name);

    public void SetBool(int hash, bool value)
      => animator.SetBool(hash, value);

    public void SetBool(string name, bool value)
      => animator.SetBool(name, value);

    public void SetFloat(int hash, float value)
      => animator.SetFloat(hash, value);

    public void SetFloat(string name, float value)
      => animator.SetFloat(name, value);
  }
}
