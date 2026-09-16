using Cysharp.Threading.Tasks;
using LR.UI.Indicator;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using LR.UI.Enum;
using LR.Manager.UI;
using LR.UI.LocaleSet;
using Zenject;

namespace LR.UI.Lobby
{
  public class UILocalizePanelPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public LocaleService localeService;
      [Inject] public IUIIndicatorPresenter indicator;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IUIDepthService depthService;
      [Inject] public UnityAction onExit;
    }

    private readonly Model model;
    private readonly UILocalizePanelView view;

    private readonly SubscribeHandle subscribeHandle;
    private const float DisableAlpha = 0.6f;
    private UILocaleButtonsView.ButtonSet selectedButtonSet;
    private bool isFirstIndiactorMove = true;

    public UILocalizePanelPresenter(Model model, UILocalizePanelView view)
    {
      this.model = model;
      this.view = view;

      foreach(var buttonSet in view.LocaleButtonsView.ButtonSets)
        SubscribeLocaleButtonSet(buttonSet);

      view.ExitSubmitDirectionSet.Subscribe(model.onExit);        

      subscribeHandle = new(
        () =>
        {
          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
          model.depthService.RaiseDepth(selectedButtonSet.SubmitDirectionSet.gameObject);
        },
        () =>
        {
          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
          model.depthService.LowerDepth();          
        });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      isFirstIndiactorMove = true;
      foreach (var buttonSet in view.LocaleButtonsView.ButtonSets)
      {
        var targetAlpha = LocalizationSettings.SelectedLocale == buttonSet.Locale ? 1.0f : DisableAlpha;
        buttonSet.SubmitDirectionSet.CanvasGroup.alpha = targetAlpha;

        var isSelectedLocale = LocalizationSettings.SelectedLocale == buttonSet.Locale;
        buttonSet.SubmitDirectionSet.Enable(!isSelectedLocale);
        if(isSelectedLocale)
          selectedButtonSet = buttonSet;
      }            
      await view.ShowAsync(isImmedieately, token);
      subscribeHandle.Subscribe();
      model.depthService.SelectTopObject();      
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void SubscribeLocaleButtonSet(UILocaleButtonsView.ButtonSet buttonSet)
    {
      buttonSet.SubmitDirectionSet.Subscribe(
        () =>
        {
          model.indicator.PlayGoodSubmitSFX(ignoreNextMoveSFX: false);

          buttonSet.SubmitDirectionSet.CanvasGroup.alpha = 1.0f;
          buttonSet.SubmitDirectionSet.OnExit();
          model.localeService.SetLocale(buttonSet.Locale);
          model.localeService.SaveLocale();

          selectedButtonSet.SubmitDirectionSet.Enable(true);
          selectedButtonSet.SubmitDirectionSet.CanvasGroup.alpha = DisableAlpha;

          selectedButtonSet = buttonSet;
          selectedButtonSet.SubmitDirectionSet.Enable(false);
        });
    }

    private void OnSelectedGameObjectEnter(GameObject gameObject)
    {
      model.indicator.MoveAsync(gameObject, isFirstIndiactorMove);

      if (gameObject.TryGetComponent<Selectable>(out var selectable))
        model.indicator.SetLeftInputGuide(selectable.navigation);

      if (gameObject != view.ExitRectTransform.gameObject)
      {
        foreach (var buttonSet in view.LocaleButtonsView.ButtonSets)
          if (buttonSet.SubmitDirectionSet.RectTransform.gameObject == gameObject && LocalizationSettings.SelectedLocale == buttonSet.Locale)
          {
            view.ExitSelectable.AddNavigation(Direction.Up, buttonSet.Selectable);
          }
      }
    }
  }
}
