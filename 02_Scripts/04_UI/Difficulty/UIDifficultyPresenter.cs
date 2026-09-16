using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.DifficultySettting
{
  public class UIDifficultyPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public IDifficultyService difficultyService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public UnityAction<UIDifficultyView.DifficultyButtonSet> onDifficultyChanged;
      [Inject] public ISFXController sfxController;
    }

    private readonly Model model;
    private readonly UIDifficultyView view;

    private readonly float EnableAlpha = 0.3f;
    private readonly float DisableAlpha = 1.0f;

    private readonly SubscribeHandle subscribeHandle;
    private UIDifficultyView.DifficultyButtonSet selectedButtonSet = null;

    public UIDifficultyPresenter(Model model, UIDifficultyView view)
    {
      this.model = model;
      this.view = view;
      
      foreach (var set in view.DifficultyButtonSets)
      {
        var isCurrentDifficulty = set.Difficulty == model.difficultyService.CurrentDifficulty;
        set.SubmitDirectionSet.Enable(!isCurrentDifficulty);
        set.SubmitDirectionSet.CanvasGroup.alpha = isCurrentDifficulty ? DisableAlpha : EnableAlpha;
        set.Description.SetActive(false);

        if(isCurrentDifficulty)
          selectedButtonSet = set;

        if (set.Difficulty == IDifficultyService.Difficulty.Hard)
          set.DescriptionRectTransform.DOShakeAnchorPos(0.2f, 3.0f, 20, 180.0f).SetLoops(-1).Play();

        set.SubmitDirectionSet.Subscribe(
          () =>
          {
            if (selectedButtonSet != null)
            {
              selectedButtonSet.SubmitDirectionSet.Enable(true);
              selectedButtonSet.SubmitDirectionSet.CanvasGroup.alpha = EnableAlpha;
            }

            var audioSourceType = set.Difficulty switch
            {
              IDifficultyService.Difficulty.Easy => AudioSourceType.Left,
              IDifficultyService.Difficulty.Normal => AudioSourceType.Center,
              IDifficultyService.Difficulty.Hard => AudioSourceType.Right,
              _ => throw new NotImplementedException(),
            };

            model.sfxController.PlayOnce(audioSourceType, SFX.UIGoodSubmit);
            
            model.difficultyService.SetDifficulty(set.Difficulty);
            set.SubmitDirectionSet.Enable(false);
            set.SubmitDirectionSet.CanvasGroup.alpha = DisableAlpha;
            selectedButtonSet = set;

            model.onDifficultyChanged?.Invoke(set);
          });        
      }

      subscribeHandle = new(
        () =>
        {
          model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObject);
        },
        () =>
        {
          model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectedGameObject);
        });
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Subscribe();
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    public void Dispose()
    {
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    private void OnSelectedGameObject(GameObject gameObject)
    {
      foreach(var set in view.DifficultyButtonSets)
      {
        var enable = set.SubmitDirectionSet.gameObject == gameObject;
        set.Description.SetActive(enable);
        if (enable)
        {
          LayoutRebuilder.ForceRebuildLayoutImmediate(set.Description.GetComponent<RectTransform>());
        }          
      }        
    }
  }
}