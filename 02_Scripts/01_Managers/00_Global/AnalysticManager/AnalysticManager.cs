using Cysharp.Threading.Tasks;
using System;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.UnityConsent;

namespace LR.Manager.Analystic
{
  public class AnalysticManager :
    IStageAnalysticService,
    IDialogueAnalysticService
  {
    private bool isInitialized;

    public AnalysticManager()
    {
      InitializeAsync().Forget();
    }

    public void SendDialogueEvent(int index, bool isSkipped)
    {
      if (!isInitialized)
        return;

      var dialogueSkipEvent = new DialogueSkipEvent()
      {
        DialogueIndex = index,
        DialogueSkipped = isSkipped
      };

      AnalyticsService.Instance.RecordEvent(dialogueSkipEvent);
    }

    public void SendStageClearEvent(int index, int deathCount, int easyRestartCount, int hardRestartCount)
    {
      if (!isInitialized)
        return;

      var stageClearEvent = new StageEvent()
      {
        StageIndex = index,
        DeathCount = deathCount,
        EasyRestartCount = easyRestartCount,
        HardRestartCount = hardRestartCount,
      };

      AnalyticsService.Instance.RecordEvent(stageClearEvent);
    }

    private async UniTask InitializeAsync()
    {
      try
      {
        await UnityServices.InitializeAsync(new InitializationOptions()
            .SetEnvironmentName("production"));
        var consentState = new ConsentState
        {
          AdsIntent = ConsentStatus.Denied,
          AnalyticsIntent = ConsentStatus.Granted,
        };
        EndUserConsent.SetConsentState(consentState);
        isInitialized = true;
        //AnalyticsService.Instance.StartDataCollection();
      }
      catch (Exception e)
      {
        UnityEngine.Debug.LogError($"Analytics 초기화 실패: {e.Message}");
      }
    }
  }
}
