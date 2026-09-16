using Cysharp.Threading.Tasks;
using LR.Stage.SignalListener;
using LR.Stage.StageDataContainer;
using LR.Manager.Stage.Signal;
using System.Collections.Generic;
using LR.Manager.Sound;
using System.Linq;
using Zenject;
using LR.Manager.GameDataManager;

namespace LR.Manager.Stage.StageObject
{
  public class SignalListenerService :
    IStageObjectSetupService,
    IStageObjectControlService
  {
    [Inject] private readonly ISignalSubscriber signalSubscriber = null;
    [Inject] private readonly ISignalIDLifeProvider signalIDLifeProvider = null;
    [Inject] private readonly IResourceManager resourceManager = null;
    [Inject] private readonly ISFXController sfxController = null;
    [Inject] private readonly AddressableKeySO addressableKeySO = null;
    [Inject] private readonly ColorSO colorSO = null;
    [Inject] private readonly IDifficultyService difficultyService = null;

    private readonly List<BaseSignalPreview> previews = new();
    private List<SignalListener> signalListeners;
    private bool isSetup = false;

    public async UniTask SetupAsync(StageDataContainer stageDataContainer, bool isEnableImmediately = false)
    {
      signalListeners = stageDataContainer.SignalListeners;
      var currentDifficulty = difficultyService.CurrentDifficulty;

      foreach (var signalListener in signalListeners)
      {
        if (!signalListener.IsEnableDifficulty(currentDifficulty))
          continue;

        var signalKey = signalListener.RequireKey;
        signalSubscriber.SubscribeSignalActivate(signalKey, signalListener.OnActivate);
        signalSubscriber.SubscribeSignalDeactivate(signalKey, signalListener.OnDeactivate);

        var signalIDLifes = signalIDLifeProvider.GetSignalIDLifes(signalKey);
        signalListener.Initialize(
          signalIDLifes.Count, 
          sfxController,
          isContainACDC: signalIDLifes.Any(pair => pair.Value == LR.Stage.TriggerTile.Enum.SignalLife.ActivateAndDeactivate));

        var previewPositions = signalListener.GetPreviewPositions(signalIDLifes.Count);
        var count = 0;
        foreach (var pair in signalIDLifes)
        {
          var id = pair.Key;
          var life = pair.Value;
          var signalPreviewKey = addressableKeySO.Path.GameObjects + life switch
          {
            LR.Stage.TriggerTile.Enum.SignalLife.OnlyActivate => addressableKeySO.GameObjectName.ACSignalPreview,
            LR.Stage.TriggerTile.Enum.SignalLife.ActivateAndDeactivate => addressableKeySO.GameObjectName.ACDCSignalPreview,
            _ => throw new System.NotImplementedException(),
          };
          var signalPreview = await resourceManager.CreateAssetAsync<BaseSignalPreview>(signalPreviewKey, signalListener.transform);
          signalPreview.Initialize(previewPositions[count], colorSO.SignalColors[signalKey]);
          previews.Add(signalPreview);

          signalSubscriber.SubscribeIDActivate(signalKey, id, onActivate);
          signalSubscriber.SubscribeIDDeactivate(signalKey, id, onDeactivate);

          void onActivate(int activatedID)
          {
            if (activatedID != id)
              return;
            signalPreview.Activate();
          }
          void onDeactivate(int deactivatedID)
          {
            if (deactivatedID != id)
              return;
            signalPreview.Deactivate();
          }

          count++;
        }
      }
      isSetup = true;
    }

    public async UniTask AwaitUntilSetupCompleteAsync()
    {
      await UniTask.WaitUntil(() => isSetup);
    }

    public void EnableAll(bool isEnable)
    {
      foreach (var signalLister in signalListeners)
        signalLister.Enable(isEnable);
    }

    public void Release()
    {

    }

    public void RestartAll()
    {
      foreach (var signalLister in signalListeners)
        signalLister.Restart();

      foreach (var signalPreview in previews)
        signalPreview.Restart();
    }
  }
}