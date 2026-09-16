using Cysharp.Threading.Tasks;
using LR.Stage.Player.GimmickGuide;
using LR.Stage.StageDataContainer;
using UnityEngine;

namespace LR.Stage.Player
{
  public interface IPlayerPresenter: IStageObjectController
  {
    public UniTask CreateGimmickGuideAsync(StageGimmick stageGimmick);

    public PlayerStatus GetPlayerStatus();

    public IPlayerMoveController GetMoveController();

    public IPlayerInputActionController GetInputActionController();

    public IPlayerInputActionSubscriber GetInputActionSubscriber();

    public IPlayerInputStateProvider GetInputStateProvider();

    public IPlayerReactionController GetReactionController();

    public IPlayerEnergySubscriber GetEnergySubscriber();

    public IPlayerEnergyProvider GetEnergyProvider();

    public IPlayerStateProvider GetStateProvider();

    public IPlayerStateSubscriber GetStateSubscriber();

    public IPlayerAnimatorController GetAnimatorController();

    public Transform GetTransform();

    public InputRequireGuideView GetInputRequireGuideView();

    public QTEGuideView GetQTEGuideView();

    public SwapGuideView GetSwapGuideView();
  }
}