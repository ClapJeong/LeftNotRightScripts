using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static LR.Manager.GameDataManager.IDifficultyService;
using static LR.Manager.Store.ILeaderBoardService;



// PC SDK 3.0 모듈 별 Using 구문이 필요합니다.
using static Stove.PCSDK.Base;
using static Stove.PCSDK.GameSupport;
/*
using static Stove.PCSDK.IAP;		// IAPSDK_NET 연동시
*/

namespace LR.Manager.Store
{
  public class StoveService : MonoBehaviour, IStoreService
  {    
    [System.Serializable]
    public struct IDT
    {
      public string GameID;
      public string ApplicationiKey;
    }

    public IDT DemoT;
    public IDT ReleaseT;

    private readonly float checkInterval = 3.0f;
    private bool isInitialized;
    private readonly CTSContainer checkCTS = new();
    private readonly string RankKeyFormat = "{0}_{1}";

    private static readonly Dictionary<string, Sprite> cachedPortraits = new();

    #region StoveBase
    public IStoreService Initialize(bool isDemo)
    {
      checkCTS.Cancel();
      checkCTS.Create();
      var token = checkCTS.token;
      CheckUpdateAsync(token).Forget();

      var initParam = new StovePCInitializeParamEx2
      {
        environment = "LIVE",
        gameId = isDemo ? DemoT.GameID : ReleaseT.GameID,
        applicationKey = isDemo ? DemoT.ApplicationiKey : ReleaseT.ApplicationiKey,
        waitTimeMillisec = 60000,
        launchLauncher = true
      };

      // 런처 확인
      Base_RestartAppIfNecessaryAsyncEx2(initParam, (callbackResult, restart) =>
      {
        PrintCallbackResult(callbackResult);

        if (!callbackResult.result.IsSuccessful())
        {
          Debug.LogError("RestartAppIfNecessary failed.");

          PrintCallbackResult(callbackResult);

          Debug.Log($"env={initParam.environment} " +
            $"gameId={initParam.gameId} " +
            $"len={initParam.gameId?.Length} " +
            $"appKey ={initParam.applicationKey} " +
            $"len={initParam.applicationKey?.Length}");
        }

        if (restart)
        {
          Application.Quit();
          return;
        }

        Base_InitializeEx(cbResult =>
        {
          PrintCallbackResult(cbResult);
          isInitialized = cbResult.result.IsSuccessful();
        });
      });

      return this;
    }

    // 모듈 통합 정리를 위한 UnInitialize 메소드 작성
    public void UnInitialize()
    {
      Result result;

      checkCTS.Cancel();

      result = Base_UnInitialize();
      PrintResult(result);

      isInitialized = false;
    }

    public void PrintResult(Result r)
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendLine("# Result");
      sb.AppendLine($" - Result.sdkName : {r.sdkName}");
      sb.AppendLine($" - Result.methodCode : {r.methodCode}");
      sb.AppendLine($" - Result.resultCode : {r.resultCode}");
      sb.AppendLine($" - Result.exceptionMessage : {r.exceptionMessage}");

      Debug.Log(sb.ToString());
    }

    // CallbackResult 구조체 출력 메서드
    public void PrintCallbackResult(CallbackResult cr)
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendLine("# CallbackResult");
      sb.AppendLine($" - CallbackResult.Result.sdkName : {cr.result.sdkName}");
      sb.AppendLine($" - CallbackResult.Result.methodCode : {cr.result.methodCode}");
      sb.AppendLine($" - CallbackResult.Result.resultCode : {cr.result.resultCode}");
      sb.AppendLine($" - CallbackResult.Result.exceptionMessage : {cr.result.exceptionMessage}");
      sb.AppendLine($" - CallbackResult.errorMessage : {cr.errorMessage}");
      sb.AppendLine($" - CallbackResult.externalError : {cr.externalError}");

