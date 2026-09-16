using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Local.StagePreview;
using LR.Manager.Stage;
using LR.Manager.UI;
using LR.Stage.StageDataContainer;
using LR.UI.Enum;
using LR.UI.Indicator;
using LR.UI.Lobby.ChapterPanel;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace LR.UI.Lobby
{
  public class UIChapterPanelPresenter : IUIPresenter
  {
    public class Model
    {      
      [Inject] public DiContainer diContainer;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public UISO uiSO;
      [Inject] public IUIDepthService depthService;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IGameModeService gameModeService;
      [Inject] public IResourceManager resourceManager;
      [Inject] public IUISelectedGameObjectService uiSelectedGameObjectService;
      [Inject] public IStagePreviewCreator stagePreviewCreator;
      [Inject] public ICanvasProvider canvasProvider;

      [Inject] public FloorSetter floorSetter;
      [Inject] public IUIIndicatorPresenter panelIndicator;
      [Inject] public UnityAction onPanelExit;
      [Inject] public Camera previewCamera;
    }
    private readonly Model model;
    private readonly UIChapterPanelView view;

    private readonly CTSContainer previewScaleCTS = new();
    private readonly CTSContainer previewWiggleCTS = new();
    private readonly CTSContainer previewWaitCTS = new();
    private readonly SubscribeHandle subscribeHandle;
    private readonly SubscribeHandle depthHandle;
    private readonly StageButtonSetService stageButtonSetService;

    private UIChapterPanelExitButtonPresenter exitButtonPresenter;
    private bool isFirstIndiactorMove = true;

    public UIChapterPanelPresenter(Model model, UIChapterPanelView view)
    {
      this.model = model;
      this.view = view;

      view.PreviewRawImage.texture = model.previewCamera.targetTexture;
      view.PreviewCanvasGroup.alpha = 0.0f;
      view.GimmickPreviewAnimator.Play(AnimatorHash.GimmickPreviewIcon.GetHash(StageGimmick.None));

      stageButtonSetService = model.diContainer.Instantiate<StageButtonSetService>(new object[]
      {
        view.ChapterButtonSetRoot,
        view.StageButtonSetCenterPosition,
        model.panelIndicator,
        view.StageButtonSetVertialLayoutGroup,
        view.ExitView
      });

      view.HideAsync(true).Forget();

      CreateExitButtonPresenter();
      CreateStageButtonSetsAsync().Forget();
      subscribeHandle = new(SubscribeSelectedGameObjectService, UnsubscribeSelectedGameObjectService);
      depthHandle = new(SubscribeCurrentDepth, UnsubscribeCurrentDepth);
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      stageButtonSetService.InitializeFirstPosition();
      previewWiggleCTS.Create();
      var previewToken = previewWiggleCTS.token;
      WigglePreviewAsync(previewToken).Forget();
      isFirstIndiactorMove = true;
      subscribeHandle.Subscribe();      
      await view.ShowAsync(isImmediately, token);
      depthHandle.Subscribe();
      exitButtonPresenter.ActivateAsync().Forget();      
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      stageButtonSetService.DeactivateAsync(isImmediately, token).Forget();
      subscribeHandle.Unsubscribe();
      depthHandle.Unsubscribe();
      exitButtonPresenter.DeactivateAsync().Forget();
      await view.HideAsync(isImmediately, token);
      previewWiggleCTS.Cancel();
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      previewWiggleCTS.Dispose();
      previewScaleCTS.Dispose();
      previewWaitCTS.Dispose();
      depthHandle.Dispose();
      subscribeHandle.Dispose();
      stageButtonSetService.Dispose();

      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private async UniTask CreateStageButtonSetsAsync()
    {
      var setCount = model.gameDataProvider.StageDataCount;
      var chapterCount = (setCount / StageConst.StageUnit) + ((setCount % StageConst.StageUnit > 0)? 1 : 0);

      var setViews = new List<UIStageButtonSetView>();
      var key = this.model.addressableKeySO.Path.UI + this.model.addressableKeySO.UIName.StageButtonSet;
      for (int i = 0; i < chapterCount; i++)
      {
        var view = await this.model.resourceManager.CreateAssetAsync<UIStageButtonSetView>(key, this.view.ChapterButtonSetRoot);
        setViews.Add(view);
      }

      UIStageButtonSetView prevView = null;
      for (int i = 0; i < chapterCount; i++)
      {
        var model = this.model.diContainer.Instantiate<UIStageButtonSetPresenter.Model>(new object[]
        {
          i + 1,
          this.model.panelIndicator,
          (UnityAction<int>)OnPreviewStage,
          (UnityAction)OnPreviewExit,
          (UnityAction)OnSelectStage,
        });
        var view = setViews[i];

        if (i != 0)
        {
          view.StageButtonViews[5].DirectionSet.Selectable.AddNavigation(Direction.Left, setViews[i - 1].Selectable);
          view.StageButtonViews[6].DirectionSet.Selectable.AddNavigation(Direction.Left, setViews[i - 1].Selectable);
          view.StageButtonViews[7].DirectionSet.Selectable.AddNavigation(Direction.Left, setViews[i - 1].Selectable);
        }
          
        if (i != chapterCount - 1)
        {
          view.StageButtonViews[1].DirectionSet.Selectable.AddNavigation(Direction.Right, setViews[i + 1].Selectable);
          view.StageButtonViews[2].DirectionSet.Selectable.AddNavigation(Direction.Right, setViews[i + 1].Selectable);
          view.StageButtonViews[3].DirectionSet.Selectable.AddNavigation(Direction.Right, setViews[i + 1].Selectable);
        }

        view.StageButtonViews[3].DirectionSet.Selectable.AddNavigation(Direction.Down, this.view.ExitView.Selectable);
        view.StageButtonViews[4].DirectionSet.Selectable.AddNavigation(Direction.Down, this.view.ExitView.Selectable);
        view.StageButtonViews[5].DirectionSet.Selectable.AddNavigation(Direction.Down, this.view.ExitView.Selectable);

        var presenter = new UIStageButtonSetPresenter(model, view);
        presenter.DeactivateAsync(true).Forget();
        presenter.AttachOnDestroy(this.view.gameObject);
        
        if (prevView != null)
        {
          prevView.Selectable.AddNavigation(Direction.Right, view.Selectable);
          view.Selectable.AddNavigation(Direction.Left, prevView.Selectable);
        }        
        view.Selectable.AddNavigation(Direction.Down, this.view.ExitView.Selectable);
        prevView = view;

        stageButtonSetService.AddMap(presenter, view);
      }      
    }

    private void OnPreviewStage(int index)
    {
      var chapter = index / StageConst.StageUnit + 1;
      view.PreviewCanvasGroup.alpha = 0.0f;
      view.GimmickPreviewAnimator.Play(AnimatorHash.GimmickPreviewIcon.GetHash(StageGimmick.None));
      previewWaitCTS.Cancel();
      previewWaitCTS.Create();
      var token = previewWaitCTS.token;
      model.stagePreviewCreator.CreatePreviewAsync(
        index,
        onComplete: stageDataContainer =>
        {
          //model.floorSetter.UpdateFloor(chapter);
          model.previewCamera.orthographicSize = stageDataContainer.cameraSize;
          view.PreviewCanvasGroup.alpha = 1.0f;

          ChangeGimmickPreviewAfterFrame(stageDataContainer.stageGimmick).Forget();
        },
        token).Forget();
    }

    private async UniTask ChangeGimmickPreviewAfterFrame(StageGimmick gimmick)
    {
      await UniTask.WaitForEndOfFrame();

      if(this != null)
        view.GimmickPreviewAnimator.Play(AnimatorHash.GimmickPreviewIcon.GetHash(gimmick));
    }

    private void OnPreviewExit()
    {
      view.GimmickPreviewAnimator.Play(AnimatorHash.GimmickPreviewIcon.GetHash(StageGimmick.None));
      view.PreviewCanvasGroup.alpha = 0.0f;
      previewWaitCTS.Cancel();
      model.stagePreviewCreator.ClosePreview();
    }

    private async UniTask WigglePreviewAsync(CancellationToken token)
    {
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var currentPos = view.PreviewRectTransform.anchoredPosition;

          var randomPos = new Vector2(
            UnityEngine.Random.Range(-model.uiSO.Lobby.PreviewWiggleRange, model.uiSO.Lobby.PreviewWiggleRange),
            UnityEngine.Random.Range(-model.uiSO.Lobby.PreviewWiggleRange, model.uiSO.Lobby.PreviewWiggleRange));

          var distance = Vector2.Distance(currentPos, randomPos);
          var duration = distance / model.uiSO.Lobby.PreviewWiggleSpeed;

          await view
            .PreviewRectTransform
            .DOAnchorPos(randomPos, duration)
            .ToUniTask(TweenCancelBehaviour.Kill, token);
        }
      }
      catch (OperationCanceledException) { }
    }

    private void OnSelectStage()
    {
      previewWiggleCTS.Cancel();
      depthHandle.Dispose();
      subscribeHandle.Dispose();
      stageButtonSetService.Dispose();
    }

    private void CreateExitButtonPresenter()
    {
      var model = this.model.diContainer.Instantiate<UIChapterPanelExitButtonPresenter.Model>(new object[]{
      this.model.panelIndicator,
      this.model.onPanelExit});
      exitButtonPresenter = new(model, view.ExitView);
      exitButtonPresenter.AttachOnDestroy(view.gameObject);
    }

    private void SubscribeSelectedGameObjectService()
    {
      model.uiSelectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, SetIndicatorTarget);
      model.uiSelectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, stageButtonSetService.OnSelectStageButtonSet);
    }

    private void UnsubscribeSelectedGameObjectService()
    {
      model.uiSelectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, SetIndicatorTarget);
      model.uiSelectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, stageButtonSetService.OnSelectStageButtonSet);
    }

    private void SubscribeCurrentDepth()
    {
      model.depthService.RaiseDepth(stageButtonSetService.GetFirstSelectButton());
    }

    private void UnsubscribeCurrentDepth()
    {
      model.depthService.LowerDepth();
    }

    private void SetIndicatorTarget(GameObject gameObject)
    {
      if (gameObject != null)
      {
        if (gameObject.TryGetComponent<BaseSubmitView>(out var submitView))
        {
          if(submitView.Selectable != null)
            model.panelIndicator.SetLeftInputGuide(submitView.Selectable.navigation);
        }          

        model.panelIndicator.MoveAsync(gameObject, isFirstIndiactorMove);
        isFirstIndiactorMove = false;
      }        
    }    
  }
}