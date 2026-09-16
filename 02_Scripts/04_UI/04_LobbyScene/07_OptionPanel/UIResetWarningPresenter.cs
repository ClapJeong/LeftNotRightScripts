using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Scene;
using LR.Manager.Sound;
using LR.Manager.UI;
using LR.UI.Enum;
using LR.UI.Indicator;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.Lobby.Option
{
  public class UIResetWarningPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IGameDataIOController gameDataIOController;
      [Inject] public ISceneLoader sceneLoader;
      [Inject] public IUIDepthService depthService;
      [Inject] public UISO uiSO;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IBGMController bgmController;

      [Inject] public IUIIndicatorPresenter indicator;
    }

    private readonly Model model;
    private readonly UIResetWarningView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer moveCTS = new();

    private bool isPanelMoving = false;

    public UIResetWarningPresenter(Model model, UIResetWarningView view)
    {
      this.model = model;
      this.view = view;

      subscribeHandle = new(() =>
      {
        view.FirstAskSet.YesButton.Subscribe(OnFirstYes);
        view.FirstAskSet.NoButton.Subscribe(OnNo);
        view.LastAskSet.YesButton.Subscribe(OnLastYes);
        view.LastAskSet.NoButton.Subscribe(OnNo);

        model.depthService.RaiseDepth(view.FirstAskSet.YesButton.gameObject);
      },
      () =>
      {
        view.FirstAskSet.YesButton.Unsubscribe(OnFirstYes);
        view.FirstAskSet.NoButton.Unsubscribe(OnNo);
        view.LastAskSet.YesButton.Unsubscribe(OnLastYes);
        view.LastAskSet.NoButton.Unsubscribe(OnNo);

        model.depthService.LowerDepth();
      });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {      
      await view.ShowAsync(isImmedieately, token);
      
      subscribeHandle?.Subscribe();
      isPanelMoving = false;
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle?.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      moveCTS?.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnFirstYes()
    {
      if (isPanelMoving)
        return;

      model.indicator.PlayGoodSubmitSFX();
      MovePanelsAsync().Forget();
    }

    private async UniTask MovePanelsAsync()
    {
      moveCTS.Cancel();
      moveCTS.Create();
      var token = moveCTS.token;
      try
      {
        isPanelMoving = true;

        model.selectedGameObjectService.SetSelectedObject(view.LastAskSet.YesButton.gameObject);

        var duration = model.uiSO.Lobby.DataResetPanelChangeDuration;
        var screenHeight = Screen.height;
        await DOTween
          .Sequence()
          .Join(view.FirstAskSet.CanvasGroup.DOFade(0.0f, duration))
          .Join(view.FirstAskSet.RectTransform.DOAnchorPosY(screenHeight, duration))
          .Join(view.LastAskSet.CanvasGroup.DOFade(1.0f, duration))
          .Join(view.LastAskSet.RectTransform.DOAnchorPosY(0.0f, duration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);        

        isPanelMoving = false;
      }
      catch (OperationCanceledException) { }
    }

    private async void OnLastYes()
    {
      if (isPanelMoving)
        return;

      subscribeHandle.Dispose();

      model.gameDataIOController.ResetData();
      await model.gameDataIOController.SaveDataAsync();

      model.bgmController.StopBGMAsync().Forget();
      model.sceneLoader.LoadSceneAsync(SceneType.Preloading).Forget();
    }

    private void OnNo()
    {
      if (isPanelMoving)
        return;

      model.indicator.PlayGoodSubmitSFX();
      DeactivateAsync().Forget();
    }
  }
}