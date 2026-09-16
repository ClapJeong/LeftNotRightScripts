using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Manager.Stage.Gimmick;
using LR.Manager.Stage.Marking;
using LR.Manager.Stage.StageObject;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.Manager.Scene;
using LR.Stage.Complete;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Stage.StageDataContainer;
using LR.Table.Dialogue;
using LR.UI;
using LR.UI.Credit;
using LR.UI.EpilogueScene;
using LR.UI.GameScene.Dialogue;
using LR.UI.GameScene.Player;
using LR.UI.GameScene.Stage;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using LR.Manager.GameDataManager;
using LR.Manager.Stage.Practice;

namespace LR.Manager.Stage
{
  public class StageManager :
  IStageStateHandler,
  IStageStateProvider,
  IStageResultHandler,
  IStageEventSubscriber,
  IPlayerGetter,
  IStageCreator,
  IDialoguePlayableProvider,  
  IDisposable
  {
    public StageDataContainer StageDataContainer { get; private set; }
    public bool IsPlayingState
      => stageState == StageEnum.State.Playing;

    #region Injects
    [Inject] private readonly DiContainer diContainer = null;
    [Inject] private readonly GlobalManager globalManager = null;
    [Inject] private readonly AddressableKeySO addressableKeySO = null;
    [Inject] private readonly IResourceManager resourceManager = null;
    [Inject] private readonly IGameDataIOController gameDataIOController = null;
    [Inject] private readonly IGameDataProvider gameDataProvider = null;
    [Inject] private readonly IGameDataSetter gameDataSetter = null;
    [Inject] private readonly IGameModeService gameModeService = null;
    [Inject] private readonly ISceneLoader sceneLoader = null;
    [Inject] private readonly ICameraEffectService cameraEffectService = null;
    [Inject] private readonly ICameraValueService cameraValueService = null;
    [Inject] private readonly ISFXController sfxController = null;
    [Inject] private readonly IInputActionSubscriber inputActionSubscriber = null;
    [Inject] private readonly UISO uiSO = null;
    [Inject] private readonly StageCompleteDataSO stageCompleteDataSO = null;
    [Inject] private readonly IUIPresenterContainer uiPresenterContainer = null;
    [Inject] private readonly IInputQTEService inputQTEService = null;
    [Inject] private readonly IInputProgressService inputProgressService = null;    
    [Inject] private readonly SoundSO soundSO = null;
    [Inject] private readonly IBGMController bgmController = null;
    [Inject] private readonly IAchievementRegister achievementRegister = null;
    [Inject] private readonly IPracticeService iPracticeService = null;
    [Inject] private readonly IDifficultyService difficultyService = null;
    [Inject] private readonly IStoreTypeProvider storeTypeProvider = null;
    [Inject(Id = "InstantiateRoot")] private readonly Transform gameObjectInstantiateRoot = null;
    [Inject(Id = "MarkRoot")] private readonly Transform markRoot = null;
    #endregion

    #region Services
    private SignalService signalService;
    public EffectService effectService;
    private PlayerService playerService;
    private TriggerTileService triggerTileService;
    private InteractiveObjectService interactiveObjectService;
    private SignalListenerService signalListenerService;
    private DialogueDataContainer dialogueDataContainer;
    private RestartShaderController restartShaderController;
    private MarkService markService;
    private PracticeService practiceService;
    private StageDataRecordService dataRecordService;
    private LeaderboardController leaderboardController;

    private StageObjectControlController controllerController;
    private ExhaustLightController exhaustLightController;
    private TimeScaleController exhaustTimerScaleController;
    #endregion    

    private readonly Dictionary<IStageEventSubscriber.StageEventType, UnityEvent> stageEvents = new();
    private readonly UnityEvent<float> onRestartDelay = new();

    private IStageGimmick stageGimmick;
    private IUIPresenter dialogueUIPresenter;    

    private StageEnum.State stageState = StageEnum.State.None;
    private StageEnum.State pauseBeforeState;

    public int StageIndex => stageIndex;

    private bool isThisStageFirst = false;
    private bool isLeftClear = false;
    private bool isRightClear = false;
    public bool IsRestartDelay => isRestartDelay;
    private bool isRestartDelay = false;    
    private bool isPlayEpilogue = false;
    private int restartRegisterFrame;
    private int stageIndex = 0;

    private StageFirstShowService firstShowService;
    private StageCameraController stageCameraController;
    private StageLightController stageLightController;
    private WallHitFallObjectController wallHitFallObjectController;

    public void CreateLazyServices()
    {
      diContainer.BindInterfacesAndSelfTo<PracticeService>().AsSingle();
      practiceService = diContainer.Resolve<PracticeService>();

      diContainer.BindInterfacesAndSelfTo<SignalService>().AsSingle();
      signalService = diContainer.Resolve<SignalService>();

      diContainer.BindInterfacesAndSelfTo<EffectService>().AsSingle();
      effectService = diContainer.Resolve<EffectService>();

      diContainer.BindInterfacesAndSelfTo<PlayerService>().AsSingle();
      playerService = diContainer.Resolve<PlayerService>();

      diContainer.BindInterfacesAndSelfTo<TriggerTileService>().AsSingle();
      triggerTileService = diContainer.Resolve<TriggerTileService>();

      diContainer.BindInterfacesAndSelfTo<InteractiveObjectService>().AsSingle();
      interactiveObjectService = diContainer.Resolve<InteractiveObjectService>();

      diContainer.BindInterfacesAndSelfTo<SignalListenerService>().AsSingle();
      signalListenerService = diContainer.Resolve<SignalListenerService>();

      diContainer.BindInterfacesAndSelfTo<DialogueDataContainer>().AsSingle();
      dialogueDataContainer = diContainer.Resolve<DialogueDataContainer>();

      diContainer.BindInterfacesAndSelfTo<RestartShaderController>().AsSingle();
      restartShaderController = diContainer.Resolve<RestartShaderController>();

      diContainer.BindInterfacesAndSelfTo<StageDataRecordService>().AsSingle();
      dataRecordService = diContainer.Resolve<StageDataRecordService>();

      controllerController = new(playerService, triggerTileService, interactiveObjectService, signalListenerService);

      markService = diContainer.Instantiate<MarkService>(new object[] { markRoot });
      diContainer.BindInterfacesAndSelfTo<MarkService>().FromInstance(markService).AsSingle();

      leaderboardController = diContainer.Instantiate<LeaderboardController>();

      exhaustTimerScaleController = new();
    }

    #region IStageCreator
    public async UniTask CreateAsync(int index, bool isEnableImmediately = false)
    {
      stageIndex = index;
      var key = addressableKeySO.Path.Stage + string.Format(addressableKeySO.StageName.StageNameFormat, stageIndex);
#if UNITY_EDITOR
      if (stageIndex < 1)
        key = "SampleStage";
#endif

      try
      {
        var topClearIndex = gameDataProvider.GetMaxClearIndex();
        isThisStageFirst = stageIndex > topClearIndex;
        isPlayEpilogue = gameModeService.GetCurrentGameMode() != IGameModeService.GameMode.Demo &&
                         (gameDataProvider.StageDataCount == stageIndex) &&
                         (topClearIndex == (stageIndex - 1));

        StageDataContainer = await resourceManager.CreateAssetAsync<StageDataContainer>(key);
        diContainer.BindInstance(StageDataContainer);

        stageCameraController = diContainer.Instantiate<StageCameraController>();
        wallHitFallObjectController = diContainer.Instantiate<WallHitFallObjectController>(new object[] { markRoot });
        stageLightController = new(
          StageDataContainer.lightRoot,
          StageDataContainer.doctorLight);
        stageLightController.EnableDoctorLight(false);

        if (isThisStageFirst)
        {
          StageDataContainer.leftPlayerClearTileView.EnableLight = false;
          StageDataContainer.leftPlayerClearTileView.IdleParticle.Stop();
          StageDataContainer.rightPlayerClearTileView.EnableLight = false;
          StageDataContainer.rightPlayerClearTileView.IdleParticle.Stop();
          stageLightController.EnablePlayerLights(false);
        }
        else
        {
          StageDataContainer.leftPlayerClearTileView.IdleParticle.Play();
          StageDataContainer.rightPlayerClearTileView.IdleParticle.Play();
        }        

        var dialogueDataHandles = await resourceManager.LoadAssetsAsync(addressableKeySO.Label.Dialogue);        
        dialogueDataContainer.CacheDialogueDatas(dialogueDataHandles, StageDataContainer);

        SetupCamera(StageDataContainer);

        await markService.InitializeAsync();
        await playerService.SetupAsync(StageDataContainer, isEnableImmediately);
        await triggerTileService.SetupAsync(StageDataContainer, isEnableImmediately);
        await interactiveObjectService.SetupAsync(StageDataContainer, isEnableImmediately);
        await signalListenerService.SetupAsync(StageDataContainer, isEnableImmediately);
        var gimmickInitializer = diContainer.Instantiate<GimmickInitializer>();
        stageGimmick = await gimmickInitializer.InitializeStageGimmickAsync(StageDataContainer.stageGimmick);

        diContainer.Inject(StageDataContainer.stageCompleteController, new object[] { isPlayEpilogue, stageLightController, stageCameraController });
        exhaustLightController = new(
          StageDataContainer.leftExhaustLight,
          StageDataContainer.rightExhaustLight,
          playerService.GetPlayer(PlayerType.Left).GetMoveController(),
          playerService.GetPlayer(PlayerType.Right).GetMoveController());
        exhaustLightController.StopLight();

        wallHitFallObjectController.Initialize();
        dataRecordService.InitializeStage(StageDataContainer, playerService.EnergyContainer);

        if (dialogueDataContainer.TryGetBeforeDialogueData(out var beforeDialogueData))
          dialogueUIPresenter = await CreateDialogueUIAsync(beforeDialogueData, OnDialogueComplete, StageDataContainer.beforeDialogueIndex, false);
      }
      catch (Exception e)
      {
        Debug.LogError($"Stage Create Error: {e}");
        gameDataSetter.ResetSelectedStage();
        sceneLoader.LoadSceneAsync(SceneType.Lobby).Forget();
      }
    }

    private async void OnDialogueComplete()
    {
      await dialogueUIPresenter.DeactivateAsync();
      dialogueUIPresenter = null;

      if (isThisStageFirst)
        PlayBeginShowAsync().Forget();
      else
        PlayStage();
    }

    private async UniTask PlayBeginShowAsync()
    {
      firstShowService = diContainer.Instantiate<StageFirstShowService>(new object[] {stageLightController, stageCameraController});

      await firstShowService.PlayAsync();

      PlayStage();
    }
    #endregion

    #region IStageService
    public async UniTask RestartAsync()
    {
      if (isRestartDelay)
        return;

      dataRecordService.ResetRecords();

      isRestartDelay = true;
      cameraEffectService.StopGrain();
      cameraEffectService.ResetImpulse();
      cameraEffectService.StopNoise();
      markService.ClearWallHitPaints();
      markService.ShowDeadPaints();
      exhaustTimerScaleController.RevertImmediately();
      await UniTask.WaitForEndOfFrame();

      dataRecordService.AddRestartCount();
      var chapter = gameDataProvider.GetSelectedChapter();
      var stage = gameDataProvider.GetSelectedStage();
      var isCompleteStage = gameDataProvider.IsClearStage(chapter, stage, out var _);
      var isSpeedRun = gameModeService.IsSpeedRun;
      var isUnderFail = playerService.EnergyContainer.TotalNormalized < 0.4f;
      if (isCompleteStage)
        dataRecordService.ResetFailCount();
      else if (!isSpeedRun && isUnderFail)
        dataRecordService.AddFailCount();

      sfxController.PauseAll(false);

      exhaustLightController.StopLight();

      SetState(StageEnum.State.RestartWait);
      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.Restart);

      sfxController.PlayOnce(AudioSourceType.CenterUI, SFX.StageRestart);
      sfxController.DisableAllSFX();

      bgmController.UpdateVolume(1.0f);
      bgmController.UpdatePitch(1.0f);

      controllerController.RestartAll();

      signalService.ResetAllSignal();
      stageGimmick?.Restart();      

      isLeftClear = false;
      isRightClear = false;

      restartShaderController.PlayRestartMatAsync().Forget();

      restartRegisterFrame = Time.frameCount;
      inputActionSubscriber.Subscribe(LRInputType.LeftAny, OnAnyPlayerInput);
      inputActionSubscriber.Subscribe(LRInputType.RightAny, OnAnyPlayerInput);
    }

