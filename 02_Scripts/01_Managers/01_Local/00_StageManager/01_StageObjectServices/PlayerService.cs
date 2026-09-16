using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Stage.Player;
using LR.Stage.Player.Enum;
using LR.Stage.StageDataContainer;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace LR.Manager.Stage.StageObject
{
  public class PlayerService :
  IStageObjectSetupService,
  IStageObjectControlService,
  IDisposable
  {
    [Inject] private readonly DiContainer diContainer = null;
    [Inject] private readonly TableContainer tableContainer = null;
    [Inject] private readonly IResourceManager resourceManager = null;
    [Inject] private readonly IDifficultyService difficultyService = null;

    public BothPlayerEnergyContainer EnergyContainer
      => energyContainer;

    private readonly List<string> playerKeys = new();
    private BothPlayerEnergyContainer energyContainer;
    private IPlayerPresenter leftPlayer;
    private IPlayerPresenter rightPlayer;

    private bool isSetupComplete = false;

    public async UniTask SetupAsync(StageDataContainer stageDataContainer, bool isEnableImmediately = false)
    {
      var maxEnergy = stageDataContainer.GetTotalEnergy(difficultyService.CurrentDifficulty);
      energyContainer = diContainer.Instantiate<BothPlayerEnergyContainer>(new object[] { maxEnergy });
      diContainer.BindInterfacesAndSelfTo<BothPlayerEnergyContainer>().FromInstance(energyContainer);

      var root = stageDataContainer.playerRoot;
      var leftPosition = stageDataContainer.leftPlayerBeginTransform.position;
      var rightPosition = stageDataContainer.rightPlayerBeginTransform.position;

      leftPlayer = await CreatePlayerAsync(PlayerType.Left, leftPosition, root, stageDataContainer.stageGimmick);
      rightPlayer = await CreatePlayerAsync(PlayerType.Right, rightPosition, root, stageDataContainer.stageGimmick);      

      leftPlayer.Enable(false);
      rightPlayer.Enable(false);      

      isSetupComplete = true;

      energyContainer.InitializePlayers();

      await UniTask.WaitForEndOfFrame();
    }

    public void Release()
    {
      leftPlayer
        .GetInputActionSubscriber()
        .Dispose();
      rightPlayer
        .GetInputActionSubscriber()
        .Dispose();
    }

    private async UniTask<IPlayerPresenter> CreatePlayerAsync(
      PlayerType playerType, 
      Vector3 beginPosition, 
      Transform root,
      StageGimmick currentGimmick)
    {
      var addressableKeySO = tableContainer.AddressableKeySO;
      var playerKey =
        addressableKeySO.Path.Player +
        addressableKeySO.GameObjectName.GetPlayerName(playerType);
      playerKeys.Add(playerKey);

      var view = await resourceManager.CreateAssetAsync<BasePlayerView>(playerKey, root);
      var model = diContainer.Instantiate<PlayerModel>(new object[] {
        playerType, 
        beginPosition});
      var presenter = new BasePlayerPresenter(model, view);
      await presenter.CreateGimmickGuideAsync(currentGimmick);

      return presenter;
    }

    public void EnableAll(bool isEnable)
    {
      leftPlayer
        .GetInputActionController()
        .EnableAllInputActions(isEnable);
      rightPlayer
        .GetInputActionController()
        .EnableAllInputActions(isEnable);
    }

    public void RestartAll()
    {
      energyContainer.Restart();
      leftPlayer.Restart();
      rightPlayer.Restart();
    }

    public IPlayerPresenter GetPlayer(PlayerType playerType)
    {
      if (!isSetupComplete)
        AwaitUntilSetupCompleteAsync().GetAwaiter().GetResult();

      return playerType switch
      {
        PlayerType.Left => leftPlayer,
        PlayerType.Right => rightPlayer,
        _ => throw new System.NotImplementedException(),
      };
    }

    public async UniTask AwaitUntilSetupCompleteAsync()
    {
      await UniTask.WaitUntil(() => isSetupComplete);
    }

    public bool IsAllPlayerExist()
      => leftPlayer != null && rightPlayer != null;

    public void Dispose()
    {
      energyContainer?.Dispose();
      foreach(var playerKey in playerKeys)
        resourceManager.ReleaseAsset(playerKey);
    }
  }
}