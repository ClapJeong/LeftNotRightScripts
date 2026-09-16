using System.Collections.Generic;
using UnityEngine.Events;

namespace LR.UI.Indicator
{
  public partial interface IUIIndicatorPresenter
  {
    //public void EmptyRightInputGuide();

    //public void SetRightInputGuide(Direction direction);    

    //public void InvokeRightInputEvent(params Direction[] directions);

    //public void SetRightInputGuide(IUISubmitView submitView);

    //public void SubscribeRightInputGuide(UnityAction<List<Direction>> unityAction);

    //public void UnsubscribeRightInputGuide(UnityAction<List<Direction>> unityAction);

    public void PlayGoodSubmitSFX(bool ignoreNextMoveSFX = true);

    public void PlayBadSubmitSFX(bool ignoreNextMoveSFX = true);
  }
}