    private void OnPracticeButtonPerformed()
    {
      var isSpeedRun = gameModeService.IsSpeedRun;
      if (isSpeedRun)
        return;

      if (restartRegisterFrame == Time.frameCount)
        return;

      if (stageState == StageEnum.State.Success)
        return;

      var isPlayingGame = !iPracticeService.IsPractice;
      if (isPlayingGame)
        practiceService.ApplyPracticeMode();
      else
        practiceService.RevertPracticeMode();

      if(stageState != StageEnum.State.Ready)
      {
        isRestartDelay = false;
        RestartAsync().Forget();        
      }        
    }


    private async void OnAnyPlayerInput(InputPhase phase)
    {
      exhaustLightController.StopLight();

      if (restartRegisterFrame == Time.frameCount)
        return;

      if (stageState == StageEnum.State.Pause)
        return;

      if (phase == InputPhase.Performed)
      {
        inputActionSubscriber.Unsubscribe(LRInputType.LeftAny, OnAnyPlayerInput);
        inputActionSubscriber.Unsubscribe(LRInputType.RightAny, OnAnyPlayerInput);

        controllerController.EnableAll(true);
        stageGimmick?.Begin();

        SetState(StageEnum.State.Playing);
        sfxController.EnableAllSFX();

        var duration = 0.0f;
        var targetDuration = uiSO.Stage.RestartDelay;
        while (duration < targetDuration)
        {
          onRestartDelay?.Invoke(duration / targetDuration);

          duration += Time.unscaledDeltaTime;
          await UniTask.Yield();
        }
        onRestartDelay?.Invoke(1.0f);

        isRestartDelay = false;
      }
    }

