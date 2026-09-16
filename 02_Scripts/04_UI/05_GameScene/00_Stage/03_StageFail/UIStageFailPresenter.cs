using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Input;
using LR.Manager.Scene;
using LR.Manager.Stage;
using LR.Manager.Stage.Practice;
using LR.Manager.UI;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using LR.UI.Indicator;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.GameScene.Stage
{
  public class UIStageFailPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public GlobalManager globalManager;
      [Inject] public IUIIndicatorService indicatorService;
      [Inject] public IStageStateHandler stageService;
      [Inject] public ISceneLoader sceneLoader;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IUIDepthService depthService;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public IGameDataSetter gameDataSetter;
      [Inject] public StageManager stageManager;
      [Inject] public UISO uiSO;
      [Inject] public IPracticeSubscriber practiceSubscriber;
      [Inject] public ColorSO colorSO;
      [Inject] public IStageRecorderService stageRecorderService;
      [Inject] public IRedPillModeService redPillModeService;
      [Inject] public IPlayerGetter playerGetter;
    }

    private readonly Model model;
    private readonly UIStageFailView view;    

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer quitThrottleCTS = new();
    private IUIIndicatorPresenter currentIndicator;
    private bool isquitThrottle = false;

    public UIStageFailPresenter(Model model, UIStageFailView view)
    {
      this.model = model;
      this.view = view;

      view.DoctorImage.enabled = model.stageManager.StageDataContainer.IsDoctorExist;

      view.RestartSubmitSet.Subscribe(
        onPerformed: OnRestart);

      view.QuitSubmitSet.Subscribe(
              onPerformed: OnQuit);

      view.LeftFailLogText.color = model.colorSO.LeftColor;
      view.RightFailLogText.color = model.colorSO.RightColor;

      subscribeHandle = new(
        onSubscribe: () =>
        {
          model.depthService.RaiseDepth(view.RestartSubmitSet.RectTransform.gameObject);
          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
          model.inputActionSubscriber.SubscribePhase(LRInputType.StageRestart, OnRestart, InputPhase.Performed);
          model.practiceSubscriber.SubscribeOnPractice(OnPracticeChanged);
        },
        onUnsubscribe: () =>
        {
          if (currentIndicator != null)
            ReleaseIndicator();

          model.depthService.LowerDepth();
          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.StageRestart, OnRestart, InputPhase.Performed);
          model.practiceSubscriber.SubscribeOnPractice(OnPracticeChanged);
        });
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.WaitForSeconds(model.uiSO.Stage.FailViewDelay);

      if (model.redPillModeService.IsRedPillEnable)
        DisplayDamagedLog();

      await view.ShowAsync(isImmediately, token);
      await GetNewIndicatorAsync();
      subscribeHandle.Subscribe();
      model.depthService.SelectTopObject();

      QuitInputThrottleAsync().Forget();
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      quitThrottleCTS.Cancel();

      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmediately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void Dispose()
    {
      quitThrottleCTS.Dispose();
      subscribeHandle.Dispose();

      if (view)
        view.DestroySelf();
    }

    private void DisplayDamagedLog()
    {
      UpdateDamagedLog(view.LeftFailLogText, PlayerType.Left);
      UpdateDamagedLog(view.RightFailLogText, PlayerType.Right);
    }

    private void UpdateDamagedLog(TextMeshProUGUI tmp, PlayerType playerType)
    {
      var result = model.stageRecorderService.GetPlayerDamageResult(playerType);
      if (result.results.Count > 0)
      {
        var stb = new StringBuilder();
        foreach (var damages in result.results.Values)
          foreach(var value in damages)
          {
            stb.Append("-");
            stb.Append(value.ToString("f2"));
            stb.Append("\n");
          }

        tmp.text = stb.ToString();
        var playerPosition = model.playerGetter.GetPlayer(playerType).GetMoveController().GetCurrentPosition();
      }
      else
      {
        tmp.text = string.Empty;
      }
    }


    private void OnPracticeChanged(bool isPractice)
    {
      DeactivateAsync().Forget();
    }

    private async UniTask QuitInputThrottleAsync()
    {
      quitThrottleCTS.Cancel();
      quitThrottleCTS.Create();
      var token = quitThrottleCTS.token;

      isquitThrottle = true;
      try
      {
        await UniTask.WaitForSeconds(model.uiSO.Stage.FailUIQuitThrottle, false, PlayerLoopTiming.Update, token);
        isquitThrottle = false;
      }
      catch (OperationCanceledException) { }
    }

    private void OnSelectedGameObjectEnter(GameObject gameObject)
    {
      if (gameObject.TryGetComponent<Selectable>(out var selectable))
        currentIndicator.SetLeftInputGuide(selectable.navigation);

      currentIndicator.MoveAsync(gameObject);
    }

    private void OnRestart()
    {
      currentIndicator.PlayGoodSubmitSFX();
      DeactivateAsync(true).Forget();
      model.stageService.RestartAsync().Forget();
    }

    private void OnQuit()
    {
      if (isquitThrottle)
        return;

      currentIndicator.PlayBadSubmitSFX();
      Dispose();
      model.gameDataSetter.ResetSelectedStage();
      model.globalManager.StopSpeedRun(false);
      model.globalManager.DisposeSpeedRunManager();
      model.sceneLoader.LoadSceneAsync(SceneType.Lobby).Forget();
    }

    private async UniTask GetNewIndicatorAsync()
    {
      currentIndicator = await model.indicatorService.GetNewAsync(view.IndicatorRoot, view.RestartSubmitSet.RectTransform);
    }

    private void ReleaseIndicator()
    {
      model.indicatorService.ReleaseTopIndicator();
      currentIndicator = null;
    }
  }
}