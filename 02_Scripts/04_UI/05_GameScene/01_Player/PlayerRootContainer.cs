using UnityEngine;

namespace LR.UI.GameScene.Player
{
  public class PlayerRootContainer: MonoBehaviour
  {
    public UIPlayerRootView leftView;
    public UIPlayerRootView rightView;
    public UIPlayerEnergyView energyView;
    public UIPlayerDamageLogView leftDamageView;
    public UIPlayerDamageLogView rightDamageView;

    [Space(5)]
    public CanvasGroup canvasGroup;
  }
}