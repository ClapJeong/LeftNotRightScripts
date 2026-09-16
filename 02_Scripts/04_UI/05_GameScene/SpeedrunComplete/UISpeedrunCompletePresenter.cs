using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Input;
using LR.Manager.Scene;
using LR.Manager.Sound;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.UI.Enum;
using LR.UI.GameScene.GlobalRecord;
using LR.UI.GameScene.Player;
using LR.UI.GameScene.Stage;
using LR.UI.GameScene.StageGimmick;
using LR.UI.Indicator;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.GameScene.Speedrun
{
  public class UISpeedrunCompletePresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public GlobalManager globalManager;
      [Inject] public IGameDataSetter gameDataSetter;
      [Inject] public ISceneLoader sceneLoader;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public IUIDepthService depthService;
      [Inject] public IUIIndicatorService indicatorService;
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public ISFXController sfxController;
      [Inject] public IEffectService effectService;
      [Inject] public IGameDataIOController gameDataIOController;
      [Inject] public IStoreTypeProvider storeTypeProvider;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
    }

    private readonly Model model;
    private readonly UISpeedrunCompleteView view;

    private readonly UISpeedrunLeaderboardPresenter safestLeaderboardPresenter;
    private readonly UISpeedrunLeaderboardPresenter fastestLeaderboardPresenter;
    private readonly SubscribeHandle subscribeHandle;
    private IUIIndicatorPresenter currentIndicator;

    public UISpeedrunCompletePresenter(Model model, UISpeedrunCompleteView view)
    {
      this.model = model;
      this.view = view;

      view.ExitButton.Subscribe(
        onPerformed: OnQuit);

      if (model.storeTypeProvider.StoreType == StoreType.Steam)
      {
        var fastestModel = model.diContainer.Instantiate<UISpeedrunLeaderboardPresenter.Model>(new object[] { StoreKeys.FastestSpeedRun, true });
        fastestLeaderboardPresenter = new(fastestModel, view.FastestLeaderboardView);
        fastestLeaderboardPresenter.AttachOnDestroy(view.gameObject);

        var safestModel = model.diContainer.Instantiate<UISpeedrunLeaderboardPresenter.Model>(new object[] { StoreKeys.SafestSpeedRun, false });
        safestLeaderboardPresenter = new(safestModel, view.SafestLeaderboardView);
        safestLeaderboardPresenter.AttachOnDestroy(view.gameObject);
      }
      else
      {
        view.FastestLeaderboardView.gameObject.SetActive(false);
        view.SafestLeaderboardView.gameObject.SetActive(false);
      }

      subscribeHandle = new(
        () =>
        {
          model.depthService.RaiseDepth(view.ExitButton.RectTransform.gameObject);
          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameobject);
        },
        () =>
        {
          if (currentIndicator != null)
            ReleaseIndicator();

          model.depthService.LowerDepth();
          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameobject);
        });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      model.presenterContainer.GetFirst<UIPlayerEnergyPresenter>()?.DeactivateAsync().Forget();
      model.presenterContainer.GetFirst<UIStageRestartPresenter>()?.DeactivateAsync().Forget();
      model.presenterContainer.GetFirst<UIPracticePresenter>()?.DeactivateAsync().Forget();
      var stageGimmickUI = model.presenterContainer.GetFirst<IUIStageGimmickPresenter>();
      stageGimmickUI?.DeactivateAsync().Forget();
      UniTask.WhenAll(model
        .presenterContainer
        .GetAll<UIPlayerInputPresenter>()
        .Select(presenter => presenter.DeactivateAsync()))
        .Forget();
      await UniTask.WhenAll(model
        .presenterContainer
        .GetAll<UIPlayerStatePortraitPresenter>()
        .Select(presenter => presenter.HideForSpeedrunAsync()));
      
      model.sfxController.PlayOnce(AudioSourceType.CenterUI, SFX.SpeedRunComplete);
      model.effectService.Create(InstanceEffectType.SpeedRunComplete, Vector3.zero, Quaternion.identity);

      await view.ShowAsync(isImmedieately, token);
      subscribeHandle.Subscribe();
      await GetNewIndicatorAsync();
      currentIndicator.SetLeftInputGuide(new List<Direction>());
      await currentIndicator.MoveAsync(view.ExitButton);

      if(fastestLeaderboardPresenter != null)
       await fastestLeaderboardPresenter.ActivateAsync(isImmedieately, token);
      safestLeaderboardPresenter?.ActivateAsync(isImmedieately, token).Forget();

    }    

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnSelectedGameobject(GameObject gameObject)
    {
      if (currentIndicator != null)
      {
        if(gameObject.TryGetComponent<Selectable>(out var selectable))
          currentIndicator.SetLeftInputGuide(selectable.navigation);

        currentIndicator.MoveAsync(gameObject).Forget();
      }         
    }

    private void OnQuit()
    {
      Dispose();
      model.globalManager.DisposeSpeedRunManager();
      model.gameDataSetter.ResetSelectedStage();
      model.sceneLoader.LoadSceneAsync(SceneType.Lobby).Forget();
    }

    private async UniTask GetNewIndicatorAsync()
    {
      currentIndicator = await model.indicatorService.GetNewAsync(view.IndicatorRoot, view.ExitButton.RectTransform);
    }

    private void ReleaseIndicator()
    {
      model.indicatorService.ReleaseTopIndicator();
      currentIndicator = null;
    }
  }
}