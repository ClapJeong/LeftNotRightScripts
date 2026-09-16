using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static LR.Manager.Store.ILeaderBoardService;

namespace LR.Manager.Store
{
  public class SteamService : IStoreService
  {
    private struct ScoreSubmitSet
    {
      public Leaderboard Leaderboard;
      public int Score;
      public string key;
      public int StageIndex;
      public int[] deatil;
    }

    private readonly CTSContainer waitCTS = new();

    private bool IsInitialized
      => SteamClient.IsValid;
   
    private readonly Queue<ScoreSubmitSet> scoreSubmitQueue = new();
    private readonly float queueUploadTerm = 5.0f;
    private readonly CTSContainer queueCTS = new();
    private bool isQueueRunning = false;

    private readonly UnityEvent<string> onDelaySubmitComplete = new();

    private readonly List<string> newScoreRecord = new();
    private readonly Dictionary<string, Leaderboard> cachedLeaderboards = new();


    public SteamService()
    {

    }

    public IStoreService Initialize(uint id)
    {
      InitializeAsync(id).Forget();
      return this;
    }

    private async UniTask InitializeAsync(uint id)
    {
      try
      {
        SteamClient.Init(id);
        UnityEngine.Debug.Log("Steam Initialize Starting...");
        await UniTask.CompletedTask;
      }
      catch (System.Exception e)
      {
        UnityEngine.Debug.Log($"Steam Initialize Fail: {e}");
      }
    }

    public void ResetAll()
    {
      if (!IsInitialized)
        return;

      SteamUserStats.ResetAll(true); // true = wipe achivements too
      SteamUserStats.StoreStats();
    }

    public void SetAchievement(string key)
    {
      if (!IsInitialized)
        return;

      var ach = new Steamworks.Data.Achievement(key);
      ach.Trigger();

      SteamUserStats.StoreStats();
    }

    public void SetData(string key, int value)
    {
      if (!IsInitialized)
        return;

      var existValue = SteamUserStats.GetStatInt(key);
      if (existValue < value)
      {
        var result = SteamUserStats.SetStat(key, value);
        SteamUserStats.StoreStats();
      }
    }

    public void UpdateDemoAchievementData(List<int> clearChapters, int normalPerfectCount, int hardPerfectCount)
    {
      if (!IsInitialized)
        return;

      foreach (var chapter in clearChapters)
        SetAchievement(string.Format(StoreKeys.StageClearFormat, chapter));

      SetData(StoreKeys.NormalPerfect, normalPerfectCount);
      SetData(StoreKeys.HardPerfect, hardPerfectCount);
    }

    public void UpdateHardModeAchievement(int hardClearCount)
    {
      SetData(StoreKeys.HardModeClearCount, hardClearCount);
    }

    public void OnDestroy()
    {
      queueCTS.Dispose();
      waitCTS.Dispose();
      SteamClient.Shutdown();
    }

    #region LeaderBoard
    public async UniTask CacheLeaderboardAsync(string key)
    {
      var leaderboard = await GetLeaderboardAsync(key);
      if (leaderboard != null && leaderboard.HasValue)
        cachedLeaderboards[key] = leaderboard.Value;
    }

    public async UniTask<int> GetRankCount(string key)
    {
      var leaderBoard = await GetLeaderboardAsync(key);
      if (leaderBoard.HasValue)
        return leaderBoard.Value.EntryCount;
      else
        return 0;
    }

    private async UniTask<Leaderboard?> GetLeaderboardAsync(string key, float waitDelay = 3f)
    {
      //UnityEngine.Debug.Log($"{key} leaderboard Load Begin");
      if (cachedLeaderboards.TryGetValue(key, out var existLeaderboard))
        return existLeaderboard;

      waitCTS.Cancel();
      waitCTS.Create();
      var token = waitCTS.token;

      var leaderboardTask = SteamUserStats.FindOrCreateLeaderboardAsync(key, LeaderboardSort.Ascending, LeaderboardDisplay.Numeric).AsUniTask();
      var timeoutTask = UniTask.Delay(
          TimeSpan.FromSeconds(waitDelay),
          cancellationToken: token);

      var (completed, leaderboard) = await UniTask.WhenAny(leaderboardTask, timeoutTask);

      //UnityEngine.Debug.Log($"{key} leaderboard: {leaderboard.HasValue}");
      return completed ? leaderboard : null;
    }

    public async UniTask<RankData[]> GetLeaderboardEntriesAsync(string key, int userRange)
    {
      var leaderboard = await GetLeaderboardAsync(key);
      if (!leaderboard.HasValue)
        return Array.Empty<RankData>();

      var playerEntry = await leaderboard.Value.GetScoresForUsersAsync(new[] { SteamClient.SteamId });
      if (playerEntry == null || playerEntry.Length == 0)
        return await GetLeaderboardEntriesAsync(key, 1, 5);
      else
      {
        var entries = await leaderboard.Value.GetScoresAroundUserAsync(-userRange, userRange);

        if (entries == null)
          return Array.Empty<RankData>();

        var datas = new RankData[entries.Length];
        for (int i = 0; i < entries.Length; i++)
        {
          var entry = entries[i];
          var portrait = await GetPortraitAsync(entry.User.Id);
          datas[i] = new()
          {
            IsMine = entry.User.Id == SteamClient.SteamId,
            Name = entry.User.Name,
            Rank = entry.GlobalRank,
            Score = entry.Score,
            Portrait = portrait,
            Deatils = entry.Details,
          };
        }

        return datas;
      }
    }


