using Cysharp.Threading.Tasks;


namespace LR.Manager.Store
{
  public interface IStoreService : IAchievementRegister, ILeaderBoardService
  {
    public void OnDestroy();
  }
}