    public void Play()
    {
      if (dialogueUIPresenter != null)
        dialogueUIPresenter.ActivateAsync();
      else if (isThisStageFirst)
        PlayBeginShowAsync().Forget();
      else
        PlayStage();
    }

    private void PlayStage()
    {
      inputActionSubscriber.SubscribePhase(LRInputType.Practice, OnPracticeButtonPerformed, InputPhase.Performed);
      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.BeforeShowComplete);
      SetState(StageEnum.State.Ready);
    }

    public void Complete()
    {
      var chapter = gameDataProvider.GetSelectedChapter();
      var stage = gameDataProvider.GetSelectedStage();

      if (gameModeService.IsSpeedRun)
      {
        var index = (chapter - 1) * StageConst.StageUnit + stage;
        if (gameDataProvider.StageDataCount == index)
          globalManager.StopSpeedRun(true);
      }

      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.AllClearEnter);
      sfxController.PlayOnce(AudioSourceType.CenterUI, SFX.LastClear);

      playerService.EnergyContainer.DebugEnergy();

      controllerController.EnableAll(false);
      stageGimmick?.Complete();


      var currentDifficulty = difficultyService.CurrentDifficulty;
      gameDataIOController.AddClearData(chapter, stage, currentDifficulty);
      if (dataRecordService.IsThisSuccessIsPerfect())
        gameDataIOController.UpdatePerfectData(chapter, stage, currentDifficulty);
      gameDataIOController.SaveDataAsync().Forget();

      if(difficultyService.CurrentDifficulty == IDifficultyService.Difficulty.Hard)
      {
        var hardClearCount = gameDataProvider.GetHardClearCount();
        achievementRegister.UpdateHardModeAchievement(hardClearCount);
      }

      dataRecordService.SaveRecord();

      if(storeTypeProvider.StoreType == StoreType.Steam)
      {
        var stageIndex = (chapter - 1) * StageConst.StageUnit + stage;
        var difficulty = difficultyService.CurrentDifficulty;
        var leaderboardKey = string.Format(StoreKeys.StageLeaderboardKeyFormat, stageIndex, difficulty);

        var result = dataRecordService.GetSumResult();
        var floatSum = result.TotalSum;
        var score = UnityEngine.Mathf.RoundToInt(floatSum * (1.0f / LeaderboardUnit.Unit));

        leaderboardController.SubmitStageLeaderboardAsync(leaderboardKey, score, stageIndex).Forget();
      }      

      dataRecordService.DebugResult();

      UpdateAchievement();      

      SetState(StageEnum.State.Success);

      if (gameModeService.IsSpeedRun)
      {
        OnReallyCompleteAsync().Forget();        
      }        
      else
        StageDataContainer.stageCompleteController.PlayAsync(() => OnReallyCompleteAsync().Forget()).Forget();
    }

    private void UpdateAchievement()
    {     
      UpdateChapterAchievement();
      UpdatePerfectAchievement();
    }

    private void UpdateChapterAchievement()
    {
      var stage = gameDataProvider.GetSelectedStage();
      var chapter = gameDataProvider.GetSelectedChapter();

      {
        if (gameDataProvider.IsClearStage(chapter, stage, out var difficulty) && difficulty != IDifficultyService.Difficulty.Easy)
        {
          achievementRegister.SetData(string.Format(StoreKeys.StageClearFormat, chapter), 1);
        }
      }

      for (int i = 0; i < StageConst.StageUnit; i++)
      {
        var index = i + 1;
        if (!gameDataProvider.IsClearStage(chapter, index, out var difficulty) || difficulty == IDifficultyService.Difficulty.Easy)
          return;
      }

      achievementRegister.SetAchievement(string.Format(StoreKeys.StageClearFormat, chapter));
    }

    private void UpdatePerfectAchievement()
    {
      var isPerfect = dataRecordService.IsThisSuccessIsPerfect();
      if (isPerfect)
      {
        var difficulty = difficultyService.CurrentDifficulty;
        if (difficulty != IDifficultyService.Difficulty.Easy)
        {
          var key = StoreKeys.GetPerfectKey(difficulty);
          var perfectCount = gameDataProvider.GetPerfectClearCount(difficulty);
          achievementRegister.SetData(key, perfectCount);
        }
      }
    }

    private async UniTask OnReallyCompleteAsync()
    {
      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.Complete);

      if (dialogueDataContainer.TryGetAfterDialogueData(out var afterDialogueData, isThisStageFirst))
      {
        dialogueUIPresenter = await CreateDialogueUIAsync(afterDialogueData, OnAfterDialogueComplete, StageDataContainer.afterDialogueIndex, true);
        dialogueUIPresenter.ActivateAsync().Forget();
      }
      else
      {
        stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.AfterDialogueComplete);
        uiPresenterContainer.GetFirst<UIStageRestartPresenter>()?.ActivateAsync().Forget();
        uiPresenterContainer.GetFirst<UIPracticePresenter>()?.ActivateAsync().Forget();
      }      

      isThisStageFirst = false;

      if (gameModeService.IsSpeedRun)
      {
        var chapter = gameDataProvider.GetSelectedChapter();
        var stage = gameDataProvider.GetSelectedStage();
        var isNextStageExist = (chapter - 1) * StageConst.StageUnit + stage < gameDataProvider.StageDataCount;
        if (isNextStageExist)
        {
          stage += 1;
          var addChapter = stage > StageConst.StageUnit;
          var nextChapter = addChapter ? chapter + 1 : chapter;
          var nextStage = addChapter ? 1 : stage;
#if UNITY_EDITOR
          if (globalManager.SkipSpeedRun)
          {
            nextChapter = 12;
            nextStage = 8;
          }
#endif
          gameDataSetter.SetSelectedStage(nextChapter, nextStage);
          globalManager.SaveResumeData(nextChapter, nextStage);
          sceneLoader.ReloadCurrentSceneAsync().Forget();
        }
      }
    }

    private void OnAfterDialogueComplete()
    {
      if (isPlayEpilogue)
      {
        OnEpilogueAsync().Forget();
      }
      else
      {
        stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.AfterDialogueComplete);
        dialogueUIPresenter.DeactivateAsync().Forget();
        dialogueUIPresenter = null;
        uiPresenterContainer.GetFirst<UIStageRestartPresenter>()?.ActivateAsync().Forget();
        uiPresenterContainer.GetFirst<UIPracticePresenter>()?.ActivateAsync().Forget();
      }
    }

    private async UniTask OnEpilogueAsync()
    {
      IUIPresenter epiloguePresenter = null;
      epiloguePresenter = await UIEpilogueRootPresenter.CreateAsync(
        diContainer,
        onFirstFadeComplete: () =>
        {
          sfxController.PlayOnce(AudioSourceType.CenterUI, SFX.EpilogueExplosion);
        },
        firstFadeShowDelay: 7.0f,
        onCreditComplete: async () =>
        {
          await PlayCreditAsync();
          await epiloguePresenter.DeactivateAsync(true);
        });
      
      await epiloguePresenter.ActivateAsync();

      var existUIPresenters = new List<IUIPresenter>
      {
        dialogueUIPresenter,
        uiPresenterContainer.GetFirst<UIStageRootPresenter>(),
        uiPresenterContainer.GetFirst<UIPlayerEnergyPresenter>()
      };
      existUIPresenters.AddRange(uiPresenterContainer.GetAll<UIStageRootPresenter>());
      foreach (var uiPresenter in existUIPresenters)
        uiPresenter.Dispose();

      GameObject.Destroy(StageDataContainer.gameObject);
    }

    private async UniTask PlayCreditAsync()
    {
      await UniTask.WaitForSeconds(stageCompleteDataSO.EpiloguecompleteCreditDelay);
      IUIPresenter creditPresenter = null;
      creditPresenter = await UICreditPresenter.CreateAsync(
        diContainer,
        onExit: async () =>
        {
          await sceneLoader.LoadSceneAsync(SceneType.Lobby, false);
          await creditPresenter.DeactivateAsync();
        });
      await creditPresenter.ActivateAsync();      
    }

    public void BeginStage()
    {
      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.Begin);

      controllerController.EnableAll(true);
      stageGimmick?.Begin();

      sfxController.PauseAll(false);
      SetState(StageEnum.State.Playing);
    }

    public void Pause()
    {
      pauseBeforeState = stageState;
      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.Pause);

      stageGimmick?.Pause();
      playerService.EnableAll(false);
      SetState(StageEnum.State.Pause);

      sfxController.PauseAll(true);
    }

    public void Resume()
    {
      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.Resume);

      playerService.EnableAll(true);
      stageGimmick?.Resume();
      SetState(pauseBeforeState);

      sfxController.PauseAll(false);
    }

    public void SetState(StageEnum.State state)
      => stageState = state;
    #endregion

    #region IStageStateProvider
    public StageEnum.State GetState()
      => stageState;
    #endregion

    #region IStageResultHandler
    public async void Exhausted()
    {
      SetState(StageEnum.State.Fail);

      inputProgressService.Stop();
      inputQTEService.Stop();
      stageGimmick?.Pause();
      sfxController.PlayOnce(AudioSourceType.CenterUI, SFX.StageFail);

      if (!gameModeService.IsSpeedRun)
      {
        stageLightController.EnablePlayerLights(false);        
        await exhaustTimerScaleController.PlayTimeSlowAsync(
          stageCompleteDataSO.ExhaustTimeScale,
          stageCompleteDataSO.ExhaustInDelay,
          stageCompleteDataSO.ExhaustInDuration,
          onDelayComplete: () =>
          {
            exhaustLightController.PlayLight();
            bgmController.UpdateVolume(soundSO.BGMVolume.ExhaustedVolume);
            bgmController.UpdatePitch(soundSO.BGMVolume.ExhaustedPitch);            
          });        
        await exhaustTimerScaleController.PlayTimeSlowAsync(
          1.0f,
          stageCompleteDataSO.ExhaustWaitDelay,
          stageCompleteDataSO.ExhaustOutDuration,
          null,
          onScaleComplete: () =>
          {
            stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.Exhausted);
          });
      }
      else
        stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.Exhausted);

      if (gameModeService.IsSpeedRun)
        RestartAsync().Forget();
      else
      {       
        cameraEffectService.PlayGrainAsync().Forget();
        cameraEffectService.PlayNoise(ICameraEffectService.NoiseType.Exhaust);        
      }
    }

    public void LeftClearEnter()
    {
      var isPractice = iPracticeService.IsPractice;
      if (isPractice)
        return;

      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.LeftClearEnter);

      isLeftClear = true;
      if (isLeftClear && isRightClear)
        Complete();
      else
        sfxController.PlayOnce(AudioSourceType.Left, SFX.FirstClear);
    }

    public void RightClearEnter()
    {
      var isPractice = iPracticeService.IsPractice;
      if (isPractice)
        return;

      stageEvents.TryInvoke(IStageEventSubscriber.StageEventType.RightClearEnter);

      isRightClear = true;
      if (isLeftClear && isRightClear)
        Complete();
      else
        sfxController.PlayOnce(AudioSourceType.Left, SFX.FirstClear);
    }
    #endregion

    #region IStageEventSubscriber
    public void SubscribeOnEvent(IStageEventSubscriber.StageEventType type, UnityAction action)
      => stageEvents.AddEvent(type, action);

    public void UnsubscribeOnEvent(IStageEventSubscriber.StageEventType type, UnityAction action)
      => stageEvents.RemoveEvent(type, action);

    public void SubscribeRestartDelay(UnityAction<float> action)
      => onRestartDelay.AddListener(action);

    public void UnsubscribeRestartDelay(UnityAction<float> action)
      => onRestartDelay.RemoveListener(action);
    #endregion

    #region IPlayerGetter
    public IPlayerPresenter GetPlayer(PlayerType playerType)
      => playerService.GetPlayer(playerType);

    public bool IsAllPlayerExist()
      => playerService.IsAllPlayerExist();
    #endregion

    #region IDialoguePlayableProvider
    public bool IsFirstDialogueExist()
      => dialogueDataContainer.IsFirstDialogueExist();
    #endregion


    private void SetupCamera(StageDataContainer stageData)
    {
      cameraValueService.SetSize(stageData.cameraSize, true);
    }

    public void Dispose()
    {      
      inputActionSubscriber.Unsubscribe(LRInputType.LeftAny, OnAnyPlayerInput);
      inputActionSubscriber.Unsubscribe(LRInputType.RightAny, OnAnyPlayerInput);
      inputActionSubscriber.UnsubscribePhase(LRInputType.Practice, OnPracticeButtonPerformed, InputPhase.Performed);

      practiceService.Dispose();
      exhaustTimerScaleController.Dispose();
      firstShowService?.Dispose();
      stageGimmick?.Dispose();
      if (StageDataContainer != null)
        resourceManager.ReleaseInstance(StageDataContainer.gameObject, true);
    }

    public void InjectGimmickUI(IUIPresenter presenter)
    {
      stageGimmick?.InjectUI(presenter);
      SubscribeOnEvent(IStageEventSubscriber.StageEventType.BeforeShowComplete, () =>
      {
        presenter.ActivateAsync().Forget();
      });
      SubscribeOnEvent(IStageEventSubscriber.StageEventType.Exhausted, () =>
      {
        presenter.DeactivateAsync().Forget();
      });
      SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, () =>
      {
        presenter.ActivateAsync().Forget();
      });
    }

    private async UniTask<IUIPresenter> CreateDialogueUIAsync(
      DialogueData dialogueData, 
      UnityAction onDialogueComplete,
      int index,
      bool showPortraitAfterDialogue)
    {      
      var presenter = await UIDialogueRootPresenter.CreateDialogueUIAsync(
        diContainer,
        dialogueData,
        onDialogueComplete,
        index,
        false,
        showPortraitAfterDialogue,
        diContainer.Resolve<LocalManager>().gameObject);

      return presenter;
    }

    public Transform GetGameObjectInstantiateRoot()
      => gameObjectInstantiateRoot;
  }
}