using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Stage;
using LR.Manager.Stage.Practice;
using LR.Manager.UI;
using LR.Stage.Player;
using LR.Table.Player;
using LR.UI.Enum;
using System;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerEnergyPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IUIPresenterContainer presenterContainer;
      [Inject] public IPlayerEnergySubscriber energySubscriber;
      [Inject] public IPlayerEnergyProvider energyProvider;
      [Inject] public PlayerEnergyDataSO playerEnergyData;
      [Inject] public IStageEventSubscriber stageEventSubscriber;
      [Inject] public UISO uiSO;
      [Inject] public IPracticeSubscriber practiceSubscriber;
    }

    private readonly Model model;
    private readonly UIPlayerEnergyView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer practiceCTS = new();
    private readonly CTSContainer restoreCTS = new();
    private readonly CTSContainer damagedCTS = new();
    private readonly CTSContainer hitCTS = new();
    private IDisposable viewUpdateObserver;

    private float prevLeftNormalized;
    private float prevRightNormalized;

    private float leftNormalized;
    private float rightNormalized;

    private float leftValueChangingDuration;
    private float rightValueChangingDuration;

    public UIPlayerEnergyPresenter(Model model, UIPlayerEnergyView view)
    {
      this.model = model;
      this.view = view;

      view.FillImage.material.SetFloat(ShaderHash.EnergyBar._OutlineEnable, 0.0f);

      subscribeHandle = new( SubscribePlayerEnergy, UnsubscribePlayerEnergy);

      model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Restart, StopAlert);
      model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.Exhausted, StopAlert);

      model.practiceSubscriber.SubscribeOnPractice(OnPractice);

      model
        .presenterContainer
        .Add(this);
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      leftValueChangingDuration = 0.0f;
      rightValueChangingDuration = 0.0f;
      subscribeHandle.Subscribe();
      await view.ShowAsync(isImmedieately, token);
    }    

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {      
      await view.HideAsync(isImmedieately, token);
      subscribeHandle.Unsubscribe();
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      model
        .presenterContainer
        .Remove(this);
      practiceCTS.Dispose();
      restoreCTS.Dispose();
      damagedCTS.Dispose();
      hitCTS.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void SubscribePlayerEnergy()
    {
      viewUpdateObserver = view
        .UpdateAsObservable()
        .Subscribe(_ =>
        {
          UpdateLeftFillNormalized();
          UpdateRightFillNormalized();

          var scale = Mathf.Clamp(leftNormalized + rightNormalized, 0.0f, 1.0f);
          view.UpdateArrowScale(scale);
          //view.FillImageRectTransform.localScale = new Vector3(scale, 1.0f, 1.0f);
          view.FillImage.material.SetFloat(ShaderHash.EnergyBar._Fill, scale);

          //var moveNormalized = leftNormalized - rightNormalized;
          //var targetPosition = view.FillImageRectTransform.rect.width * 0.5f * moveNormalized;
          //view.FillImageRectTransform.anchoredPosition = new Vector3(targetPosition, 0.0f, 0.0f);
          //view.CenterRectTransform.anchoredPosition = new Vector3(targetPosition, 0.0f, 0.0f);
        });

      model.energySubscriber.SubscribeValueEvent(IPlayerEnergySubscriber.ValueEvent.LeftDamaged, OnLeftDamaged);
      model.energySubscriber.SubscribeValueEvent(IPlayerEnergySubscriber.ValueEvent.RightDamaged, OnRightDamaged);
      model.energySubscriber.SubscribeThreshhold(IPlayerEnergySubscriber.Threshhold.Below, model.uiSO.Player.PortraitLowEnergy, PlayAlert);
      model.energySubscriber.SubscribeThreshhold(IPlayerEnergySubscriber.Threshhold.Above, model.uiSO.Player.PortraitLowEnergy, StopAlert);
      model.stageEventSubscriber.SubscribeOnEvent(IStageEventSubscriber.StageEventType.AllClearEnter, StopAlert);
    }

    private void UpdateLeftFillNormalized()
    {
      var currentLeftNormalized = model.energyProvider.LeftEnergyNormalized;

      if (leftValueChangingDuration > 0.0f)
      {
        var t = 1.0f - leftValueChangingDuration / model.uiSO.Player.EnergyChangedUIDuration;
        this.leftNormalized = Mathf.Lerp(prevLeftNormalized, currentLeftNormalized, t);
        leftValueChangingDuration -= Time.deltaTime;
      }
      else
      {
        this.leftNormalized = currentLeftNormalized;
      }
    }

    private void UpdateRightFillNormalized()
    {
      var currentRightNormalized = model.energyProvider.RightEnergyNormalized;

      if (rightValueChangingDuration > 0.0f)
      {
        var t = 1.0f - rightValueChangingDuration / model.uiSO.Player.EnergyChangedUIDuration;
        this.rightNormalized = Mathf.Lerp(prevRightNormalized, currentRightNormalized, t);
        rightValueChangingDuration -= Time.deltaTime;
      }
      else
      {
        this.rightNormalized = currentRightNormalized;
      }
    }

    private void OnPractice(bool isPractice)
    {
      practiceCTS.Cancel();
      practiceCTS.Create();
      var token = practiceCTS.token;
      if (isPractice)
        DeactivateAsync(false, token).Forget();
      else
        ActivateAsync(false, token).Forget();
    }

    private void UnsubscribePlayerEnergy()
    {
      viewUpdateObserver.Dispose();
      model.energySubscriber.UnsubscribeValueEvent(IPlayerEnergySubscriber.ValueEvent.LeftDamaged, OnLeftDamaged);
      model.energySubscriber.UnsubscribeValueEvent(IPlayerEnergySubscriber.ValueEvent.RightDamaged, OnRightDamaged);
      model.energySubscriber.UnsubscribeThreshhold(IPlayerEnergySubscriber.Threshhold.Below, model.uiSO.Player.PortraitLowEnergy, PlayAlert);
      model.energySubscriber.UnsubscribeThreshhold(IPlayerEnergySubscriber.Threshhold.Above, model.uiSO.Player.PortraitLowEnergy, StopAlert);
      model.stageEventSubscriber.UnsubscribeOnEvent(IStageEventSubscriber.StageEventType.AllClearEnter, StopAlert);
    }

    private void OnLeftDamaged(float damagedNormalized)
    {
      prevLeftNormalized = model.energyProvider.LeftEnergyNormalized + damagedNormalized;
      leftValueChangingDuration = model.uiSO.Player.EnergyChangedUIDuration;

      damagedCTS.Cancel();
      damagedCTS.Create();
      var token = damagedCTS.token;
      PlayDamagedBlinkAsync(token).Forget();
    }

    private void OnRightDamaged(float damagedNormalized)
    {
      prevRightNormalized = model.energyProvider.RightEnergyNormalized + damagedNormalized;
      rightValueChangingDuration = model.uiSO.Player.EnergyChangedUIDuration;

      damagedCTS.Cancel();
      damagedCTS.Create();
      var token = damagedCTS.token;
      PlayDamagedBlinkAsync(token).Forget();
    }

    private async UniTask PlayDamagedBlinkAsync(CancellationToken token)
    {
      try
      {
        var interval = model.uiSO.Player.EnergyChangedUIDuration / (model.uiSO.Player.EnergyChangedBlinkCount * 2.0f);
        var sequence = DOTween.Sequence();
        for(int i = 0; i < model.uiSO.Player.EnergyChangedBlinkCount; i++)
        {
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
          sequence
            .AppendCallback(() =>
            {
              view.FillImage.SetAlpha(0.0f);
            })
            .AppendInterval(interval)
            .AppendCallback(() =>
            {
              view.FillImage.SetAlpha(1.0f);
            })
            .AppendInterval(interval);
        }
        sequence.OnComplete(() => { view.FillImage.SetAlpha(1.0f); });
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
        await sequence.ToUniTask(TweenCancelBehaviour.Complete, token);
      }
      catch (OperationCanceledException)
      {
        view.FillImage.SetAlpha(1.0f);
      }
    }

    private void PlayAlert()
    {
      view.FillImage.material.SetFloat(ShaderHash.EnergyBar._OutlineEnable, 1.0f);
    }

    private void StopAlert()
    {
      view.FillImage.material.SetFloat(ShaderHash.EnergyBar._OutlineEnable, 0.0f);
    }
  }
}