    public async UniTask<RankData[]> GetLeaderboardEntriesAsync(string key, int min, int max)
    {
      var leaderboard = await GetLeaderboardAsync(key);
      if (!leaderboard.HasValue)
        return Array.Empty<RankData>();

      var entries = await leaderboard.Value.GetScoresAsync(max - min + 1, min);
      if (entries == null)
        return Array.Empty<RankData>();

      var datas = new RankData[entries.Length];
      for (int i = 0; i < entries.Length; i++)
      {
        var entry = entries[i];
        var portrait = await GetPortraitAsync(entry.User.Id);
        datas[i] = new()
        {
          IsMine = entry.User.Id == SteamClient.SteamId,
          Name = entry.User.Name,
          Rank = entry.GlobalRank,
          Score = entry.Score,
          Portrait = portrait,
          Deatils = entry.Details,
        };
      }

      return datas;
    }

    public async UniTask<Sprite> GetPortraitAsync(SteamId steamId)
    {
      SteamFriends.RequestUserInformation(steamId, false);

      Image? image = null;

      // 아바타가 캐시될 때까지 대기
      while (!image.HasValue)
      {
        image = await SteamFriends.GetLargeAvatarAsync(steamId);

        if (!image.HasValue)
          await UniTask.DelayFrame(1);
      }

      var img = image.Value;

      var tex = new Texture2D(
          (int)img.Width,
          (int)img.Height,
          TextureFormat.RGBA32,
          false);

      for (int x = 0; x < img.Width; x++)
      {
        for (int y = 0; y < img.Height; y++)
        {
          var p = img.GetPixel(x, y);

          tex.SetPixel(
              x,
              (int)img.Height - 1 - y,
              new Color32(p.r, p.g, p.b, p.a));
        }
      }

      tex.Apply();

      return Sprite.Create(
          tex,
          new Rect(0, 0, tex.width, tex.height),
          new Vector2(0.5f, 0.5f));
    }

    public async UniTask<bool> QueueScoreSubmitAsync(string key, int score, int stageIndex, int[] detail = default)
    {
      var leaderboard = await GetLeaderboardAsync(key);
      if (leaderboard.HasValue)
      {
        var result = await leaderboard.Value.ReplaceScore(score, detail);
        //Debug.Log("Score Submit!");

        if (result.HasValue)
        {
          AddNewScore(key);
        }
        else
        {
          //  Debug.Log("Score Submit Fail!");
          scoreSubmitQueue.Enqueue(new()
          {
            Leaderboard = leaderboard.Value,
            Score = score,
            key = key,
            StageIndex = stageIndex,
            deatil = detail,
          });

          if (!isQueueRunning)
            UpdateQueueAsync().Forget();
        }

        return result.HasValue;
      }
      else
        return false;
    }

    public async UniTask<int> GetExistScoreAsync(string key)
    {
      var leaderboard = await GetLeaderboardAsync(key);
      if (!leaderboard.HasValue)
        return -1;

      var entries = await leaderboard.Value.GetScoresAroundUserAsync(0);
      if (entries == null || entries.Length == 0)
        return -1;

      return entries[0].Score;
    }

    public bool IsNewScoreExist(string key)
    {
      return newScoreRecord.Contains(key);
    }

    public void DeleteNewScore(string key)
    {
      if (IsNewScoreExist(key))
      {
        //UnityEngine.Debug.Log($"score remove -[ {stageIndex} ]");
        newScoreRecord.Remove(key);
      }        
    }

    private void AddNewScore(string key)
    {
      if(!IsNewScoreExist(key))
      {
        newScoreRecord.Add(key);
        //UnityEngine.Debug.Log($"new score add! [ {stageIndex} ]");
      }  
    }

    public void SubscribeOnDelaySubmitComplete(UnityAction<string> unityAction)
      => onDelaySubmitComplete.AddListener(unityAction);
    public void UnsubscribeOnDelaySubmitComplete(UnityAction<string> unityAction)
      => onDelaySubmitComplete.RemoveListener(unityAction);
    #endregion

    private async UniTask UpdateQueueAsync()
    {
      try
      {
        UnityEngine.Debug.Log("Score Upload Queue Begin");
        var token = queueCTS.token;
        isQueueRunning = true;
        while(scoreSubmitQueue.Count > 0)
        {
          token.ThrowIfCancellationRequested();

          var set = scoreSubmitQueue.Peek();
          var submit = await set.Leaderboard.SubmitScoreAsync(set.Score);
          if (submit.HasValue)
          {
            AddNewScore(set.key);

            scoreSubmitQueue.Dequeue();
            onDelaySubmitComplete?.Invoke(set.key);
          }            

          await UniTask.WaitForSeconds(queueUploadTerm, true, PlayerLoopTiming.Update, token);
        }
        isQueueRunning = false;
        UnityEngine.Debug.Log("Score Upload Queue Complete");
      }
      catch (OperationCanceledException) { }
    }
  }
}
