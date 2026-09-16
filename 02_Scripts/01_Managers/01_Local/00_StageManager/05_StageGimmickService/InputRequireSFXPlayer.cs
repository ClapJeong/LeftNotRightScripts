using Cysharp.Threading.Tasks;
using LR.Manager.Sound;
using LR.Table.StageGimmick;
using System;
using UnityEngine;
using Zenject;

namespace LR.Manager.Stage.Gimmick.InputRequires
{
  public class InputRequireSFXPlayer : IDisposable
  {
    [Inject] private readonly ISFXController sfxController = null;
    [Inject] private readonly IStageStateProvider stageStateProvider = null;
    private readonly InputRequire.ValueSet leftValueSet;
    private readonly InputRequire.ValueSet rightValueSet;
    private readonly InputRequireData data;

    private readonly CTSContainer cts = new();

    public InputRequireSFXPlayer(
      InputRequire.ValueSet leftValueSet,
      InputRequire.ValueSet rightValueSet,
      InputRequireData data)
    {
      this.leftValueSet = leftValueSet;
      this.rightValueSet = rightValueSet;
      this.data = data;
    }

    public async UniTask PlayAsync()
    {
      cts.Cancel();
      cts.Create();
      var token = cts.token;
      var flag = false;
      try
      {
        var time = data.ClockIntervalMax;
        while (true)
        {
          token.ThrowIfCancellationRequested();
          if (!stageStateProvider.IsPlayingState)
          {
            await UniTask.Yield();
            continue;
          }
          
          if(leftValueSet.isRegen || rightValueSet.isRegen)
          {
            await UniTask.Yield();
            continue;
          }

          time -= Time.deltaTime;
          if (time <= 0.0f)
          {
            var minNormalized = Mathf.Min(leftValueSet.NormalizedValue, rightValueSet.NormalizedValue);
            time = Mathf.Lerp(data.ClockIntervalMin, data.ClockIntervalMax, minNormalized);
            flag = !flag;

            var volume = Mathf.Lerp(data.ClockSoundMin, 1.0f, 1.0f - minNormalized);

            sfxController.PlayOnce(AudioSourceType.Center, flag ? SFX.Clock_0 : SFX.Clock_1, true, volume);
          }
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
      cts.Dispose();
    }
  }
}
