namespace LR.Stage.Player
{
  public interface IPlayerAnimatorController
  {
    public void Play(int hash);

    public void Play(string name);

    public void SetFloat(int hash, float value) ;

    public void SetFloat(string name, float value);

    public void SetBool(int hash, bool value);

    public void SetBool(string name, bool value);
  }
}
