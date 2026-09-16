using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LR.UI.Indicator
{
  public partial interface IUIIndicatorPresenter
  {
    public void SetLeftInputGuide(Direction direction);

    public void SetLeftInputGuide(List<Direction> directions);

    public void SetLeftInputGuide(Navigation navigation);

    public void SubscribeLeftInputGuide(UnityAction<List<Direction>> unityAction);

    public void UnsubscribeLeftInputGuide(UnityAction<List<Direction>> unityAction);
  }
}