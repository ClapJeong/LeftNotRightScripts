using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Manager.Store
{
  public interface ILeaderBoardService
  {
    public struct RankData
    {
      public bool IsMine;
      public string Name;
      public int Score;
      public Sprite Portrait;
      public int Rank;
      public int[] Deatils;
    }

    public UniTask CacheLeaderboardAsync(string key);

    public UniTask<int> GetRankCount(string key);

    public UniTask<RankData[]> GetLeaderboardEntriesAsync(string key, int userRange);
    public UniTask<RankData[]> GetLeaderboardEntriesAsync(string key, int min, int max);

    public UniTask<bool> QueueScoreSubmitAsync(string key, int score, int stageIndex, int[] detail = default);

    public UniTask<int> GetExistScoreAsync(string key);

    public void SubscribeOnDelaySubmitComplete(UnityAction<string> unityAction);
    public void UnsubscribeOnDelaySubmitComplete(UnityAction<string> unityAction);

    public bool IsNewScoreExist(string key);
    public void DeleteNewScore(string key);
  }
}
