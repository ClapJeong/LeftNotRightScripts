using UnityEngine.Events;

namespace LR.Manager.GameDataManager
{
  public interface IRedPillModeService
  {
    public bool IsRedPillEnable { get; }

    public void EnableRedPillMode(bool mode);

    public void SubscribeOnChanged(UnityAction<bool> onChanged);

    public void UnsubscribeOnChanged(UnityAction<bool> onChanged);
  }
}
