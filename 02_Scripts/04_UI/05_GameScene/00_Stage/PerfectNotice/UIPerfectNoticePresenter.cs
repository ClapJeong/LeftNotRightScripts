using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Table.Player;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI.GameScene.Stage
{
  public class UIPerfectNoticePresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public IStageFailDataProvider stageFailDataProvider;
      [Inject] public IStageEventSubscriber stageEventSubscriber;
      [Inject] public IPlayerEnergySubscriber playerEnergySubscriber;
      [Inject] public ISFXController sfxController;
      [Inject] public PlayerEnergyDataSO playerEnergyDataSO;
      [Inject] public ColorSO colorSO;
      [Inject] public UISO uiSO;
    }

    private readonly Model model;
    private readonly UIPerfectNoticeView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer descriptionMoveCTS;
    private bool isRunning;

    public UIPerfectNoticePresenter(Model model, UIPerfectNoticeView view)
    {
      this.model = model;
      this.view = view;

      view.OutlineImage.color = model.colorSO.GetDifficultyClearColor(model.difficultyService.CurrentDifficulty);

      if (IsPerfectCleared())
      {
        DeactivateAsync(true).Forget();
      }
      else
      {
        subscribeHandle = new(() =>
        {
          model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);
          model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Complete, OnComplete);
          model.playerEnergySubscriber.SubscribeOnHit(OnHit);
        },
        () =>
        {
          model.stageEventSubscriber.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, OnRestart);
          model.stageEventSubscriber.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.Complete, OnComplete);
          model.playerEnergySubscriber.UnsubscribeOnHit(OnHit);
        });
        subscribeHandle.Subscribe();
        descriptionMoveCTS = new();
        isRunning = true;

        ActivateAsync().Forget();
      }  
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      descriptionMoveCTS?.Dispose();
      subscribeHandle?.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private bool IsCurrentDifficultyCleard()
    {
      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();
      var difficulty = model.difficultyService.CurrentDifficulty;
      return model.gameDataProvider.IsClearStageWithDifficulty(chapter, stage, difficulty);
    }

    private bool IsPerfectCleared()
    {
      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();
      var difficulty = model.difficultyService.CurrentDifficulty;

      return model.gameDataProvider.IsPerfect(chapter, stage, difficulty);
    }

    private bool IsBonusEnable()
    {
      var chapter = model.gameDataProvider.GetSelectedChapter();
      var stage = model.gameDataProvider.GetSelectedStage();
      var isCleared = model.gameDataProvider.IsClearStage(chapter, stage, out var _);

      var isDeathStepOver = model.stageFailDataProvider.FailCount >= model.playerEnergyDataSO.DeathStep;
      return !isCleared && isDeathStepOver;
    }


    private void OnRestart()
    {
      var isPerfectCleared = IsPerfectCleared();
      var isBonusEnable = IsBonusEnable();

      if (IsCurrentDifficultyCleard())
      {
        if (isPerfectCleared)
          view.HideAsync(true).Forget();
        else
        {
          isRunning = true;
          view.ShowAsync(true).Forget();
        }
      }
      else
      {
        if (isBonusEnable)
        {
          isRunning = false;

          OnPerfectDeactivate();
        }
        else
        {
          isRunning = true;
          view.ShowAsync(true).Forget();
        }
      }
    }

    private void OnComplete()
    {
      if(isRunning && IsPerfectCleared())
      {
        OnPerfectSuccess();
        DeactivateAsync().Forget();
        isRunning = false;
        subscribeHandle?.Unsubscribe();
      }
    }

    private void OnHit(PlayerType _, DamageType __)
    {
      if (!isRunning)
        return;

      OnPerfectFail();      

      isRunning = false;
    }

    private void OnPerfectSuccess()
    {      
      //view.IconAnimator.Play(AnimatorHash.PerfectNotice.Success);

      isRunning = false;
    }

    private void OnPerfectFail()
    {
      model.sfxController.PlayOnce(AudioSourceType.CenterUI, SFX.PerfectFail);
      view.IconAnimator.Play(AnimatorHash.PerfectNotice.Fail);
    }

    private void OnPerfectDeactivate()
    {
      descriptionMoveCTS.Cancel();
      descriptionMoveCTS.Create();
      var token = descriptionMoveCTS.token;
      MoveDescriptionAsync(token).Forget();
    }

    private async UniTask MoveDescriptionAsync(CancellationToken token)
    {
      try
      {
        view.DisableTextRect.anchoredPosition = Vector2.zero;
        view.DisableTextCanvasGroup.alpha = 1.0f;
        var iconFadeDuration = model.uiSO.Stage.PerfectIconFadeDuration;
        var textMoveLength = model.uiSO.Stage.PerfectFailTextMoveLength;
        var textMoveDuration = model.uiSO.Stage.PerfectFailTextMoveDuration;
        var textFadeDelay = textMoveDuration * model.uiSO.Stage.PerfectFailTextFadeBeginDurationRatio;
        var textFadeDuration = textMoveDuration - textFadeDelay;
        await DOTween
          .Sequence()
          .Join(view.IconCanvasGroup.DOFade(0.0f, iconFadeDuration))
          .Join(view.DisableTextRect.DOAnchorPosY(textMoveLength, textMoveDuration))
          .AppendInterval(textFadeDelay)
          .Append(view.DisableTextCanvasGroup.DOFade(0.0f, textFadeDuration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);

        view.gameObject.SetActive(false);
      }
      catch (OperationCanceledException) { }
    }
  }
}
