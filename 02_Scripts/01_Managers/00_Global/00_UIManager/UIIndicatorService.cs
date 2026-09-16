using Cysharp.Threading.Tasks;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.UI.Indicator;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace LR.Manager.UI
{
  public class UIIndicatorService : IUIIndicatorService
  {
    [Inject(Id = "IndicatorDisableRoot")] private readonly Transform disableRoot = null;
    [Inject] private readonly IResourceManager resourceManager = null;
    [Inject] private readonly DiContainer diContainer = null;

    private readonly Stack<IUIIndicatorPresenter> enableIndicators = new();
    private readonly Stack<IUIIndicatorPresenter> disabledIndicators = new();    

    private readonly string indicatorKey;    

    public UIIndicatorService(AddressableKeySO addressableKeySO)
    {
      indicatorKey = addressableKeySO.Path.UI + addressableKeySO.UIName.Indicator;
    }

    public IDisposable ReleaseTopIndicatorOnDestroy(GameObject target)
      => target
              .OnDestroyAsObservable()
              .Subscribe(_ =>
              {
                ReleaseTopIndicator();
              });

    public IUIIndicatorPresenter GetTopIndicator()
      => enableIndicators.Peek();

    public bool TryGetTopIndicator(out IUIIndicatorPresenter current)
      => enableIndicators.TryPeek(out current) && current != null;


    public async UniTask<IUIIndicatorPresenter> GetNewAsync(Transform root, RectTransform beginTarget)
    {
      if (disabledIndicators.TryPop(out var topIndicator))
      {
        enableIndicators.Push(topIndicator);
        topIndicator.ReInitialize(root, beginTarget);
        await topIndicator.ActivateAsync();
        return topIndicator;
      }
      else
      {
        var newIndicator = await CreateAsync(root, beginTarget);
        enableIndicators.Push(newIndicator);
        return newIndicator;
      }
    }

    public void ReleaseTopIndicator()
    {
      if(enableIndicators.TryPop(out var topIndicator))
      {
        topIndicator.DeactivateAsync();
        disabledIndicators.Push(topIndicator);
      }      
    }

    public bool IsTopIndicatorIsThis(IUIIndicatorPresenter target)
      => TryGetTopIndicator(out var topIndicator) && topIndicator == target;

    private async UniTask<IUIIndicatorPresenter> CreateAsync(Transform root, RectTransform beginTarget)
    {
      var model = diContainer.Instantiate<BaseUIIndicatorPresenter.Model>(new object[]
      {
        root,
        beginTarget,
        disableRoot,
      });
      var view = await resourceManager.CreateAssetAsync<BaseUIIndicatorView>(indicatorKey, root);
      var presenter = new BaseUIIndicatorPresenter(model, view);
      view.name = $"Indicator: {presenter.GetHashCode()}";
      await presenter.ActivateAsync();
      return presenter;
    }
  }
}