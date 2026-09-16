using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using LR.UI.Enum;
using LR.Manager.Stage;
using LR.Manager.Sound;
using LR.Manager.Input;
using Zenject;
using LR.Manager.GameDataManager;

namespace LR.UI.GameScene.Stage
{
  public class UIStageBeginPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public ColorSO colorSO;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IGameModeService gameModeService;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public IStageStateHandler stageService;
      [Inject] public ISFXController sfxController;
      [Inject] public IStageStateProvider stageStateProvider;
      [Inject] public LR.Stage.StageDataContainer.StageGimmick currentStageGimmick;
    }
    

    private readonly Model model;
    private readonly UIStageBeginView view;

    private SubscribeHandle subscribeHandle;

    private bool enableInput = false;

    public UIStageBeginPresenter(Model model, UIStageBeginView view)
    {
      this.model = model;
      this.view = view;

      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();
      view.StageInfoTMP.text = $"{chapter}-{stage}";

      if (model.currentStageGimmick == LR.Stage.StageDataContainer.StageGimmick.None)
      {
        view.GimmickDescriptionTMP.text = "";
      }
      else
      {        
        view.GimmickLocalizeStringEvent.SetEntry($"gimmick_{model.currentStageGimmick}_description");
      }        
      CreateSubscribeHandle();
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmediately, token);
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      if (model.gameModeService.IsSpeedRun)
        isImmediately = true;

      await view.ShowAsync(isImmediately, token);
      subscribeHandle.Subscribe();
      view.GimmickIconAnimator.Play(AnimatorHash.GimmickPreviewIcon.GetHash(model.currentStageGimmick));
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.OnDestroyAsObservable().Subscribe(_ => Dispose());

    public void Dispose()
    {
      subscribeHandle.Dispose();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void EnableInput(bool enableInput)
      => this.enableInput = enableInput;

    private void CreateSubscribeHandle()
    {
      subscribeHandle = new SubscribeHandle(
        onSubscribe: () =>
        {
          model.inputActionSubscriber.SubscribePhase(LRInputType.LeftAny, OnAnyLeftPerformed, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.RightAny, OnAnyRightPerformed, InputPhase.Performed);
        },
        onUnsubscribe: () =>
        {
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.LeftAny, OnAnyLeftPerformed, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.RightAny, OnAnyRightPerformed, InputPhase.Performed);
        });
    }

    private void OnAnyLeftPerformed()
    {
      if (!enableInput || model.stageStateProvider.GetState() != StageEnum.State.Ready)
        return;

      BeginStage();
    }

    private void OnAnyRightPerformed()
    {
      if (!enableInput || model.stageStateProvider.GetState() != StageEnum.State.Ready)
        return;

      BeginStage();
    }

    private void BeginStage()
    {
      model.stageService.BeginStage();
      DeactivateAsync().Forget();
    }
  }
}