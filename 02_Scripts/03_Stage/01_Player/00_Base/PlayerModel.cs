using UnityEngine;

namespace LR.Stage.Player
{
  using LR.Manager.Input;
  using LR.Manager.Local.CameraService;
  using LR.Manager.Sound;
  using LR.Manager.Stage;
  using LR.Manager.Stage.Marking;
  using LR.Stage.Player.Enum;
  using LR.Table.Camera;
  using LR.Table.Player;
  using Zenject;

  public class PlayerModel
  {
    [Inject] public DiContainer diContainer;
    [Inject] public AddressableKeySO addressableKeySO;
    [Inject] public IResourceManager resourceManager;
    [Inject] public PlayerEnergyDataSO energyDataSO;
    [Inject] public BothPlayerEnergyContainer energyContainer;
    [Inject] public ColorSO colorSO;

    [Inject] public IStageRecorderService stageRecorderService;
    [Inject] public IEffectService effectService;
    [Inject] public IStageStateHandler stageService;
    [Inject] public IStageResultHandler stageResultHandler;
    [Inject] public IPlayerGetter playerGetter;
    [Inject] public IInputActionSubscriber inputActionSubscriber;
    [Inject] public IInputActionProvider inputActionProvider;    
    [Inject] public IStageStateProvider stageStateProvider;
    [Inject] public IStageEventSubscriber stageEventSubscriber;
    [Inject] public ISFXController sfxController;
    [Inject] public ICameraEffectService cameraEffectService;
    [Inject] public IMarkPlacer markPlacer;
    [Inject] public PlayerModelSO modelSO;

    public ImpulseData impulseData;
    public PlayerType playerType;
    public Vector3 beginPosition;
    public IInputSequenceStopController inputSequenceStopController;

    public PlayerModel(
      PlayerType playerType, 
      Vector3 beginPosition, 
      CameraDataSO cameraDataSO,
      IInputQTEService qteStopper,
      IInputProgressService progressStopper)
    {
      this.playerType = playerType;
      this.beginPosition = beginPosition;
      this.inputSequenceStopController = playerType switch
      {
        PlayerType.Left => qteStopper,
        PlayerType.Right => progressStopper,
        _ => throw new System.NotImplementedException(),
      };
      this.impulseData = cameraDataSO.ImpulseData;
    }

    public Vector3 ParseDirection(Direction direction)
      => direction switch
      {
        Direction.Up => modelSO.Movement.UpVector,
        Direction.Down => modelSO.Movement.DownVector,
        Direction.Left => modelSO.Movement.LeftVector,
        Direction.Right => modelSO.Movement.RightVector,
        _ => throw new System.NotImplementedException(),
      };
  }
}