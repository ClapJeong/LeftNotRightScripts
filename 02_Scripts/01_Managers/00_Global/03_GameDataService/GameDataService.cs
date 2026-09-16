using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using static LR.Manager.GameDataManager.IDifficultyService;

namespace LR.Manager.GameDataManager
{
  public class GameDataService : 
    IGameDataIOController, 
    IGameModeService,
    IGameDataProvider,
    ISpeedRunDataProvider,
    IGameDataSetter,
    IDifficultyService,
    IRedPillModeService,
    IPracticeService
  {
    private readonly IResourceManager resourceManager;
    private readonly AddressableKeySO addressableKeySO;
    private readonly string GameDataPath;

    private GameData gameData;
    private IGameModeService.GameMode gameMode = IGameModeService.GameMode.None;
    private IDifficultyService.Difficulty currentDifficulty = IDifficultyService.Difficulty.Normal;
    private readonly UnityEvent<IDifficultyService.Difficulty> onDifficultyChanged = new();
    private readonly UnityEvent<bool> onRedPillModeChanged = new();

    public int StageDataCount { get; protected set; }
    public bool IsRedPillEnable => isRedPillMode;

    private int selectedChapter;
    private int selectedStage;

    private bool isRedPillMode = false;

    public GameDataService(
      IResourceManager resourceManager,
      AddressableKeySO addressableKeySO)
    {
      this.resourceManager = resourceManager;
      this.addressableKeySO = addressableKeySO;
      GameDataPath = Application.persistentDataPath + "/GameData.json";

      currentDifficulty = (IDifficultyService.Difficulty)PlayerPrefs.GetInt(PlayerPrefsName.Difficulty, (int)IDifficultyService.Difficulty.Normal);

      isRedPillMode = PlayerPrefs.GetInt(PlayerPrefsName.RedPillMode, 0) == 0 ? false : true;
    }

    #region IGameModeService
    public bool IsSpeedRun
      => gameMode == IGameModeService.GameMode.SpeedRun;

    public IGameModeService.GameMode SetGameMode(IGameModeService.GameMode gameMode)
      => this.gameMode = gameMode;

    public IGameModeService.GameMode GetCurrentGameMode()
      => gameMode;
    #endregion

    #region IGameDataIOController
    public async UniTask SaveDataAsync(CancellationToken token = default)
    {
      if (gameData == null)
        gameData = new GameData(GlobalManager.instance.GameDataVersion);

      var json = JsonUtility.ToJson(gameData);
      await File.WriteAllTextAsync(GameDataPath, json, token);
    }

    public void ResetData()
    {
      gameData = new GameData(GlobalManager.instance.GameDataVersion);
    }

    public async UniTask LoadDataAsync(bool resetData, CancellationToken token = default)
    {
      if (resetData)
      {
        ResetData();
        await SaveDataAsync();
      }
      else
      {
        var gameDataVersion = GlobalManager.instance.GameDataVersion;
        if (File.Exists(GameDataPath) == false)
        {
          gameData = new GameData(gameDataVersion);
        }
        else
        {
          var text = await File.ReadAllTextAsync(GameDataPath, token);
          gameData = JsonUtility.FromJson<GameData>(text);
          gameData.UpdateVersion(gameDataVersion);
        }
      }

      await CacheStageCount(resourceManager);
    }

    public void AddClearData(int chapter, int stage, IDifficultyService.Difficulty difficulty)
    {
      var existData = gameData.ClearDatas.FirstOrDefault(data => data.Chapter == chapter && data.Stage == stage);
      var difficultyIndex = (int)difficulty;
      if (existData != null)
      {
        existData.UpdateDifficulty(difficulty);
      }
      else
      {
        var newData = new GameData.ClearData(chapter, stage, difficultyIndex);
        gameData.ClearDatas.Add(newData);
      }
    }

    public void UpdatePerfectData(int chapter, int stage, IDifficultyService.Difficulty difficulty)
    {
      var existData = gameData.ClearDatas.FirstOrDefault(data => data.Chapter == chapter && data.Stage == stage);
      var difficultyIndex = (int)difficulty;
      existData?.UpdatePerfect(difficulty);
    }

    public void AddSpeedRunData(float time, int restartCount)
    {
      gameData.speedRunDatas.Add(new GameData.SpeedRunData(time, restartCount));
    }

    public void UpdateDemoPlayed(bool isDemoPlaying)
      => gameData.IsDemoPlayed = isDemoPlaying;

    public void UpdateSpeedrunResumeData(int chater, int stage)
    {
      //Debug.Log("Upate Resume Data");
      gameData.LastSpeedrunChapter = chater;
      gameData.LastSpeedrunStage = stage;
    }

    public void UpdateSpeedrunResumeTimeData(float time, int count)
    {
      //Debug.Log("Upate Resume Time Data");
      if (gameData.RunningSpeedrunData != null)
      {
        gameData.RunningSpeedrunData.Second = time;
        gameData.RunningSpeedrunData.RestartCount = count;
      }
      else
      {
        gameData.RunningSpeedrunData = new(time, count);
      }
    }

    public void ResetSpeedrunResumeData()
    {
      //Debug.Log("Reset Resume Data");
      gameData.LastSpeedrunChapter = 0;
      gameData.LastSpeedrunStage = 0;
      gameData.RunningSpeedrunData = null;
    }
    #endregion

    #region IGameDataProvider
    public int GetSelectedChapter()
      => selectedChapter;

    public int GetSelectedStage()
      => selectedStage;

    public int GetMaxClearIndex()
    {
      if (gameData == null)
        return 0;

      var maxClearData = gameData.ClearDatas.OrderByDescending(data => (data.Chapter - 1) * StageConst.StageUnit + data.Stage).FirstOrDefault();
      if (maxClearData == null)
        return 0;
      else
        return (maxClearData.Chapter - 1) * StageConst.StageUnit + maxClearData.Stage;
    }

    public bool IsVeryFirst()
      => gameData == null || gameData.ClearDatas.Count == 0;

    public bool IsAllClear()
      => gameData.ClearDatas.Count == StageDataCount;

    public bool IsStageExist(int chapter, int stage)
       => ((chapter - 1) * StageConst.StageUnit + stage) <= StageDataCount;

    public bool IsClearStage(int chapter, int stage, out IDifficultyService.Difficulty difficulty)
    {
      var existData = gameData.ClearDatas.FirstOrDefault(data => data.Chapter == chapter && data.Stage == stage);
      if(existData == null)
      {
        difficulty = Difficulty.Easy;
      }
      else
      {
        difficulty = existData.GetClearDifficulty();;
      }

      return existData != null;
    }

    public bool IsClearStageWithDifficulty(int chapter, int stage, IDifficultyService.Difficulty difficulty)
    {
      var existData = gameData.ClearDatas.FirstOrDefault(data => data.Chapter == chapter && data.Stage == stage);
      if (existData != null)
      {
        var clearDifficulty = existData.GetClearDifficulty();
        var targetIndex = (int)difficulty;
        var clearIndex = (int)clearDifficulty;
        return targetIndex <= clearIndex;
      }
      else
        return false;
    }

    public bool WasDemoPlayed()
      => gameData.IsDemoPlayed;

    public List<int> GetClearChaptes()
    {
      var clearChapters = new List<int>();
      var dictionary = new Dictionary<int, int>();
      foreach (var clearData in gameData.ClearDatas)
      {
        var clearDifficulty = clearData.GetClearDifficulty();
        if (clearDifficulty == Difficulty.Easy)
          continue;

        var chapter = clearData.Chapter;

        if (dictionary.ContainsKey(chapter))
        {
          dictionary[chapter]++;
          var count = dictionary[chapter];
          if (count == StageConst.StageUnit)
            clearChapters.Add(chapter);
        }
        else
          dictionary[chapter] = 1;
      }

      return clearChapters;
    }

    public int GetHardClearCount()
    {
      var count = 0;
      foreach(var clearData in gameData.ClearDatas)
        if (clearData.GetClearDifficulty() == Difficulty.Hard)
          count++;

      return count;
    }

    public bool IsPerfect(int chapter, int stage, IDifficultyService.Difficulty difficulty)
    {
      foreach(var clearData in gameData.ClearDatas)
      {
        if(chapter == clearData.Chapter && stage == clearData.Stage)
          return clearData.IsPerfect(difficulty);
      }

      return false;
    }

    public bool TryGetSpeedrunResumeData(out int chapter, out int stage)
    {
      chapter = gameData.LastSpeedrunChapter;
      stage = gameData.LastSpeedrunStage;
      return chapter > 0 && stage > 0 && gameData.RunningSpeedrunData != null;
    }

    public int GetPerfectClearCount(IDifficultyService.Difficulty difficulty)
    {
      var count = 0;
      foreach (var clearData in gameData.ClearDatas)
        if (clearData.IsPerfect(difficulty))
          count++;

      return count;
    }
    #endregion

    #region ISpeedRunDataProvider
    public GameData.SpeedRunData GetFastedSpeedRunData()
      => gameData.speedRunDatas.OrderBy(data => data.Second).FirstOrDefault() ?? new GameData.SpeedRunData(0.0f, 0);

    public GameData.SpeedRunData GetSafestSpeedRunData()
      => gameData.speedRunDatas.OrderBy(data => data.RestartCount).FirstOrDefault() ?? new GameData.SpeedRunData(0.0f, 0);

    public bool TryGetRunningData(out GameData.SpeedRunData data)
    {
      data = gameData.RunningSpeedrunData;
      return data != null;
    }
    #endregion

    #region IGameDataSetter
    public void SetSelectedStage(int chapter, int stage)
    {
      selectedChapter = chapter;
      selectedStage = stage;
    }

    public void ResetSelectedStage()
    {
      selectedChapter = -1;
      selectedStage = -1;
    }
    #endregion

    #region IDifficultyService
    public IDifficultyService.Difficulty CurrentDifficulty
      => currentDifficulty;

    public void SetDifficulty(IDifficultyService.Difficulty difficulty)
    {
      if (currentDifficulty == difficulty)
        return;

      onDifficultyChanged?.Invoke(difficulty);
      currentDifficulty = difficulty;

      PlayerPrefs.SetInt(PlayerPrefsName.Difficulty, (int)difficulty);
    }

    public void SubscribeOnDifficultyChanged(UnityAction<Difficulty> unityAction)
      => onDifficultyChanged.AddListener(unityAction);

    public void UnsubscribeOnDifficultyChanged(UnityAction<Difficulty> unityAction)
      =>onDifficultyChanged.RemoveListener(unityAction);
    #endregion

    #region IRedPillModeService
    public void EnableRedPillMode(bool mode)
    {
      isRedPillMode = mode;
      PlayerPrefs.SetInt(PlayerPrefsName.RedPillMode, isRedPillMode ? 1 : 0);

      onRedPillModeChanged?.Invoke(isRedPillMode);
    }

    public void SubscribeOnChanged(UnityAction<bool> onChanged)
      => onRedPillModeChanged.AddListener(onChanged);

    public void UnsubscribeOnChanged(UnityAction<bool> onChanged)
      => onRedPillModeChanged.RemoveListener(onChanged);
    #endregion

    #region IPracticeService
    public bool IsPractice
    {
      get
      {
        return isPractice;
      }
      set
      {
        isPractice = value;
      }
    }
    private bool isPractice = false;
    #endregion

    private async UniTask CacheStageCount(IResourceManager resourceManager)
    {
      var table = addressableKeySO;
      var stageLabel = table.Label.Stage;

      var stages = await resourceManager.LoadAssetsAsync(stageLabel);
      StageDataCount = stages.Count;
    }

    #region Debugging
    public void Debugging_RaiseClearData()
    {
      RaiseClearData();

      SaveDataAsync().Forget();
    }

    private void RaiseClearData()
    {
      var last = gameData.ClearDatas.OrderByDescending(data => (data.Chapter - 1) * StageConst.StageUnit + data.Stage).FirstOrDefault();
      if (last == null)
      {
        AddClearData(1, 1, currentDifficulty);
      }
      else if (((last.Chapter - 1) * StageConst.StageUnit + last.Stage) == StageDataCount)
        return;
      else
      {
        if (last.Stage == StageConst.StageUnit)
          AddClearData(last.Chapter + 1, 1, currentDifficulty);
        else
          AddClearData(last.Chapter, last.Stage + 1, currentDifficulty);
      }
    }

    public void Debugging_LowerClearData()
    {
      var last = gameData.ClearDatas.OrderByDescending(data => (data.Chapter - 1) * StageConst.StageUnit + data.Stage).FirstOrDefault();
      if (last == null)
        return;
      else gameData.ClearDatas.Remove(last);
      SaveDataAsync().Forget();
    }

    public void Debugging_ClearClearData()
    {
      gameData.ClearDatas.Clear();
      SaveDataAsync().Forget();
    }

    public void Debugging_MaxClearData()
    {
      gameData.ClearDatas.Clear();
      for (int i = 0; i < StageDataCount; i++)
        RaiseClearData();

      SaveDataAsync().Forget();
    }    
    #endregion
  }
}