using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Local.CameraService;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Manager.UI;
using LR.Stage.StageDataContainer;
using LR.UI;
using LR.UI.GameScene.Player;
using LR.UI.GameScene.Stage;
using LR.UI.GameScene.StageGimmick;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.Stage.Complete
{
  public class StageCompleteController : MonoBehaviour, IStageCompleteObject
  {
    public bool IsDoctorExist => doctor.State == StageCompleteDoctor.DoctorBeginState.Idle || 
                                 doctor.State == StageCompleteDoctor.DoctorBeginState.Shaking;

    [SerializeField] private StageCompleteSignal signal;
    [SerializeField] private SpriteRenderer stairSpriteRenderer;
    [SerializeField] private SpriteRenderer shadowSpriteRenderer;
    [SerializeField] private Animator exitAnimator;
    [SerializeField] private ParticleSystem exitDoorOpenEffect;
    [SerializeField] private StageCompleteDoctor doctor;    

    private readonly CTSContainer playCTS = new();
    private ICameraValueService cameraValueService;
    private ICameraEffectService cameraEffectService;
    private IUIPresenterContainer presenterContainer;
    private ISFXController sfxController;
    private StageCompleteDataSO data;
    private bool isLastStage;
    private bool isPlayEpilogue;

    private StageLightController stageLightController;
    private StageCameraController stageCameraController;
    private readonly List<IUIPresenter> uiPresenters = new();

    private enum CameraMoveType
    {
      None,
      OneWay,
      Round,
    }

    [Inject]
    public void Initialize(
      [Inject] ICameraValueService cameraValueService,
      [Inject] ICameraEffectService cameraEffectService,
      [Inject] IUIPresenterContainer presenterContainer,
      [Inject] StageManager stageManager,
      [Inject] StageCompleteDataSO data,
      [Inject] IGameDataProvider gameDataProvider,
      [Inject] IGameModeService gameModeService,
      [Inject] ISFXController sfxController,
      bool isPlayEpilogue,
      StageLightController stageLightController,
      StageCameraController stageCameraController)
    {
      this.cameraValueService = cameraValueService;
      this.cameraEffectService = cameraEffectService;
      this.isPlayEpilogue = isPlayEpilogue;      
      this.presenterContainer = presenterContainer;
      signal.Initialize(stageManager);      
      var isSpeedRun = gameModeService.IsSpeedRun;
      doctor.Initialize(gameDataProvider, stageManager, data, sfxController, isSpeedRun);
      this.sfxController = sfxController;
      this.data = data;
     
      this.stageLightController = stageLightController;
      this.stageCameraController = stageCameraController;

      var chapter = gameDataProvider.GetSelectedChapter();
      var stage = gameDataProvider.GetSelectedStage();
      var index = (chapter - 1) * StageConst.StageUnit + stage;
      this.isLastStage = gameModeService.GetCurrentGameMode() != IGameModeService.GameMode.Demo && 
                         gameDataProvider.StageDataCount == index;

      var exitFlip = shadowSpriteRenderer.transform.position.y > stairSpriteRenderer.transform.position.y;
      shadowSpriteRenderer.flipY = exitFlip;
      stairSpriteRenderer.flipY = exitFlip;

      stageManager.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);
    }

    public void OnRestart()
    {
      exitDoorOpenEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      PlayExitIdle();
      stageLightController.EnablePlayerLights(true);
      stageLightController.EnableDoctorLight(false);
      UniTask.WhenAll(uiPresenters.Select(presenter => presenter.ActivateAsync(true)));
    }

    private void PlayExitIdle()
      => exitAnimator.Play(AnimatorHash.StageCompleteExit.Idle);

    private void PlayExitActivate()
      => exitAnimator.Play(AnimatorHash.StageCompleteExit.Activate);

    public async UniTask PlayAsync(UnityAction onComplete, CancellationToken token = default)
    {
      playCTS.Cancel();
      playCTS.Create();
      var myToken = playCTS.token;

      try
      {
        var cameraMoveType = isPlayEpilogue ? CameraMoveType.OneWay
                                            : doctor.State != StageCompleteDoctor.DoctorBeginState.None ? CameraMoveType.Round
                                                                                                        : CameraMoveType.None;        

        var cameraBeginSize = cameraValueService.GetCurrentOrthographizSize();
        var cameraOriginSize = cameraValueService.GetInitializedOrthographizSize();
        var cameraZoomInSize = cameraOriginSize * data.ZoomInRatio;
        var cameraTargetPosition = isPlayEpilogue ? signal.transform.position
                                                  : 0.5f * data.ZoomPositionRatio * (doctor.transform.position + exitAnimator.transform.position);

        if (uiPresenters.Count == 0)
          CacheAllUIPresenters();

        switch (cameraMoveType)
        {
          case CameraMoveType.None:
            {
              await UniTask.WaitForSeconds(data.FirstDelay, false, PlayerLoopTiming.Update, myToken);

              await PlayExitPartAsync(myToken);
              await UniTask.WaitForSeconds(data.DoorOpenDuration);
              exitDoorOpenEffect.Play();
            }
            break;

          case CameraMoveType.OneWay:
            {
              await UniTask.WhenAll(uiPresenters.Select(presenter => presenter.DeactivateAsync()));

              await UniTask.WaitForSeconds(data.FirstDelay, false, PlayerLoopTiming.Update, myToken);

              await UniTask.WhenAll(
  stageCameraController.ChangeCameraSizeAsync(cameraBeginSize, cameraZoomInSize, data.ZoomInDuration, myToken),
  stageCameraController.PositonLerpAsync(cameraTargetPosition, data.ZoomInDuration, myToken));

              await PlayExitPartAsync(myToken);

              await UniTask.WaitForSeconds(data.EpilogueUIShowDelay);

            }
              break;

          case CameraMoveType.Round:
            {
              await UniTask.WhenAll(uiPresenters.Select(presenter => presenter.DeactivateAsync()));

              await UniTask.WaitForSeconds(data.FirstDelay, false, PlayerLoopTiming.Update, myToken);

              UniTask.WhenAll(
     stageCameraController.ChangeCameraSizeAsync(cameraBeginSize, cameraZoomInSize, data.ZoomInDuration, myToken),
     stageCameraController.PositonLerpAsync(cameraTargetPosition, data.ZoomInDuration, myToken)).Forget();

              await UniTask.WaitForSeconds(data.ZoomInDuration * 0.5f, false, PlayerLoopTiming.Update, myToken);
              await UniTask.WaitForSeconds(data.DelayAfterExitOpen, false, PlayerLoopTiming.Update, myToken);
              await PlayExitPartAsync(myToken);
              doctor.PlayAsync(null, myToken).Forget();
              var doctorMoveDuration = doctor.GetTotalMoveDuration();
              await UniTask.WaitForSeconds(doctorMoveDuration * data.DocMoveDelayRatio, false, PlayerLoopTiming.Update, myToken);

              stageCameraController.PositonLerpAsync(Vector3.zero, data.ZoomOutDuration + data.DelayAfterDoctorWork, myToken).Forget();
              await stageCameraController.ChangeCameraSizeAsync(cameraZoomInSize, cameraOriginSize, data.ZoomOutDuration, myToken);
            }
            break;
        }

        onComplete?.Invoke();
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask PlayExitPartAsync(CancellationToken token)
    {
      sfxController.PlayOnce(AudioSourceType.Center, SFX.DoctorDoor);
      await signal.PlayAsync(null, token);

      if (!isLastStage)
      {
        stageLightController.EnableDoctorLight(true);
        await UniTask.WaitForSeconds(data.DelayAfterSignalWork, false, PlayerLoopTiming.Update, token);
        sfxController.PlayOnce(AudioSourceType.Center, SFX.ExitDoorOpen);
        cameraEffectService.GenerateImpulse(ICameraEffectService.ImpulseType.ExitOpen);
        PlayExitActivate();
      }      
    }

    private void CacheAllUIPresenters()
    {
      uiPresenters.Clear();
      uiPresenters.AddRange(presenterContainer.GetAll<UIPlayerInputPresenter>());
      uiPresenters.Add(presenterContainer.GetFirst<UIPlayerEnergyPresenter>());

      var restartGuideUI = presenterContainer.GetFirst<UIStageRestartPresenter>();
      var practiceGuideUI = presenterContainer.GetFirst<UIPracticePresenter>();
      uiPresenters.Add(restartGuideUI);
      uiPresenters.Add(practiceGuideUI);

      var gimmickPresenter = presenterContainer.GetFirst<IUIStageGimmickPresenter>();
      if (gimmickPresenter != null)
        uiPresenters.Add(gimmickPresenter);
    }

    private void OnDestroy()
    {
      playCTS.Dispose();
    }
  }
}