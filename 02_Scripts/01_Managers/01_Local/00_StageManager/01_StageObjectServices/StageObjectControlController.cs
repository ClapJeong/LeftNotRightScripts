using LR.Manager.Stage.StageObject;
using System.Collections.Generic;

namespace LR.Manager.Stage
{
  public class StageObjectControlController : IStageObjectControlService
  {
    private readonly List<IStageObjectControlService> services = new();

    public StageObjectControlController(params IStageObjectControlService[] services)
    {
      foreach (var service in services)
        this.services.Add(service);
    }

    public void EnableAll(bool isEnable)
    {
      foreach(var service in services)
        service.EnableAll(isEnable);
    }

    public void RestartAll()
    {
      foreach (var service in services)
        service.RestartAll();
    }
  }
}
