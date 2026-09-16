using Cysharp.Threading.Tasks;
using LR.UI.Indicator;
using System;
using UnityEngine;

namespace LR.Manager.UI
{
  public interface IUIIndicatorService
  {
    public UniTask<IUIIndicatorPresenter> GetNewAsync(Transform root, RectTransform beginTarget);

    public IUIIndicatorPresenter GetTopIndicator();

    public bool TryGetTopIndicator(out IUIIndicatorPresenter current);

    public IDisposable ReleaseTopIndicatorOnDestroy(GameObject target);

    public void ReleaseTopIndicator();

    public bool IsTopIndicatorIsThis(IUIIndicatorPresenter target);
  }
}