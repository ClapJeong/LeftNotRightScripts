using System.Collections.Generic;

namespace LR.Manager.Store
{
  public interface IAchievementRegister
  {
    public void ResetAll();

    public void SetData(string key, int value);

    public void SetAchievement(string key);

    public void UpdateDemoAchievementData(List<int> clearChapters, int normalPerfect, int hardPerfect);

    public void UpdateHardModeAchievement(int hardClearCount);
  }
}
