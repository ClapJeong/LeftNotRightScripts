using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.Manager.Stage.Gimmick;
using LR.Stage.Player.Enum;
using LR.Stage.StageDataContainer;
using Zenject;

namespace LR.Manager.Stage
{
  public class GimmickInitializer
  {
    private readonly DiContainer diContainer;
    private readonly StageGimmickSO stageGimmickSO;
    private readonly IPlayerGetter playerGetter;
    private readonly IStageStateProvider stageStateProvider;
    private readonly ISFXController sfxController;

    public GimmickInitializer(
      DiContainer diContainer,
      StageGimmickSO stageGimmickSO, 
      IPlayerGetter playerGetter, 
      IStageStateProvider stageStateProvider, 
      ISFXController sfxController)
    {
      this.diContainer = diContainer;
      this.stageGimmickSO = stageGimmickSO;
      this.playerGetter = playerGetter;
      this.stageStateProvider = stageStateProvider;
      this.sfxController = sfxController;
    }

    public async UniTask<IStageGimmick> InitializeStageGimmickAsync(StageGimmick gimmick)
    {
      var param = new object[1];
      IStageGimmick stageGimmick = null;
      switch (gimmick)
      {
        case StageGimmick.CameraRotator:
          {
            param[0] = stageGimmickSO.CameraBalanceData;
            stageGimmick = diContainer.Instantiate<CameraRotatorGimmick>(param);
          }
          break;

        case StageGimmick.CameraMover:
          {
            param[0] = stageGimmickSO.CameraMoverData;
            stageGimmick = diContainer.Instantiate<CameraMoverGimmick>(param);
          }
          break;

        case StageGimmick.QTEBomb:
          {
            param[0] = stageGimmickSO.QTEBombData;
            stageGimmick = diContainer.Instantiate<QTEBomb>(param);
          }
          break;

        case StageGimmick.InputRequire:
          {
            param[0] = stageGimmickSO.InputRequireData;
            stageGimmick = diContainer.Instantiate<InputRequire>(param);
          }
          break;

        case StageGimmick.Swap:
          {
            param[0] = stageGimmickSO.SwapData;
            stageGimmick = diContainer.Instantiate<Swap>(param);
          }
          break;          
      }
      await UniTask.CompletedTask;
      return stageGimmick;
    }
  }
}
