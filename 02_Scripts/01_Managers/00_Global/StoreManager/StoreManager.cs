using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using static LR.Manager.GameDataManager.IDifficultyService;
using static LR.Manager.Store.ILeaderBoardService;

namespace LR.Manager.Store
{
  public enum StoreType
  {
    None,
    Steam,
    Stove,
  }
  public class StoreManager : 
    MonoBehaviour,
    IAchievementRegister,
    ILeaderBoardService,
    IStoreTypeProvider
  {
    [SerializeField] private StoreType storeType;
    [SerializeField] private bool IsDemo;
    [Header("[ Steam ]")]
    [SerializeField] private uint SteamDemoID;
    [SerializeField] private uint SteamReleaseID;    
    [Header("[ Stove ]")]
    [SerializeField] private StoveService stoveService;

    private IStoreService storeService;

    public StoreType StoreType => storeType;

    private void Awake()
    {
      storeService = storeType switch
      {
        StoreType.None => null,
        StoreType.Steam => new SteamService().Initialize(IsDemo ? SteamDemoID : SteamReleaseID),
        StoreType.Stove => stoveService.Initialize(IsDemo),
        _ => throw new System.NotImplementedException(),
      };
    }

    public void ResetAll()
    {
      storeService?.ResetAll();
    }

    public void SetAchievement(string key)
    {
      if (GlobalManager.instance.Demo)
        return;      

      storeService?.SetAchievement(key);
    }

    public void SetData(string key, int value)
    {
      if (GlobalManager.instance.Demo)
        return;

      storeService?.SetData(key, value);
    }

    private void OnValidate()
    {
      if (stoveService != null)
        stoveService.gameObject.SetActive(storeType == StoreType.Stove);
    }

    #region ILeaderBoardService
    public async UniTask<RankData[]> GetLeaderboardEntriesAsync(string key, int userRange)
    {
      if (storeService != null)
        return await storeService.GetLeaderboardEntriesAsync(key, userRange);
      else
        return Array.Empty<RankData>();
    }

    public async UniTask<RankData[]> GetLeaderboardEntriesAsync(string key, int min, int max)
    {
      if(storeService != null)
        return await storeService.GetLeaderboardEntriesAsync(key, min, max);
      else
        return Array.Empty<RankData>();
    }

    public async UniTask<bool> QueueScoreSubmitAsync(string key, int score, int stageIndex, int[] detail = default)
    {
      if (storeService != null)
        return await storeService.QueueScoreSubmitAsync(key, score, stageIndex, detail);
      else
        return false;
    }
    #endregion

    public void UpdateDemoAchievementData(List<int> clearChapters, int normalPerfectCount, int hardPerfectCount)
    {
      if (GlobalManager.instance.Demo)
        return;

      storeService?.UpdateDemoAchievementData(clearChapters, normalPerfectCount, hardPerfectCount);
    }

    public void UpdateHardModeAchievement(int hardClearCount)
    {
      if (GlobalManager.instance.Demo)
        return;

      storeService?.UpdateHardModeAchievement(hardClearCount);
    }

    private void OnDestroy()
    {
      storeService?.OnDestroy();
    }

    public async UniTask<int> GetExistScoreAsync(string key)
    {
      if (storeService != null)
        return await storeService.GetExistScoreAsync(key);
      else
        return -1;
    }

    public void SubscribeOnDelaySubmitComplete(UnityAction<string> unityAction)
      => storeService?.SubscribeOnDelaySubmitComplete(unityAction);

    public void UnsubscribeOnDelaySubmitComplete(UnityAction<string> unityAction)
      => storeService?.UnsubscribeOnDelaySubmitComplete(unityAction);

    public bool IsNewScoreExist(string key)
    {
      if(storeService != null)
        return storeService.IsNewScoreExist(key);
      else
        return false;
    }

    public void DeleteNewScore(string key)
      => storeService?.DeleteNewScore(key);

    public async UniTask CacheLeaderboardAsync(string key)
    {
      if (storeService != null)
        await storeService.CacheLeaderboardAsync(key);
    }

    public async UniTask<int> GetRankCount(string key)
    {
      if (storeService != null)
        return await storeService.GetRankCount(key);
      else
        return 0;
    }
  }
}
