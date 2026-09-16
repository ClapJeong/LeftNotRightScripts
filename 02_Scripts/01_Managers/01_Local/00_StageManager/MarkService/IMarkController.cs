using Cysharp.Threading.Tasks;

namespace LR.Manager.Stage.Marking
{
  public interface IMarkController
  {
    public UniTask InitializeAsync();

    public void ClearWallHitPaints();

    public void ClearDeadPaints();

    public void ShowDeadPaints();
  }
}