      Debug.Log(sb.ToString());
    }

    private async UniTask CheckUpdateAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();
          Base_RunCallback();

          await UniTask.WaitForSeconds(checkInterval);
        }
      }
      catch (OperationCanceledException) { }
    }

    private void OnDestroy()
    {
      queueCTS.Dispose();
      checkCTS.Dispose();

      if (isInitialized)
      {
        GameSupport_UnInitialize();
        Base_UnInitialize();
      }
    }
    #endregion

    #region IStoreService
    public void ResetAll()
    {
      if (!isInitialized)
        return;

      var result = GameSupport_Initialize();
      if (!result.IsSuccessful())
        return;
    }

    public void SetData(string key, int value)
    {
      if (!isInitialized)
        return;

      var result = GameSupport_Initialize();
      if (!result.IsSuccessful())
        return;

      GameSupport_ModifyStat(key, value, null);
    }

    public void SetAchievement(string key)
    {
      if (!isInitialized)
        return;

      var result = GameSupport_Initialize();
      if (!result.IsSuccessful())
        return;
    }

    public void UpdateDemoAchievementData(List<int> clearChapters, int normalPerfectCount, int hardPerfectCount)
    {
      if (!isInitialized)
        return;

      foreach (var chapter in clearChapters)
        SetData(string.Format(StoreKeys.StageClearFormat, chapter), 1);

      SetData(StoreKeys.NormalPerfect, normalPerfectCount);
      SetData(StoreKeys.HardPerfect, hardPerfectCount);
    }

    public void UpdateHardModeAchievement(int hardClearCount)
    {
      SetData(StoreKeys.HardModeClearCount, hardClearCount);
    }

    public async UniTask InitializeAsync(uint id)
    {
      await UniTask.CompletedTask;
    }

    void IStoreService.OnDestroy()
    {
      
    }
    #endregion

    #region Leaderboard

    private struct ScoreSubmitSet
    {
      public int StageIndex;
      public IDifficultyService.Difficulty Difficulty;
      public int Score;
    }

    private readonly Queue<ScoreSubmitSet> scoreSubmitQueue = new();
    private readonly float queueUploadTerm = 5f;
    private readonly CTSContainer queueCTS = new();
    private bool isQueueRunning;

    private readonly UnityEvent<string> onDelaySubmitComplete = new();
    private readonly List<string> newScoreRecord = new();

    public async UniTask<RankData[]> GetLeaderboardEntriesAsync(string key, int min, int max)
    {
      await UniTask.CompletedTask;
      return Array.Empty<RankData>();
    }

    public async UniTask<RankData[]> GetLeaderboardEntriesAsync(string key, int range)
    {
      await UniTask.CompletedTask;
      return Array.Empty<RankData>();

      //if (!isInitialized)
      //  return new();

      //if (!GameSupport_Initialize().IsSuccessful())
      //  return new();

      //var pageSize = range * 2 + 1;

      //async UniTask<(StovePCRank[] ranks, uint total)> RequestPage(uint pageIndex, bool includeMyRank)
      //{
      //  var tcs = new UniTaskCompletionSource<(StovePCRank[], uint)>();

      //  StovePCRankParams param = new()
      //  {
      //    leaderboardId = key,
      //    pageIndex = pageIndex,
      //    pageSize = (uint)pageSize,
      //    includeMyRank = includeMyRank
      //  };

      //  GameSupport_Rank(param, (cb, ranks, total) =>
      //  {
      //    if (!cb.result.IsSuccessful() || ranks == null)
      //      tcs.TrySetResult((Array.Empty<StovePCRank>(), total));
      //    else
      //      tcs.TrySetResult((ranks, total));
      //  });

      //  return await tcs.Task;
      //}

      //// 1. 내 랭크 획득
      //var (myResult, totalCount) = await RequestPage(1, true);
      //if (myResult.Length == 0)
      //  return new();

      //var myRank = myResult[0];
      //uint targetPage = (myRank.rank - 1) / (uint)pageSize + 1;

      //// 2. 필요한 페이지들 조회
      //var pageMap = new Dictionary<uint, StovePCRank[]>();

      //var current = await RequestPage(targetPage, false);
      //pageMap[targetPage] = current.ranks;

      //var indexInPage = (int)((myRank.rank - 1) % (uint)pageSize);

      //if (indexInPage < range && targetPage > 1)
      //{
      //  var prev = await RequestPage(targetPage - 1, false);
      //  pageMap[targetPage - 1] = prev.ranks;
      //}

      //if (indexInPage >= pageSize - range && targetPage * (uint)pageSize < totalCount)
      //{
      //  var next = await RequestPage(targetPage + 1, false);
      //  pageMap[targetPage + 1] = next.ranks;
      //}

      //// 3. 페이지 합치기
      //var merged = new List<StovePCRank>();

      //foreach (var pair in pageMap.OrderBy(x => x.Key))
      //  merged.AddRange(pair.Value);

      //// 중복 제거
      //merged = merged
      //    .GroupBy(x => x.rank)
      //    .Select(x => x.First())
      //    .OrderBy(x => x.rank)
      //    .ToList();

      //// 4. 내 기준 ±range 추출
      //int myIndex = merged.FindIndex(x => x.rank == myRank.rank);
      //if (myIndex < 0)
      //  return new();

      //int start = Math.Max(0, myIndex - range);
      //int end = Math.Min(merged.Count - 1, myIndex + range);

      //var result = new List<RankData>();

      //for (int i = start; i <= end; i++)
      //{
      //  var rank = merged[i];

      //  result.Add(new RankData
      //  {
      //    IsMine = rank.rank == myRank.rank,
      //    Name = rank.nickname,
      //    Rank = (int)rank.rank,
      //    Score = rank.score,
      //    Portrait = await GetPortraitAsync(rank.profileImage)
      //  });
      //}

      //return result;
    }

    public async UniTask<bool> QueueScoreSubmitAsync(int stageIndex, IDifficultyService.Difficulty difficulty, int score)
    {
      if (!isInitialized)
        return false;

      if (!GameSupport_Initialize().IsSuccessful())
        return false;

      var difficultyString = difficulty.ToString().ToUpper();
      var statId = string.Format(RankKeyFormat, stageIndex, difficultyString);

      var tcs = new UniTaskCompletionSource<bool>();

      GameSupport_ModifyStat(statId, score, (cb, stat) =>
      {
        if (cb.result.IsSuccessful())
        {
          AddNewScore(string.Empty);
          tcs.TrySetResult(true);
        }
        else
        {
          scoreSubmitQueue.Enqueue(new ScoreSubmitSet
          {
            StageIndex = stageIndex,
            Difficulty = difficulty,
            Score = score
          });

          if (!isQueueRunning)
            UpdateQueueAsync().Forget();

          tcs.TrySetResult(false);
        }
      });

      return await tcs.Task;
    }

    public async UniTask<int> GetExistScoreAsync(int stageIndex, IDifficultyService.Difficulty difficulty)
    {
      if (!isInitialized)
        return -1;

      if (!GameSupport_Initialize().IsSuccessful())
        return -1;

      var statId = string.Format(
          RankKeyFormat,
          stageIndex,
          difficulty.ToString().ToUpper());

      var tcs = new UniTaskCompletionSource<int>();

      GameSupport_Stat(statId, (cb, stat) =>
      {
        if (cb.result.IsSuccessful())
          tcs.TrySetResult(stat.currentValue);
        else
          tcs.TrySetResult(-1);
      });

      return await tcs.Task;
    }

    private async UniTask UpdateQueueAsync()
    {
      try
      {
        var token = queueCTS.token;
        isQueueRunning = true;

        while (scoreSubmitQueue.Count > 0)
        {
          token.ThrowIfCancellationRequested();

          var set = scoreSubmitQueue.Peek();

          var tcs = new UniTaskCompletionSource<bool>();

          var statId = string.Format(
              RankKeyFormat,
              set.StageIndex,
              set.Difficulty.ToString().ToUpper());

          GameSupport_ModifyStat(statId, set.Score, (cb, stat) =>
          {
            tcs.TrySetResult(cb.result.IsSuccessful());
          });

          if (await tcs.Task)
          {
            AddNewScore(string.Empty);
            scoreSubmitQueue.Dequeue();
            onDelaySubmitComplete.Invoke(string.Empty);
          }

          await UniTask.WaitForSeconds(
              queueUploadTerm,
              true,
              PlayerLoopTiming.Update,
              token);
        }
      }
      catch (OperationCanceledException)
      {
      }
      finally
      {
        isQueueRunning = false;
      }
    }

    public async UniTask CacheLeaderboardAsync(string key)
    {
      await UniTask.CompletedTask;
    }

    public async UniTask<int> GetRankCount(string key)
    {
      await UniTask.CompletedTask;
      return 0;
    }


    public async UniTask<Sprite> GetPortraitAsync(string url)
    {
      if (cachedPortraits.TryGetValue(url, out var sprite))
        return sprite;

      sprite = await LoadSprite(url);

      if (sprite != null)
        cachedPortraits[url] = sprite;

      return sprite;
    }

    private static async UniTask<Sprite> LoadSprite(string url)
    {
      if (string.IsNullOrEmpty(url))
        return null;

      using var req = UnityWebRequestTexture.GetTexture(url);
      await req.SendWebRequest();

      if (req.result != UnityWebRequest.Result.Success)
        return null;

      var tex = DownloadHandlerTexture.GetContent(req);

      return Sprite.Create(
          tex,
          new Rect(0, 0, tex.width, tex.height),
          new Vector2(0.5f, 0.5f));
    }

    private void AddNewScore(string key)
    {
      if (!newScoreRecord.Contains(key))
        newScoreRecord.Add(key);
    }

    public void SubscribeOnDelaySubmitComplete(UnityAction<string> unityAction)
        => onDelaySubmitComplete.AddListener(unityAction);

    public void UnsubscribeOnDelaySubmitComplete(UnityAction<string> unityAction)
        => onDelaySubmitComplete.RemoveListener(unityAction);

    public bool IsNewScoreExist(string key)
        => newScoreRecord.Contains(key);

    public void DeleteNewScore(string key)
        => newScoreRecord.Remove(key);

    public async UniTask<bool> QueueScoreSubmitAsync(string key, int score, int stageIndex, int[] detail)
    {
      await UniTask.CompletedTask;
      return false;
    }

    public async UniTask<int> GetExistScoreAsync(string key)
    {
      await UniTask.CompletedTask;
      return 0;
    }

    #endregion
  }
}