using UnityEngine;
using LR.Stage.Player.Enum;
using LR.Stage.StageDataContainer;

[System.Serializable]
public class GameObjectName
{
  [field: SerializeField] public string LeftPlayer { get; private set; }
  [field: SerializeField] public string RightPlayer { get; private set; }

  public string GetPlayerName(PlayerType playerType)
    => playerType switch
    {
      PlayerType.Left => LeftPlayer,
      PlayerType.Right => RightPlayer,
      _ => throw new System.NotImplementedException()
    };

  [field: Header("[ Gimmick ]")]
  [field: SerializeField] public string ACSignalPreview {  get; private set; }
  [field: SerializeField] public string ACDCSignalPreview { get; private set; }
  [field: SerializeField] public string InputRequireGuideView {  get; private set; }
  [field: SerializeField] public string QTEGuideView { get; private set; }
  [field: SerializeField] public string SwapGuideView { get; private set; }

  public bool TryGetGimmickGuideViewKey(StageGimmick gimmick, out string key)
  {
    key = gimmick switch
    {
      StageGimmick.QTEBomb => QTEGuideView,
      StageGimmick.InputRequire => InputRequireGuideView,
      StageGimmick.Swap => SwapGuideView,
      _ => string.Empty,
    };

    return !string.IsNullOrEmpty(key);
  }
  [field: Header("[ Paint ]")]
  [field: SerializeField] public string LeftWallHitPaint {  get; private set; }
  [field: SerializeField] public string RightWallHitPaint { get; private set; }
  [field: SerializeField] public string LeftDeadPaint { get; private set; }
  [field: SerializeField] public string RightDeadPaint { get; private set; }

  [field: Header("[ Else ]")]
  [field: SerializeField] public string WallHitFallingObject { get; private set; }
}