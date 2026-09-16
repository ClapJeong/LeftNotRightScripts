using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.GameDataManager;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.Manager.UI;
using LR.Stage.StageDataContainer;
using LR.Table.Dialogue;
using LR.UI.Enum;
using LR.UI.EpilogueScene;
using LR.UI.GameScene.Dialogue;
using LR.UI.Indicator;
using LR.UI.Lobby.DialogueReplayPanel;
using LR.UI.Preloading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.Lobby
{
  public class UIDialogueReplayPanelPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public DiContainer diContainer;
      [Inject] public GlobalManager globalManager;
      [Inject] public TableContainer tableContainer;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public ISFXController sfxController;
      [Inject] public ICanvasProvider canvasProvider;
      [Inject] public IResourceManager resourceManager;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IUIDepthService depthService;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IBGMController bgmController;
      [Inject] public UISO uiSO;
      [Inject] public IGameModeService gameModeService;

      [Inject] public IUIIndicatorPresenter indicator;
      [Inject] public UnityAction onExit;
    }

    private readonly Model model;
    private readonly UIDialogueReplayPanelView view;

    private readonly SubscribeHandle subscribeHandle;

    private readonly List<UIDialogueReplayButtonSet> enableButtons = new();
    private readonly Dictionary<UIDialogueReplayButtonSet, int> buttonRowMap = new();
    private readonly Dictionary<int, TextAsset> dialogueMap = new();

    private readonly Vector2 initializeAnchoredPosition;

    private CancellationTokenSource rootMoveCTS;

    private Vector2 buttonRootMoveAnchoredPosition;
    private UIDialogueReplayButtonSet firstButton;

    private bool isFirstIndicatorMove = true;
    private bool buttonsCreated;

    public UIDialogueReplayPanelPresenter(Model model, UIDialogueReplayPanelView view)
    {
      this.model = model;
      this.view = view;

      initializeAnchoredPosition = view.RectTransform.anchoredPosition;

      view.ExitSubmitDirectionSet.Subscribe(
        model.onExit);

      subscribeHandle = new(
        () =>
        {
          model.selectedGameObjectService.SubscribeEvent(
            IUISelectedGameObjectService.EventType.OnEnter,
            OnSelectedGameObject);

          model.depthService.RaiseDepth(firstButton.gameObject);
        },
        () =>
        {
          model.selectedGameObjectService.UnsubscribeEvent(
            IUISelectedGameObjectService.EventType.OnEnter,
            OnSelectedGameObject);

          model.depthService.LowerDepth();
        });
    }

    public async UniTask ActivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      isFirstIndicatorMove = true;

      if (!buttonsCreated)
      {
        CreateButtons().Forget();
        buttonsCreated = true;
      }
      else
        ShowEnableButtonsAsync().Forget();

      await view.ShowAsync(isImmediately, token);

      buttonRootMoveAnchoredPosition =
        initializeAnchoredPosition -
        Vector2.up * view.GridLayoutGroup.cellSize.y;

      subscribeHandle.Subscribe();
    }

    private async UniTask ShowEnableButtonsAsync()
    {
      foreach (var enableButton in enableButtons)
      {
        await UniTask.WaitForSeconds(model.uiSO.Lobby.DialogueReplayShowInterval);
        enableButton.ShowAsync().Forget();
      }        
    }

    public async UniTask DeactivateAsync(bool isImmediately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();

      await view.HideAsync(isImmediately, token);
      foreach (var enableButton in enableButtons)
        enableButton.HideAsync(true).Forget();


      view.ButtonRoot.anchoredPosition = initializeAnchoredPosition;
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      rootMoveCTS?.Cancel();
      rootMoveCTS?.Dispose();

      subscribeHandle.Dispose();

      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    // =========================
    // Selection
    // =========================

    private void OnSelectedGameObject(GameObject go)
    {
      MoveIndicator(go);
      UpdateInputGuide(go);
      UpdateRootPosition(go);
    }

    private void MoveIndicator(GameObject gameObject)
    {
      if (gameObject.TryGetComponent<RectTransform>(out var rect))
      {
        model.indicator.MoveAsync(gameObject, isFirstIndicatorMove).Forget();
        isFirstIndicatorMove = false;
      }
    }

    private void UpdateInputGuide(GameObject go)
    {
      if (go.TryGetComponent<Selectable>(out var selectable))
        model.indicator.SetLeftInputGuide(selectable.navigation);
    }

    private void UpdateRootPosition(GameObject go)
    {
      foreach(var pair in buttonRowMap)
      {
        if(pair.Key.gameObject == go)
        {
          var selectedRow = pair.Value;

          var rowOffset = -2;
          var targetRow = Mathf.Max(0, selectedRow + rowOffset);

          var targetPos = GetRowAnchoredPosition(targetRow);

          if (buttonRootMoveAnchoredPosition == targetPos)
            return;

          rootMoveCTS?.Cancel();
          rootMoveCTS?.Dispose();
          rootMoveCTS = new();

          MoveRootAsync(targetPos, rootMoveCTS.Token).Forget();
        }
      }
    }

    private Vector2 GetRowAnchoredPosition(int row)
    {
      var grid = view.GridLayoutGroup;

      var offset =
        grid.spacing.y * row +
        grid.cellSize.y * row;

      return initializeAnchoredPosition - Vector2.up * offset;
    }

    private async UniTask MoveRootAsync(Vector2 target, CancellationToken token)
    {
      buttonRootMoveAnchoredPosition = target;

      try
      {
        await view.ButtonRoot
          .DOAnchorPos(target, model.tableContainer.UISO.Indicator.RectTransformMoveDuration)
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException) { }
    }

    // =========================
    // Button Creation
    // =========================

    private async UniTask CreateButtons()
    {
      var topEnableDialogueIndex = await GetTopEnableDialogueIndex();

      if (model.globalManager.EnableAllDialogue)
        topEnableDialogueIndex = int.MaxValue;

      var locations =
        await model.resourceManager.GetLocationsAsync(
          model.tableContainer.AddressableKeySO.Label.Dialogue);

      var count = locations.Count + 2;

      var maxColumn = view.GridLayoutGroup.constraintCount;
      var row = Mathf.CeilToInt((float)count / maxColumn);

      var selectables = new Selectable[row, maxColumn];

      for (int i = 0; i < row; i++)
      {
        var columnCount = GetColumnCount(i, row, count, maxColumn);

        for (int j = 0; j < columnCount; j++)
        {
          var button =
            GameObject.Instantiate(view.ButtonSetPrefab, view.ButtonRoot);
          model.diContainer.InjectGameObject(button.gameObject);

          if (i == 0 && j == 0)
          {
            SetupPrologueButton(button);
          }
          else if (i == row - 1 && j == columnCount - 1)
          {
            SetupEpilogueButton(button);
          }
          else
          {
            var index = i * maxColumn + j - 1;
            SetupDialogueButton(index, button, topEnableDialogueIndex);
          }

          selectables[i, j] = button.Selectable;

          buttonRowMap.Add(button, i);

          if (i == 0 && j == 0)
          {
            firstButton = button;
            view.ExitSelectable.AddNavigation(Direction.Up, button.Selectable);
          }
        }
      }

      SetupNavigation(selectables, row, count, maxColumn);

      await CacheDialogueAssets();
    }

    private void SetupPrologueButton(UIDialogueReplayButtonSet button)
    {
      button.name = "Prologue";
      button.LocalizeStringEvent.SetEntry("ui_Prologue");

      enableButtons.Add(button);

      button.SubmitDirectionSet.Subscribe(
        () =>
        {
          model.indicator.PlayGoodSubmitSFX(false);

          button.SubmitDirectionSet.Enable(false);

          PlayPrologueAsync(() =>
          {
            button.SubmitDirectionSet.Enable(true);
          }).Forget();
        });
    }

    private void SetupEpilogueButton(UIDialogueReplayButtonSet button)
    {      
      button.name = "Epilogue";
      button.LocalizeStringEvent.SetEntry("ui_Epilogue");
      var enableEpilogueButton = model.gameModeService.GetCurrentGameMode() != IGameModeService.GameMode.Demo &&
                                 model.gameDataProvider.GetMaxClearIndex() == model.gameDataProvider.StageDataCount;
      if (model.globalManager.EnableAllDialogue)
        enableEpilogueButton = true;

      if (enableEpilogueButton)
      {
        enableButtons.Add(button);

        button.SubmitDirectionSet.Subscribe(
          () =>
          {
            model.indicator.PlayGoodSubmitSFX(false);

            button.SubmitDirectionSet.Enable(false);

            PlayEpilogueAsync(() =>
            {
              button.SubmitDirectionSet.Enable(true);
            }).Forget();
          });
      }
      else
      {
        button.CanvasGroup.alpha = 0.4f;
        button.DeactivateScroll();
      }
    }

    private void SetupDialogueButton(int index, UIDialogueReplayButtonSet button, int topEnableDialogueIndex)
    {      
      button.name = (index + 1).ToString();

      button.LocalizeStringEvent.SetEntry($"dlg_{index}_name");

      if (index <= topEnableDialogueIndex)
      {
        enableButtons.Add(button);

        button.SubmitDirectionSet.Subscribe(
          () =>
          {
            model.indicator.PlayGoodSubmitSFX(false);

            button.SubmitDirectionSet.Enable(false);

            PlayDialogueAsync(index,
              () => button.SubmitDirectionSet.Enable(true)).Forget();
          });
      }
      else
      {
        button.CanvasGroup.alpha = 0.4f;
        button.DeactivateScroll();
      }
    }

    private int GetColumnCount(int rowIndex, int totalRow, int totalCount, int maxColumn)
    {
      if (rowIndex < totalRow - 1)
        return maxColumn;

      var remain = totalCount % maxColumn;
      return remain == 0 ? maxColumn : remain;
    }

    private void SetupNavigation(
      Selectable[,] selectables,
      int row,
      int count,
      int maxColumn)
    {
      for (int i = 0; i < row; i++)
      {
        var columnCount = GetColumnCount(i, row, count, maxColumn);

        for (int j = 0; j < columnCount; j++)
        {
          var selectable = selectables[i, j];

          if (j > 0)
            selectable.AddNavigation(Direction.Left, selectables[i, j - 1]);

          if (j < columnCount - 1)
            selectable.AddNavigation(Direction.Right, selectables[i, j + 1]);

          if (i > 0)
            selectable.AddNavigation(Direction.Down, selectables[i - 1, j]);
          else
            selectable.AddNavigation(Direction.Down, view.ExitSelectable);

          if (i < row - 1)
            selectable.AddNavigation(Direction.Up, selectables[i + 1, j]);
        }
      }
    }

    // =========================
    // Dialogue
    // =========================

    private async UniTask CacheDialogueAssets()
    {
      var handles =
        await model.resourceManager.LoadAssetsAsync(
          model.tableContainer.AddressableKeySO.Label.Dialogue);

      foreach (var handle in handles)
      {
        var text = handle.Result as TextAsset;
        var index = int.Parse(text.name);

        dialogueMap[index] = text;
      }
    }

    private async UniTask PlayPrologueAsync(UnityAction onComplete)
    {
      EventSystem.current
         .GetComponent<InputSystemUIInputModule>().enabled = false;

      model.depthService.RaiseDepth(null);

      var key =
  model.tableContainer.AddressableKeySO.Path.UI +
  model.tableContainer.AddressableKeySO.UIName.VeryFirstCutscene;
      UIVeryFirstCutscene firstCutscene = null;
      firstCutscene = await model.resourceManager.CreateAssetAsync<UIVeryFirstCutscene>(key, model.canvasProvider.GetCanvas(RootType.Overlay).transform);
      model.diContainer.Inject(firstCutscene);
      firstCutscene.PlayCutscene(onStopped: async () =>
      {
        await firstCutscene.DestroyAsync();

        model.depthService.LowerDepth();

        EventSystem.current
          .GetComponent<InputSystemUIInputModule>().enabled = true;

        onComplete?.Invoke();
        model.bgmController.PlayLobbyBGMAsync().Forget();
      });
      model.bgmController.UpdateVolume(0.0f);
    }

    private async UniTask PlayEpilogueAsync(UnityAction onComplete)
    {
      EventSystem.current
        .GetComponent<InputSystemUIInputModule>().enabled = false;

      model.depthService.RaiseDepth(null);

      IUIPresenter epiloguePresenter = null;
      epiloguePresenter = await UIEpilogueRootPresenter.CreateAsync(
        model.diContainer,
        onFirstFadeComplete: null,
        firstFadeShowDelay: 1.0f,
        onCreditComplete: async () =>
        {
          await epiloguePresenter.DeactivateAsync();

          model.depthService.LowerDepth();

          EventSystem.current
            .GetComponent<InputSystemUIInputModule>().enabled = true;
          onComplete?.Invoke();
        });
      epiloguePresenter.ActivateAsync().Forget();
    }

    private async UniTask PlayDialogueAsync(int index, UnityAction onComplete)
    {
      EventSystem.current
        .GetComponent<InputSystemUIInputModule>().enabled = false;

      model.depthService.RaiseDepth(null);

      var data = dialogueMap[index];

      var dialogueData =
        JsonUtility.FromJson<DialogueData>(data.text);

      IUIPresenter dialoguePresenter = null;
      dialoguePresenter =
        await CreateDialogueUIAsync(dialogueData,
          onDialogueComplete: () =>
          {
            OnDialogueCompleteAsync(dialoguePresenter, onComplete).Forget();
          },
          index);

      await dialoguePresenter.ActivateAsync();
    }

    private async UniTask OnDialogueCompleteAsync(IUIPresenter dialoguePresenter, UnityAction onComplete)
    {
      await dialoguePresenter.DeactivateAsync();
      await UniTask.WaitForSeconds(0.25f);      

      model.depthService.LowerDepth();

      EventSystem.current
        .GetComponent<InputSystemUIInputModule>().enabled = true;

      onComplete?.Invoke();
    }

    private async UniTask<IUIPresenter> CreateDialogueUIAsync(
      DialogueData dialogueData,
      UnityAction onDialogueComplete,
      int index)
    {
      var presenter =
        await UIDialogueRootPresenter.CreateDialogueUIAsync(
          this.model.diContainer,
          dialogueData,
          onDialogueComplete,
          index,
          true,
          false,
          view.gameObject);

      return presenter;
    }

    // =========================
    // StageData
    // =========================

    private async UniTask<int> GetTopEnableDialogueIndex()
    {
      var stageIndex = model.gameDataProvider.GetMaxClearIndex();

      while (stageIndex > 0)
      {
        var key =
          model.tableContainer.AddressableKeySO.Path.Stage +
          string.Format(model.tableContainer.AddressableKeySO.StageName.StageNameFormat, stageIndex);

        try
        {
          var handle =
            await model.resourceManager.LoadAssetsAsync(key);

          var stageData =
            (handle.First().Result as GameObject)
            .GetComponent<StageDataContainer>();

          if (stageData.afterDialogueIndex > -1)
          {
            model.resourceManager.ReleaseAsset(key);
            return stageData.afterDialogueIndex;
          }

          if (stageData.beforeDialogueIndex > -1)
          {
            model.resourceManager.ReleaseAsset(key);
            return stageData.beforeDialogueIndex;
          }
        }
        catch { }

        stageIndex--;
      }

      return 0;
    }
  }
}
