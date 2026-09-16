using LR.Stage.Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LR.Manager.Stage
{
  public class ExhaustLightController
  {
    private readonly Light2D leftLight;
    private readonly Light2D rightLight;
    private readonly IPlayerMoveController leftMoveController;
    private readonly IPlayerMoveController rightMoveController;

    public ExhaustLightController(Light2D leftLight, Light2D rightLight, IPlayerMoveController leftMoveController, IPlayerMoveController rightMoveController)
    {
      this.leftLight = leftLight;
      this.rightLight = rightLight;
      this.leftMoveController = leftMoveController;
      this.rightMoveController = rightMoveController;
    }

    public void PlayLight()
    {
      leftLight.transform.position = leftMoveController.GetCurrentPosition() + new Vector2(-0.125f, 0.25f);
      rightLight.transform.position = rightMoveController.GetCurrentPosition() + new Vector2(0.125f, -0.25f);
      leftLight.enabled = true;
      rightLight.enabled = true;
    }

    public void StopLight()
    {
      leftLight.enabled = false;
      rightLight.enabled = false;
    }
  }
}
