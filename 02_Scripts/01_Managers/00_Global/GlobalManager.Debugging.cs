using LR.Manager.GameDataManager;

public partial class GlobalManager
{
  private GameDataService gameDataService
    => diContainer.Resolve<GameDataService>();
  public void Debugging_AddClearStage()
    => gameDataService.Debugging_RaiseClearData();

  public void Debugging_MinusClearState()
    => gameDataService.Debugging_LowerClearData();

  public void Debugging_ClearClearStage()
    => gameDataService.Debugging_ClearClearData();

  public void Debugging_MaxClearStage()
    => gameDataService.Debugging_MaxClearData();
}
