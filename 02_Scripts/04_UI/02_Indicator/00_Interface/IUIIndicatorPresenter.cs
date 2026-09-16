using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LR.UI.Indicator
{
  public partial interface IUIIndicatorPresenter: IUIPresenter
  {
    public void ReInitialize(Transform root, RectTransform rectTransform);

    public UniTask MoveAsync(GameObject gameObject, bool isImmediately = false);
    public UniTask MoveAsync(RectTransform rectTransform, bool isImmediately = false);
    public UniTask MoveAsync(BaseSubmitView submitView, bool isImmediately = false);
  }
}