using Cysharp.Threading.Tasks;
using LR.Manager.Store;
using Zenject;


namespace LR.Manager.Stage
{
  public class LeaderboardController
  {
    [Inject] public ILeaderBoardService leaderBoardService;

    public async UniTask SubmitStageLeaderboardAsync(string key, int score, int stageIndex)
    {
      //UnityEngine.Debug.Log("submit begin");

      var existScore = await leaderBoardService.GetExistScoreAsync(key);

      bool isBetterScore = existScore < 0 || score < existScore;

      if (isBetterScore)
        await leaderBoardService.QueueScoreSubmitAsync(key, score, stageIndex);
      else
        leaderBoardService.DeleteNewScore(key);
    }

    public async UniTask SubmitSpeedrunAsync(int score, int restartCount)
    {
      UnityEngine.Debug.Log("speedrun Submit begin");
      var fastKey = StoreKeys.FastestSpeedRun;
      var safeKey = StoreKeys.SafestSpeedRun;

      await leaderBoardService.CacheLeaderboardAsync(fastKey);
      await leaderBoardService.CacheLeaderboardAsync(safeKey);

      var existFastScore = await leaderBoardService.GetExistScoreAsync(fastKey);
      bool isFaster = existFastScore < 0 || score < existFastScore;
      if (isFaster)
        UnityEngine.Debug.Log($"faster! {score} < {existFastScore}");
      await leaderBoardService.QueueScoreSubmitAsync(fastKey, score, 0, new int[] { restartCount });
      else
        leaderBoardService.DeleteNewScore(fastKey);

      var existSafestScore = await leaderBoardService.GetExistScoreAsync(safeKey);
      bool isSafer = existSafestScore < 0 || restartCount < existSafestScore;
      if (isSafer)
        UnityEngine.Debug.Log($"safer! {restartCount} < {existSafestScore}");
      //await leaderBoardService.QueueScoreSubmitAsync(safeKey, restartCount, 0, new int[] { score });
      else
        leaderBoardService.DeleteNewScore(safeKey);
    }

  }
}
