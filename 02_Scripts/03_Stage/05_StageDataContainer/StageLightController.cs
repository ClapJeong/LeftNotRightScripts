using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LR.Stage.StageDataContainer
{
  public class StageLightController
  {
    private readonly Light2D doctorLight;

    private readonly List<Light2D> playerLights = new();

    public StageLightController(Transform lightRoot, Light2D doctorLight)
    {
      this.doctorLight = doctorLight;

      foreach(var light2D in lightRoot.GetComponentsInChildren<Light2D>())
      {
        if (light2D == doctorLight)
          continue;
        playerLights.Add(light2D);
      }
    }

    public void EnableDoctorLight(bool isEnable)
      => doctorLight.enabled = isEnable;

    public void EnablePlayerLights(bool isEnable)
    {
      foreach(var light in playerLights)
        light.enabled = isEnable;
    }
  }
}
