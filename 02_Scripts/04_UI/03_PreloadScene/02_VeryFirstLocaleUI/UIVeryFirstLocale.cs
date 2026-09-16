using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.UI.Indicator;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using LR.Manager.UI;
using LR.Manager.Input;
using Zenject;
using LR.UI.LocaleSet;
using LR.UI.Input;
using LR.UI.DifficultySettting;
using LR.Manager.Device;
using LR.Stage.Player.Enum;
using System.Linq;
using LR.Manager.GameDataManager;
using LR.UI.VolumeControl;

namespace LR.UI.Preloading
{
  public class UIVeryFirstLocale : MonoBehaviour
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public IDeviceEvnetSubscriber deviceEvnetSubscriber;
      [Inject] public LocaleService localeService;
      [Inject] public IUIIndicatorService indicatorService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IUIDepthService depthService;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public IResourceManager resourceManager;
      [Inject] public ColorSO colorSO;
      [Inject] public UISO uiSO;
      [Inject] public IDeviceProvider deviceProvider;
      [Inject] public AddressableKeySO addressableKeySO;
      [Inject] public IDifficultyService difficultyService;
      [Inject] public UnityAction onConfirm;
    }

    private const float DisableAlpha = 0.4f;

    [SerializeField] private UIInputView leftInputView;
    [SerializeField] private UIInputView rightInputView;
    [Space(5)]
    [SerializeField] private UISubmitDirectionSet startDirectionSet;
    [SerializeField] private Selectable startSelectable;
    [SerializeField] private UIDifficultyView difficultyView;        
    [SerializeField] private UILocaleButtonsView localeButtonView;
    [SerializeField] private UIVolumeControlView volumeControlView;
    [SerializeField] private UIRedPillButtonView redPillButtonView;
    [Space(10)]
    [SerializeField] private Transform indicatorRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    private Model model;

    private UIDifficultyPresenter difficultyPresenter;
    private UIVolumeControlPresenter volumeControlPresenter;
    private UIRedPillButtonPresenter redPillButtonPresenter;
    private UILocaleButtonsView.ButtonSet selectedLocaleButtonSet;
    private IUIIndicatorPresenter indicator;

    public async UniTask InitializeAsync(Model model)
    {
      canvasGroup.alpha = 0.0f;
      this.model = model;

      model.diContainer.Inject(rightInputView);

      var volumeControlModel = model.diContainer.Instantiate<UIVolumeControlPresenter.Model>();
      volumeControlPresenter = new(volumeControlModel, volumeControlView);
      volumeControlPresenter.ActivateAsync().Forget();
      volumeControlPresenter.AttachOnDestroy(gameObject);

      var redPillModel = model.diContainer.Instantiate<UIRedPillButtonPresenter.Model>();
      redPillButtonPresenter = new(redPillModel, redPillButtonView);
      redPillButtonPresenter.AttachOnDestroy(gameObject);
      redPillButtonPresenter.ActivateAsync(true).Forget();

      await InputAtlasProvider.GetInputAtlasesAsync(
        model.addressableKeySO, 
        model.resourceManager,
        (deviceType, atlas) =>
        {
          leftInputView.AddAtlas(PlayerType.Left, deviceType, atlas);
          rightInputView.AddAtlas(PlayerType.Right, deviceType, atlas);
        });
      await LocaleAutoSetter.SetLocaleBySystemLanguageAsync();

      var difficultyModel = model.diContainer.Instantiate<UIDifficultyPresenter.Model>(new object[]
      {
        (UnityAction<UIDifficultyView.DifficultyButtonSet>)OnDifficultyChanged
      });
      difficultyPresenter = new(difficultyModel, difficultyView);
      difficultyPresenter.ActivateAsync(true).Forget();
      difficultyPresenter.AttachOnDestroy(gameObject);
      var currentLocaleSelectable = localeButtonView.ButtonSets.FirstOrDefault(set => set.Locale == LocalizationSettings.SelectedLocale).Selectable;
      foreach(var diffSet in difficultyView.DifficultyButtonSets)
        diffSet.Selectable.AddNavigation(Direction.Down, currentLocaleSelectable);

      model.difficultyService.SetDifficulty(IDifficultyService.Difficulty.Normal);
      var currentDifficultySelectable = difficultyView.DifficultyButtonSets.FirstOrDefault(set => set.Difficulty == model.difficultyService.CurrentDifficulty).Selectable;
      foreach (var localeSet in localeButtonView.ButtonSets)
        localeSet.Selectable.AddNavigation(Direction.Up, currentDifficultySelectable);
      startSelectable.AddNavigation(Direction.Down, currentDifficultySelectable);

      model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
    
      model.deviceEvnetSubscriber.SubscribeDeviceEvent(leftInputView.ToggleDeviceInputSet);
      model.deviceEvnetSubscriber.SubscribeDeviceEvent(rightInputView.ToggleDeviceInputSet);
      leftInputView.ApplyColor(model.colorSO, Stage.Player.Enum.PlayerType.Left);
      rightInputView.ApplyColor(model.colorSO, Stage.Player.Enum.PlayerType.Right);

      model.inputActionSubscriber.Subscribe(LRInputType.LeftUp, OnLeftUp);
      model.inputActionSubscriber.Subscribe(LRInputType.LeftRight, OnLeftRight);
      model.inputActionSubscriber.Subscribe(LRInputType.LeftDown, OnLeftDown);
      model.inputActionSubscriber.Subscribe(LRInputType.LeftLeft, OnLeftLeft);
      model.inputActionSubscriber.Subscribe(LRInputType.RightUp, OnRightUp);
      model.inputActionSubscriber.Subscribe(LRInputType.RightRight, OnRightRight);
      model.inputActionSubscriber.Subscribe(LRInputType.RightDown, OnRightDown);
      model.inputActionSubscriber.Subscribe(LRInputType.RightLeft, OnRightLeft);

      foreach (var buttonSet in localeButtonView.ButtonSets)
      {
        buttonSet.SubmitDirectionSet.CanvasGroup.alpha = LocalizationSettings.SelectedLocale == buttonSet.Locale ? 1.0f : DisableAlpha;

        buttonSet.SubmitDirectionSet.Subscribe(
                  onPerformed: () =>
                  {
                    OnLocaleChanged(buttonSet);
                  });

        if (LocalizationSettings.SelectedLocale == buttonSet.Locale)
        {
          selectedLocaleButtonSet = buttonSet;
          selectedLocaleButtonSet.SubmitDirectionSet.Enable(false);
          indicator = await model.indicatorService.GetNewAsync(indicatorRoot, buttonSet.SubmitDirectionSet.RectTransform);
          model.depthService.RaiseDepth(buttonSet.SubmitDirectionSet.RectTransform.gameObject);

          volumeControlView
          .MasterVolumeSet
          .Selectable
          .AddNavigation(Direction.Up, buttonSet.Selectable);
        }
      }

      startDirectionSet.Subscribe(model.onConfirm);

      canvasGroup.alpha = 1.0f;
    }

    private void OnLocaleChanged(UILocaleButtonsView.ButtonSet buttonSet)
    {
      indicator.PlayGoodSubmitSFX(ignoreNextMoveSFX: false);
      buttonSet.SubmitDirectionSet.CanvasGroup.alpha = 1.0f;
      buttonSet.SubmitDirectionSet.OnExit();
      model.localeService.SetLocale(buttonSet.Locale);
      model.localeService.SaveLocale();

      selectedLocaleButtonSet.SubmitDirectionSet.Enable(true);
      selectedLocaleButtonSet.SubmitDirectionSet.CanvasGroup.alpha = DisableAlpha;
      selectedLocaleButtonSet = buttonSet;
      selectedLocaleButtonSet.SubmitDirectionSet.Enable(false);

      foreach (var diffSet in difficultyView.DifficultyButtonSets)
        diffSet.Selectable.AddNavigation(Direction.Down, buttonSet.Selectable);

      volumeControlView
        .MasterVolumeSet
        .Selectable
        .AddNavigation(Direction.Up, buttonSet.Selectable);
    }

    public async UniTask DestroyAsync()
    {
      difficultyPresenter.DeactivateAsync(true).Forget();
      model.inputActionSubscriber.Unsubscribe(LRInputType.LeftUp, OnLeftUp);
      model.inputActionSubscriber.Unsubscribe(LRInputType.LeftRight, OnLeftRight);
      model.inputActionSubscriber.Unsubscribe(LRInputType.LeftDown, OnLeftDown);
      model.inputActionSubscriber.Unsubscribe(LRInputType.LeftLeft, OnLeftLeft);
      model.inputActionSubscriber.Unsubscribe(LRInputType.RightUp, OnRightUp);
      model.inputActionSubscriber.Unsubscribe(LRInputType.RightRight, OnRightRight);
      model.inputActionSubscriber.Unsubscribe(LRInputType.RightDown, OnRightDown);
      model.inputActionSubscriber.Unsubscribe(LRInputType.RightLeft, OnRightLeft);

      model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(leftInputView.ToggleDeviceInputSet);
      model.deviceEvnetSubscriber.UnsubscribeDeviceEvent(rightInputView.ToggleDeviceInputSet);
      startDirectionSet.Unsubscribe(model.onConfirm);
      model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObjectEnter);
      model.indicatorService.ReleaseTopIndicator();      

      await DOTween
        .Sequence()
        .Append(canvasGroup.DOFade(0.0f, 1.0f))
        .AppendInterval(0.5f)
        .OnComplete(() =>
        {
          model.resourceManager.ReleaseInstance(gameObject);
        })
        .ToUniTask(TweenCancelBehaviour.Complete);
    }

    private void OnSelectedGameObjectEnter(GameObject gameObject)
    {
      indicator.MoveAsync(gameObject);

      if (gameObject.TryGetComponent<Selectable>(out var selectable))
        indicator.SetLeftInputGuide(selectable.navigation);
    }

    private void OnDifficultyChanged(UIDifficultyView.DifficultyButtonSet set)
    {
      startSelectable.AddNavigation(Direction.Down, set.Selectable);
      foreach (var localeSet in localeButtonView.ButtonSets)
        localeSet.Selectable.AddNavigation(Direction.Up, set.Selectable);
    }

    private void OnLeftUp(InputPhase inputPhase)
    {
      var direction = Direction.Up;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnLeftPerformed(direction); break;
        case InputPhase.Canceled: OnLeftCanceled(direction); break;
      }
    }

    private void OnLeftRight(InputPhase inputPhase)
    {
      var direction = Direction.Right;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnLeftPerformed(direction); break;
        case InputPhase.Canceled: OnLeftCanceled(direction); break;
      }
    }

    private void OnLeftDown(InputPhase inputPhase)
    {
      var direction = Direction.Down;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnLeftPerformed(direction); break;
        case InputPhase.Canceled: OnLeftCanceled(direction); break;
      }
    }

    private void OnLeftLeft(InputPhase inputPhase)
    {
      var direction = Direction.Left;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnLeftPerformed(direction); break;
        case InputPhase.Canceled: OnLeftCanceled(direction); break;
      }
    }

    private void OnRightUp(InputPhase inputPhase)
    {
      var direction = Direction.Up;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnRightPerformed(direction); break;
        case InputPhase.Canceled: OnRightCanceled(direction); break;
      }
    }

    private void OnRightRight(InputPhase inputPhase)
    {
      var direction = Direction.Right;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnRightPerformed(direction); break;
        case InputPhase.Canceled: OnRightCanceled(direction); break;
      }
    }

    private void OnRightDown(InputPhase inputPhase)
    {
      var direction = Direction.Down;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnRightPerformed(direction); break;
        case InputPhase.Canceled: OnRightCanceled(direction); break;
      }
    }

    private void OnRightLeft(InputPhase inputPhase)
    {
      var direction = Direction.Left;
      switch (inputPhase)
      {
        case InputPhase.Performed: OnRightPerformed(direction); break;
        case InputPhase.Canceled: OnRightCanceled(direction); break;
      }
    }

    private void OnLeftPerformed(Direction direction)
      => UpdateIcon(leftInputView, PlayerType.Left, direction, true);

    private void OnLeftCanceled(Direction direction)
      => UpdateIcon(leftInputView, PlayerType.Left, direction, false);

    private void OnRightPerformed(Direction direction)
      => UpdateIcon(rightInputView, PlayerType.Right, direction, true);

    private void OnRightCanceled(Direction direction)
      => UpdateIcon(rightInputView, PlayerType.Right, direction, false);

    private void UpdateIcon(UIInputView inputView, PlayerType playerType, Direction direction, bool isInput)
    {
      var targetScale = Vector3.one * (isInput ? model.uiSO.Player.InputIconScale : 1.0f);
      inputView.GetImageRectTransform(model.deviceProvider.CurrentDeviceType, direction).localScale = targetScale;
      inputView.UpdateIcon(playerType, direction, isInput);
    }

  }
}
