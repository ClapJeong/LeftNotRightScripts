using Cysharp.Threading.Tasks;
using LR.Manager.Device;
using LR.Stage.Player.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;

namespace LR.Stage.Player.GimmickGuide
{
  public class SwapGuideView : BaseGimmickGuideView
  {
    [SerializeField] private float swapDuration;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem swapEffect;
    [SerializeField] private SpriteRenderer bar;
    [SerializeField] private float pumpDuration;
    [SerializeField] private float pumpScale;

    private readonly CTSContainer swapCTS = new();
    private readonly CTSContainer pumpCTS = new();
    private ColorSO colorSO;
    private IDeviceEvnetSubscriber deviceEvnetSubscriber;
    private LRDeviceType currentDeviceType;
    private PlayerType playerType;
    private Dictionary<LRDeviceType, SpriteAtlas> atlases;
    private bool isSwapped = false;

    public override async UniTask InitializeAsync(PlayerType playerType, DiContainer diContainer)
    {
      await base.InitializeAsync(playerType, diContainer);

      this.playerType = playerType;      

      atlases = await InputAtlasProvider.GetInputAtlasesAsync(
        diContainer.Resolve<AddressableKeySO>(),
        diContainer.Resolve<IResourceManager>());

      this.colorSO = diContainer.Resolve<ColorSO>();
      bar.color = colorSO.GetPlayerColor(playerType);
      currentDeviceType = diContainer.Resolve<IDeviceProvider>().CurrentDeviceType;

      deviceEvnetSubscriber = diContainer.Resolve<IDeviceEvnetSubscriber>();
      deviceEvnetSubscriber.SubscribeDeviceEvent(OnDeviceChanged);
    }

    public void Swap(bool isSwapped, bool isImmediately)
    {
      swapCTS.Cancel();
      swapCTS.Create();
      var token = swapCTS.token;
      SwapAsync(isSwapped, isImmediately, token).Forget();

      var targetPlayerType = isSwapped ? playerType.ParseOpposite() : playerType;
      bar.color = colorSO.GetPlayerColor(targetPlayerType);
    }

    public void OnDuration(float normalized)
    {
      var current = bar.transform.localScale;
      current.x = normalized;
      bar.transform.localScale = current;
    }

    public async UniTask PumpAsync()
    {
      pumpCTS.Cancel();
      pumpCTS.Create();
      var token = pumpCTS.token;

      try
      {
        var targetTransform = spriteRenderer.transform;
        targetTransform.localScale = Vector3.one;

        var duration = 0.0f;
        while (duration < pumpDuration)
        {
          token.ThrowIfCancellationRequested();

          targetTransform.localScale = Vector3.one * Mathf.Lerp(1.0f, pumpScale, duration / pumpDuration);

          duration += Time.deltaTime;
          await UniTask.Yield();
        }

        duration = pumpDuration;

        while (duration > 0.0f)
        {
          token.ThrowIfCancellationRequested();

          targetTransform.localScale = Vector3.one * Mathf.Lerp(1.0f, pumpScale, duration / pumpDuration);

          duration -= Time.deltaTime;
          await UniTask.Yield();
        }

        targetTransform.localScale = Vector3.one;
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask SwapAsync(bool isSwapped, bool isImmediately, CancellationToken token)
    {
      this.isSwapped = isSwapped;
      var duration = 0.0f;
      var targetDuration = isImmediately ? 0.0f : swapDuration * 0.5f;
      try
      {
        while (duration < targetDuration)
        {
          token.ThrowIfCancellationRequested();

          contentTransform.eulerAngles = new Vector3(0.0f, Mathf.Lerp(0.0f, 180.0f, duration / targetDuration), 0.0f);

          duration += Time.deltaTime;
          await UniTask.Yield();
        }

        if(atlases.TryGetValue(currentDeviceType, out var atlas))
        {
          var spriteName = InputIconAssetName.GetInputPreviewName(isSwapped ? playerType.ParseOpposite() : playerType, false);
          var sprite = atlas.GetSprite(spriteName);
          spriteRenderer.sprite = sprite;
        }

        if (!isImmediately)
        {
          var color = colorSO.GetPlayerColor(isSwapped ? playerType.ParseOpposite() : playerType);
          var mainModule = swapEffect.main;
          mainModule.startColor = color;
          swapEffect.Play();
        }

        while (duration > 0.0f)
        {
          token.ThrowIfCancellationRequested();

          contentTransform.eulerAngles = new Vector3(0.0f, Mathf.Lerp(0.0f, 180.0f, duration / targetDuration), 0.0f);

          duration -= Time.deltaTime;
          await UniTask.Yield();
        }

        contentTransform.eulerAngles = Vector3.zero;
      }
      catch (OperationCanceledException) { }
    }

    private void OnDeviceChanged(LRDeviceType deviceType)
    {
      if (atlases.TryGetValue(deviceType, out var atlas))
      {
        var spriteName = InputIconAssetName.GetInputPreviewName(isSwapped ? playerType.ParseOpposite() : playerType, false);
        var sprite = atlas.GetSprite(spriteName);
        spriteRenderer.sprite = sprite;
      }

      currentDeviceType = deviceType;
    }

    private void OnDestroy()
    {
      pumpCTS.Dispose();
      swapCTS.Dispose();
      deviceEvnetSubscriber.UnsubscribeDeviceEvent(OnDeviceChanged);
    }
  }
}
