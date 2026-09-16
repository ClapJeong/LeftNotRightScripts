using Cysharp.Threading.Tasks;
using System.Threading;

namespace LR.Manager.GameDataManager
{
  public interface IGameModeService
  {
    public enum GameMode
    {
      None,
      SpeedRun,
      Demo,
    }
    public bool IsSpeedRun { get; }

    public GameMode SetGameMode(GameMode gameMode);

    public GameMode GetCurrentGameMode();    
  }
}