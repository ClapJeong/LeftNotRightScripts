using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace LR.UI.GameScene.Player
{
  public class UIPlayerDamageLogPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IPlayerEnergySubscriber playerEnergySubscriber;
      [Inject] public UISO uiSO;
      [Inject] public ColorSO colorSO;
      [Inject] public IRedPillModeService redPillModeService;
      [Inject] public PlayerType playerType;
    }

    private readonly Model model;
    private readonly UIPlayerDamageLogView view;

    private readonly List<UINumberFontObject> aliveTexts = new();
    private readonly Queue<UINumberFontObject> deadTexts = new();
    private readonly CTSContainer fontMoveCTS = new();

    public UIPlayerDamageLogPresenter(Model model, UIPlayerDamageLogView view)
    {
      this.model = model;
      this.view = view;

      model.playerEnergySubscriber.SubscribeOnDamageValue(OnPlayerDamaged);
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }    

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      fontMoveCTS.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnPlayerDamaged(PlayerType playerType, float value)
    {
      if (!model.redPillModeService.IsRedPillEnable)
        return;

      if (model.playerType != playerType)
        return;

      var enableText = GetEnableText();
      enableText.UpdateText(-value);
      enableText.UpdateColor(model.colorSO.GetPlayerColor(model.playerType));
      enableText.UpdateScale(model.uiSO.Player.DamageLog.Scale);
      var token = fontMoveCTS.token;
      PlayFontObejectAsync(enableText, value, token).Forget();
    }

    private UINumberFontObject GetEnableText()
    {
      if(deadTexts.TryDequeue(out UINumberFontObject queueText))
      {
        aliveTexts.Add(queueText);
        return queueText;
      }
      else
      {
        var newText = GameObject.Instantiate(view.TextPrefab, view.TextRoot);
        aliveTexts.Add(newText);
        return newText;
      }
    }

    private async UniTask PlayFontObejectAsync(UINumberFontObject fontObject, float damage, CancellationToken token)
    {
      var rectTransform = fontObject.RectTransform;
      var canvasGroup = fontObject.CanvasGroup;

      var randomX = UnityEngine.Random.Range(-model.uiSO.Player.DamageLog.RandomXRange, model.uiSO.Player.DamageLog.RandomXRange);
      var randomY = UnityEngine.Random.Range(-model.uiSO.Player.DamageLog.RandomYRange, model.uiSO.Player.DamageLog.RandomYRange);
      rectTransform.anchoredPosition = new Vector2(randomX, randomY);

      var unit = (damage / model.uiSO.Player.DamageLog.DamageUnit);
      var length = unit * model.uiSO.Player.DamageLog.LengthPerUnit;
      var duration = unit * model.uiSO.Player.DamageLog.SecondPerUnit;

      canvasGroup.alpha = 1.0f;

      try
      {        
        var targetPos = rectTransform.anchoredPosition + Vector2.up * length;
        var fadeBeginDuration = duration * model.uiSO.Player.DamageLog.FadeBeginDurationRatio;
        var fadeDuration = duration - fadeBeginDuration;
        await DOTween
          .Sequence()
          .Join(rectTransform.DOAnchorPos(targetPos, duration))
          .AppendInterval(fadeBeginDuration)
          .Append(canvasGroup.DOFade(0.0f, fadeDuration))
          .AppendCallback(() =>
          {
            aliveTexts.Remove(fontObject);
            deadTexts.Enqueue(fontObject);
          })
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException)
      {
        if (this != null)
          canvasGroup.alpha = 0.0f;
      }
    }
  }
}