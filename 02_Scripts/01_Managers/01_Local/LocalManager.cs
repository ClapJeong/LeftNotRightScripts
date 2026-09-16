using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Local.CameraService;
using LR.Manager.Local.StagePreview;
using LR.Manager.Scene;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Manager.Stage.StageObject;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.Stage.Player.Enum;
using LR.UI;
using LR.UI.Enum;
using LR.UI.GameScene.Player;
using LR.UI.GameScene.Stage;
using LR.UI.GameScene.StageGimmick;
using LR.UI.Lobby;
using LR.UI.Preloading;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public partial class LocalManager : MonoBehaviour
{
  public static LocalManager instance;

  public CameraManager CameraManager;

  [SerializeField] private SceneType sceneType;
  [SerializeField] private Transform previewRoot;
  [SerializeField] private Camera previewCamera;
  [SerializeField] private FloorSetter floorSetter;

  private readonly List<IUIPresenter> firstPresenters = new();
  private readonly List<string> releaseKeys = new();
  private readonly CompositeDisposable disposables = new();

  [Inject] private readonly DiContainer diContainer = null;
  [Inject(Optional = true)] private readonly GlobalManager globalManager = null;
  [Inject(Optional = true)] private readonly IGameModeService gameModeService = null;
  [Inject(Optional = true)] private readonly IGameDataSetter gameDataSetter = null;
  [Inject(Optional = true)] private readonly IGameDataProvider gameDataProvider = null;
  [Inject(Optional = true)] private readonly IResourceManager resourceManager = null;
  [Inject(Optional = true)] private readonly AddressableKeySO addressableKeySO = null;
  [Inject(Optional = true)] private readonly ICanvasProvider canvasProvider = null;
  [Inject(Optional = true)] private readonly VeryFirstService veryFirstService = null;
  [Inject(Optional = true)] private readonly ISceneLoader sceneLoader = null;
  [Inject(Optional = true)] private readonly IBGMController bgmController = null;
  [Inject(Optional = true)] private readonly SoundService soundService = null;
  [Inject(Optional = true)] private readonly IGameDataIOController gameDataIOController = null;
  [Inject(Optional = true)] private readonly IAchievementRegister achievementRegister = null;

  private StagePreviewService stagePreviewService = null;
  public StageManager stageManager = null;
  private bool isInitialized = false;
  private GameObject destroyTarget;

  public void InjectInitialize()
  {    
    isInitialized = true;
  }

  public async UniTask InitializeAsync()
  {
    soundService.EnableAllSFX();
    soundService.PauseAll(false);
    instance = this;

    await UniTask.WaitWhile(() => !isInitialized);

    switch (sceneType)
    {
      case SceneType.Initialize:
        {
        }
        break;

      case SceneType.Preloading:
        {
          var resetGameData = false;
          if (PlayerPrefs.GetInt(PlayerPrefsName.Version, 0) != globalManager.GameVersion && globalManager.ResetGameData)
            resetGameData = true;
          PlayerPrefs.SetInt(PlayerPrefsName.Version, globalManager.GameVersion);
          await gameDataIOController.LoadDataAsync(resetGameData);

          var isDemoToRelease = !globalManager.Demo && gameDataProvider.WasDemoPlayed();
          if (isDemoToRelease)
          {
            var clearChapters = gameDataProvider.GetClearChaptes();
            var normalPerfectCount = gameDataProvider.GetPerfectClearCount(IDifficultyService.Difficulty.Normal);
            var hardPerfectCount = gameDataProvider.GetPerfectClearCount(IDifficultyService.Difficulty.Hard);
            achievementRegister.UpdateDemoAchievementData(clearChapters, normalPerfectCount, hardPerfectCount);
          }
            
          //마지막으로 데모 플레이했다가 본편으로 넘어가면 기존 데이터 일괄 확인

          gameModeService.SetGameMode(globalManager.Demo ? IGameModeService.GameMode.Demo 
                                                         : IGameModeService.GameMode.None);

          gameDataIOController.UpdateDemoPlayed(globalManager.Demo);
          gameDataIOController.SaveDataAsync().Forget();

          soundService.InitializeVolumes();
          await CreateFirstUIAsync();
          await LoadPreloadAsync();
          await LoadDialogueAsync();

          if (isDemoToRelease)
          {
            await CreateDemoContinueUIAsync();
          }
#if UNITY_EDITOR
          else if (globalManager.PlayVeryFirst)
#else
else if(gameDataProvider.IsVeryFirst())
#endif
          {
            await PlayVeryFirstSceneAsync();
          }
          else
          {
            gameDataSetter.ResetSelectedStage();
            sceneLoader.LoadSceneAsync(SceneType.Lobby).Forget();
          }
        }
        break;

      case SceneType.Lobby:
        {
          stagePreviewService = diContainer.Instantiate<StagePreviewService>(new object[] { previewRoot });
          disposables.Add(stagePreviewService);
          diContainer.BindInstance<IStagePreviewCreator>(stagePreviewService);
          bgmController.PlayLobbyBGMAsync().Forget();
          await CreateFirstUIAsync();
        }
        break;

      case SceneType.Game:
        {
          InitializeManagers();

          var chapter = gameDataProvider.GetSelectedChapter();
          var stage = gameDataProvider.GetSelectedStage();

          if (!gameModeService.IsSpeedRun)
          {
            PlayerPrefs.SetInt(PlayerPrefsName.SelectedChapter, chapter);
            PlayerPrefs.SetInt(PlayerPrefsName.SelectedStage, stage);
          }

          //floorSetter.UpdateFloor(chapter);
          var index = (Mathf.Max(0, chapter - 1)) * StageConst.StageUnit + stage;
          await stageManager.CreateAsync(index);
          await CreateFirstUIAsync();

          bgmController.PlayGameBGMAsync().Forget();
        }
        break;

      case SceneType.Epilogue:
        {

        }
        break;
    }
  }

  private async UniTask CreateDemoContinueUIAsync()
  {
    var key = addressableKeySO.Path.UI + addressableKeySO.UIName.DemoContinue;
    releaseKeys.Add(key);
    var canvasRoot = canvasProvider.GetCanvas(RootType.SceneLoading).transform;
    var view = await resourceManager.CreateAssetAsync<UIDemoContinueView>(key, canvasRoot);
    IUIPresenter presenter = null;
    var model = diContainer.Instantiate<UIDemoContinuePresenter.Model>(new object[]
    {
      (UnityAction)(()=>OnDemoContinue(presenter)),
      (UnityAction)(()=>OnDemoReset(presenter))
    });
    presenter = new UIDemoContinuePresenter(model, view);
    presenter.AttachOnDestroy(gameObject);
    presenter.ActivateAsync().Forget();
  }

  private async void OnDemoContinue(IUIPresenter demoPresenter)
  {
    await demoPresenter.DeactivateAsync();

    gameDataSetter.ResetSelectedStage();
    sceneLoader.LoadSceneAsync(SceneType.Lobby).Forget();
  }

  private async void OnDemoReset(IUIPresenter demoPresenter)
  {
    await demoPresenter.DeactivateAsync();

    gameDataIOController.ResetData();

    await PlayVeryFirstSceneAsync();
    //데이터 초기화, 저장, 이후 시퀸스
  }

  private async UniTask PlayVeryFirstSceneAsync()
  {
    await veryFirstService.CreateFirstTimelineAsync();
    await veryFirstService.CreateFirstLocaleUI();
    await veryFirstService.InitializeFirstLocaleUIAsync(
      onConfirm: async () =>
      {
        await veryFirstService.DestroyFirstLocaleUIAsync();

        veryFirstService.PlayFirstTimeline(
          onComplete: () =>
          {
            gameDataSetter.SetSelectedStage(1, 1);
            sceneLoader.LoadSceneAsync(
              SceneType.Game,
              false,
              default,
              null,
              onComplete: () =>
              {
                UniTask.Delay(10)
                 .ContinueWith(() => veryFirstService.DestroyCutscene()).Forget();
              },
              downVolume: false).Forget();
          });
      });
  }
  public async Task Play()
  {
    switch (sceneType)
    {
      case SceneType.Initialize:
        break;

      case SceneType.Preloading:
        {
          var tasks = new List<UniTask>();
          foreach (var firstPresenter in firstPresenters)
            tasks.Add(firstPresenter.ActivateAsync());

          await UniTask.WhenAll(tasks);
        }
        break;

      case SceneType.Lobby:
        {
          var tasks = new List<UniTask>();
          foreach (var firstPresenter in firstPresenters)
            tasks.Add(firstPresenter.ActivateAsync());

          await UniTask.WhenAll(tasks);
        }
        break;

      case SceneType.Game:
        {
          stageManager.Play();
        }        
        break;
    }
  }

  private void InitializeManagers()
  {
    diContainer.BindInterfacesAndSelfTo<InputProgressService>().AsSingle();
    diContainer.BindInterfacesAndSelfTo<InputProgressUIService>().AsSingle();
    diContainer.BindInterfacesAndSelfTo<InputQTEService>().AsSingle();
    diContainer.BindInterfacesAndSelfTo<InputQTEUIService>().AsSingle();

    diContainer.BindInterfacesAndSelfTo<StageManager>().AsSingle();
    stageManager = diContainer.Resolve<StageManager>();
    stageManager.CreateLazyServices();
    stageManager.AddTo(disposables);
  }

  private async UniTask CreateFirstUIAsync()
  {
    switch (sceneType)
    {
      case SceneType.Initialize:
        break;

      case SceneType.Preloading:
        {
        }
        break;

      case SceneType.Lobby:
        {
          var renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);
          previewCamera.targetTexture = renderTexture;
          await CreateLobbyUIAsync();
        }
        break;

      case SceneType.Game:
        {
          var playBeforeDialogue = stageManager.IsFirstDialogueExist();
          await CreatePlayerUIsAsync(playBeforeDialogue);                    
          await CreateStageUIAsync(playBeforeDialogue);
          if (stageManager.StageDataContainer.stageGimmick != LR.Stage.StageDataContainer.StageGimmick.None)
            await CreateGimmickUIAsync();
        }
        break;
    }
  }

  private async UniTask CreateLobbyUIAsync()
  {    
    var root = canvasProvider.GetCanvas(RootType.Overlay).transform;
    var key = addressableKeySO.Path.UI + addressableKeySO.UIName.LobbyRoot;
    releaseKeys.Add(key);

    var model = diContainer.Instantiate<UILobbyRootPresenter.Model>(new object[] { previewCamera, floorSetter});
    var view = await resourceManager.CreateAssetAsync<UILobbyRootView>(key, root);
    var presenter = new UILobbyRootPresenter(model, view);
    presenter.DeactivateAsync(true).Forget();

    presenter.AttachOnDestroy(gameObject);
    await presenter.ActivateAsync();
  }

  private async UniTask CreatePlayerUIsAsync(bool playBeforeDialogue)
  {
    var root = canvasProvider.GetCanvas(RootType.Overlay).transform;
    var key = addressableKeySO.Path.UI + addressableKeySO.UIName.PlayerRoot;
    releaseKeys.Add(key);
    var viewRoot = await resourceManager.CreateAssetAsync<PlayerRootContainer>(key, root);
    viewRoot.canvasGroup.alpha = 1.0f;
    viewRoot.transform.SetAsFirstSibling();
    destroyTarget = viewRoot.gameObject;

    var energyContainer = diContainer.Resolve<PlayerService>().EnergyContainer;
    var energyModel = diContainer.Instantiate<UIPlayerEnergyPresenter.Model>(new object[] {energyContainer});
    var energyView = viewRoot.energyView;
    var energyPresenter = new UIPlayerEnergyPresenter(energyModel, energyView);
    energyPresenter.AttachOnDestroy(gameObject);
    energyPresenter.ActivateAsync(true).Forget();

    var Leftmodel = diContainer.Instantiate<UIPlayerRootPresenter.Model>(new object[] { PlayerType.Left });
    var leftView = viewRoot.leftView;
    var leftPresenter = new UIPlayerRootPresenter(Leftmodel, leftView);
    leftPresenter.AttachOnDestroy(gameObject);

    var rightmodel = diContainer.Instantiate<UIPlayerRootPresenter.Model>(new object[] { PlayerType.Right });
    var rightView = viewRoot.rightView;
    var rightPresenter = new UIPlayerRootPresenter(rightmodel, rightView);
    rightPresenter.AttachOnDestroy(gameObject);

    energyPresenter.DeactivateAsync(true).Forget();
    leftPresenter.DeactivateAsync(true).Forget();
    rightPresenter.DeactivateAsync(true).Forget();

    var leftDamageLogModel = diContainer.Instantiate<UIPlayerDamageLogPresenter.Model>(new object[] { PlayerType.Left });
    var leftDamageLogView = viewRoot.leftDamageView;
    var leftDamagePresenter = new UIPlayerDamageLogPresenter(leftDamageLogModel, leftDamageLogView);
    leftDamagePresenter.AttachOnDestroy(gameObject);
    var rightDamageLogModel = diContainer.Instantiate<UIPlayerDamageLogPresenter.Model>(new object[] { PlayerType.Right });
    var rightDamageLogView = viewRoot.rightDamageView;
    var rightDamagePresenter = new UIPlayerDamageLogPresenter(rightDamageLogModel, rightDamageLogView);
    rightDamagePresenter.AttachOnDestroy(gameObject);
    leftDamagePresenter.ActivateAsync(true).Forget();
    rightDamagePresenter.ActivateAsync(true).Forget();

    stageManager.SubscribeOnEvent(IStageEventSubscriber.StageEventType.BeforeShowComplete, () =>
    {
      UniTask.WhenAll(
      energyPresenter.ActivateAsync(),
      leftPresenter.ActivateAsync(),
      rightPresenter.ActivateAsync()).Forget();
    });
  }

  private async UniTask CreateStageUIAsync(bool playBeforeDialogue)
  {
    var key = addressableKeySO.Path.UI + addressableKeySO.UIName.StageRoot;
    releaseKeys.Add(key);
    var root = canvasProvider.GetCanvas(RootType.Overlay).transform;

    var model = diContainer.Instantiate<UIStageRootPresenter.Model>();
    var view = await resourceManager.CreateAssetAsync<UIStageRootView>(key, root);
    view.transform.SetAsFirstSibling();
    var presenter = new UIStageRootPresenter(model, view);

    if (playBeforeDialogue == false)
      firstPresenters.Add(presenter);
    presenter.AttachOnDestroy(gameObject);
    await presenter.DeactivateAsync(true);
  }

  private async UniTask CreateGimmickUIAsync()
  {
    var root = canvasProvider.GetCanvas(RootType.Overlay).transform;

    string key = string.Empty;
    IUIPresenter presenter = null;
    switch (stageManager.StageDataContainer.stageGimmick)
    {
      case LR.Stage.StageDataContainer.StageGimmick.CameraRotator:
        {
          key = addressableKeySO.Path.UI + addressableKeySO.UIName.CameraRotator;
          var model = diContainer.Instantiate<UICameraRotatorPresenter.Model>();
          var view = await resourceManager.CreateAssetAsync<UICameraRotatorView>(key, root);
          view.transform.SetAsFirstSibling();
          presenter = new UICameraRotatorPresenter(model, view);
        }
        break;

      case LR.Stage.StageDataContainer.StageGimmick.CameraMover:
        {
          key = addressableKeySO.Path.UI + addressableKeySO.UIName.CameraMover;
          var model = diContainer.Instantiate<UICameraMoverPresenter.Model>();
          var view = await resourceManager.CreateAssetAsync<UICameraMoverView>(key, root);
          view.transform.SetAsFirstSibling(); 
          presenter = new UICameraMoverPresenter(model, view);
        }
        break;

      case LR.Stage.StageDataContainer.StageGimmick.QTEBomb:
        {
          key = addressableKeySO.Path.UI + addressableKeySO.UIName.QTEBomb;
          var model = diContainer.Instantiate<UIQTEBombPresenter.Model>();
          var view = await resourceManager.CreateAssetAsync<UIQTEBombView>(key, root);
          view.transform.SetAsFirstSibling();
          presenter = new UIQTEBombPresenter(model, view);
        }
        break;

      case LR.Stage.StageDataContainer.StageGimmick.InputRequire:
        {
          key = addressableKeySO.Path.UI + addressableKeySO.UIName.InputRequire;
          var model = diContainer.Instantiate<UIInputRequirePresenter.Model>();
          var view = await resourceManager.CreateAssetAsync<UIInputRequireView>(key, root);
          view.transform.SetAsFirstSibling();
          presenter = new UIInputRequirePresenter(model, view);
        }
        break;

      case LR.Stage.StageDataContainer.StageGimmick.Swap:
        {
          key = addressableKeySO.Path.UI + addressableKeySO.UIName.Swap;
          var model = diContainer.Instantiate<UISwapPresenter.Model>();
          var view = await resourceManager.CreateAssetAsync<UISwapView>(key, root);
          view.transform.SetAsFirstSibling();
          presenter = new UISwapPresenter(model, view);
        }
        break;
    }
    if(!string.IsNullOrEmpty(key))
      releaseKeys.Add(key);

    if(presenter != null)
    {
      stageManager.InjectGimmickUI(presenter);
      presenter.AttachOnDestroy(gameObject);
      await presenter.DeactivateAsync(true);
    }    
  }

  private async UniTask LoadPreloadAsync()
  {
    var label = addressableKeySO.Label.Preload;
    await resourceManager.LoadAssetsAsync(label);
  }

  private async UniTask LoadDialogueAsync()
  {
    var label = addressableKeySO.Label.Dialogue;
    await resourceManager.LoadAssetsAsync(label);
  }

  private void OnDestroy()
  {
    if (destroyTarget != null)
      DestroyImmediate(destroyTarget);

    foreach (var key in releaseKeys)
      resourceManager.ReleaseAsset(key);
    disposables.Dispose();
  }
}
