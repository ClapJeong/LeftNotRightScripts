using Cysharp.Threading.Tasks;
using LR.Manager.Analystic;
using LR.Manager.SpeedRun;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.Manager.Scene;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Zenject;
using LR.Manager.GameDataManager;
using Steamworks;

public partial class GlobalManager : MonoBehaviour
{
  public static GlobalManager instance;

  [field: SerializeField] public TableContainer Table { get; private set; }
  [field: SerializeField] public LocaleService LocaleService { get; private set; }
  [Inject] private readonly DiContainer diContainer = null;
  [Inject] private readonly SceneService sceneProvider = null;
  [Inject] public IUISubmitController UIsubmitController { get; private set; }
  [Inject] public LazyInject<GameDataService> GameDataService { get; private set; }
  private SpeedRunManager speedRunManager;

  [Header("[ Debugging ]")]
  public bool Demo = false;
  public bool PlayVeryFirst = false;
  public bool EnableAllDialogue = false;
  public int GameVersion = 1;
  public int GameDataVersion = 0;
  public bool ResetGameData = false;
  public bool SkipSpeedRun = false;
  public KeyCode SampleStageKeyCode = KeyCode.F12;

  private readonly CompositeDisposable disposables = new();
  private AnalysticManager analysticManager;

  [Header("[ Test Values]")]
  public string LeaderBoardKey;
  public int LeaderBoardValue;
  [ContextMenu("[ LeaderboardTest ]")]
  public async void LeaderboardTest()
  {
    var key = "1_Easy";
    var leaderBoard = await SteamUserStats.FindLeaderboardAsync(key);
    var update = await leaderBoard.Value.ReplaceScore(LeaderBoardValue);
    Debug.Log(update.HasValue);
  }

  [ContextMenu("[ CreateLeaderBoards ]")]
  public async void CreateLeaderboards()
  {
    for (int i = 0; i < 96; i++)
    {
      var index = i + 1;
      var easyKey = $"{index}_Easy";
      var normalKey = $"{index}_Normal";
      var hardKey = $"{index}_Hard";
      await SteamUserStats.FindOrCreateLeaderboardAsync(easyKey, Steamworks.Data.LeaderboardSort.Ascending, Steamworks.Data.LeaderboardDisplay.Numeric);
      await SteamUserStats.FindOrCreateLeaderboardAsync(normalKey, Steamworks.Data.LeaderboardSort.Ascending, Steamworks.Data.LeaderboardDisplay.Numeric);
      var lehu = await SteamUserStats.FindOrCreateLeaderboardAsync(hardKey, Steamworks.Data.LeaderboardSort.Ascending, Steamworks.Data.LeaderboardDisplay.Numeric);
      Debug.Log(lehu.HasValue);
    }
  }


  [ContextMenu("[ Reset Achievement ]")]
  public void ResetAchievement()
  {
    if (!Application.isPlaying)
      return;

    diContainer.Resolve<IAchievementRegister>().ResetAll();
  }


  private void Awake()
  {
    if(instance == null)
    {
      DontDestroyOnLoad(gameObject);
      instance = this;

      //analysticManager = new();

      var isFullScreen = PlayerPrefs.GetInt(PlayerPrefsName.FullScreen) == 0;

      //var isFullScreen = IsFullScreen();

      var mode = isFullScreen ? FullScreenMode.ExclusiveFullScreen
                              : FullScreenMode.FullScreenWindow;
      var width = Screen.currentResolution.width;
      var height = Screen.currentResolution.height;

#if !UNITY_EDITOR
      DisableMouse();
#endif
    }
    else
    {
      Destroy(gameObject);
    }
  }

#if UNITY_EDITOR
  private void Update()
  {
    if (Input.GetKeyDown(SampleStageKeyCode))
    {
      gameDataService.SetSelectedStage(-1, -1);
      sceneProvider.LoadSceneAsync(SceneType.Game).Forget();
    }
  }
#endif

  private void DisableMouse()
  {
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
  }


  private void OnDestroy()
  {
    disposables.Dispose();
  }

  public void Initialize(LocaleService localeService)
  {
    this.LocaleService = localeService;
    InitializeAsync().Forget();
  }

  private async UniTask InitializeAsync()
  {
    await LocalizationSettings.InitializationOperation;
    await sceneProvider.LoadSceneAsync(SceneType.Preloading, false);    
  }

  public void PlaySpeedrun()
  {
    speedRunManager = diContainer.Instantiate<SpeedRunManager>(new object[] { gameObject });
    disposables.Add(speedRunManager);
    speedRunManager.Play();
  }

  public void StopSpeedRun(bool isComplete)
  {
    speedRunManager?.End(isComplete);    
  }

  public void SaveResumeData(int chapter, int stage)
  {
    speedRunManager?.SaveResumeData(chapter, stage);
  }

  public void DisposeSpeedRunManager()
  {
    speedRunManager?.Dispose();
  }
}
