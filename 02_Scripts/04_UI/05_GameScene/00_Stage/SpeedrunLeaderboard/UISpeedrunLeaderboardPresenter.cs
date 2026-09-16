using Cysharp.Threading.Tasks;
using DG.Tweening;
using LR.Manager.Input;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace LR.UI
{
  public class UISpeedrunLeaderboardPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public ILeaderBoardService leaderBoardService;
      [Inject] public UISO uiSO;
      [Inject] public ColorSO colorSO;
      [Inject] public IUISelectedGameObjectService selectedGameObjectService;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public string key;
      [Inject] public bool IsFastest;
    }

    private readonly Model model;
    private readonly UISpeedrunLeaderboardView view;

    private readonly int range = 2;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer doctorMoveCTS = new();
    private readonly CTSContainer newPumpCTS = new();

    private bool isSelected = false;
    private bool isDeafultSet = true;
    private bool isScrolling = false;
    private readonly Queue<Direction> inputDirectionQueue = new();
    private readonly List<ILeaderBoardService.RankData> cachedRankDatas = new();
    private readonly Dictionary<int, ILeaderBoardService.RankData[]> pageIndexRankDatas = new();
    private int rankCount;
    private int pageIndex = 0;
    private int myRank;
    private readonly int PageUnit = 5;


    public UISpeedrunLeaderboardPresenter(Model model, UISpeedrunLeaderboardView view)
    {
      this.model = model;
      this.view = view;

      DeactivateAsync(true).Forget();

      subscribeHandle = new(() =>
      {
        model.leaderBoardService.SubscribeOnDelaySubmitComplete(OnSubscribeDelaySubmitComplete);
        model.selectedGameObjectService.SubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectGameObject);
        model.inputActionSubscriber.SubscribePhase(LRInputType.LeftUp, OnLeftUp, InputPhase.Performed);
        model.inputActionSubscriber.SubscribePhase(LRInputType.LeftDown, OnLeftDown, InputPhase.Performed);
      },
      () =>
      {
        model.leaderBoardService.UnsubscribeOnDelaySubmitComplete(OnSubscribeDelaySubmitComplete);
        model.selectedGameObjectService.UnsubscribeEvent(IUISelectedGameObjectService.EventType.OnEnter, OnSelectGameObject);
        model.inputActionSubscriber.UnsubscribePhase(LRInputType.LeftUp, OnLeftUp, InputPhase.Performed);
        model.inputActionSubscriber.UnsubscribePhase(LRInputType.LeftDown, OnLeftDown, InputPhase.Performed);
      });
      var token = doctorMoveCTS.token;
      foreach (var doctorRectTransform in view.DoctorRectTransforms)
        MovedoctorAsync(doctorRectTransform, token).Forget();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      view.GetLeaderboardSet(!isDeafultSet).RootRectTransform.anchoredPosition = new Vector2(0.0f, -1000.0f);
      newPumpCTS.Cancel();

      UpdateLoadingView();

      await CacheLeaderboardAsync();
      ShowGlobalRecordFirstAsync().Forget();
      subscribeHandle.Subscribe();
      await view.ShowAsync(isImmedieately, token);
    }

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle?.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
      UpdateLoadingView();
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      newPumpCTS.Dispose();
      doctorMoveCTS.Dispose();
      subscribeHandle?.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private void OnSelectGameObject(GameObject gameObject)
    {
      isSelected = gameObject == view.gameObject;
    }

    private void OnLeftUp()
    {
      if (!isSelected)
        return;

      var direction = Direction.Up;
      OnEnqueueDirection(direction);
    }

    private void OnLeftDown()
    {
      if (!isSelected)
        return;

      var direction = Direction.Down;
      OnEnqueueDirection(direction);
    }

    private void OnEnqueueDirection(Direction direction)
    {
      if (!IsPageIndexMovable(direction))
        return;

      if (inputDirectionQueue.Count == 0 && !isScrolling)
      {
        FlipLeaderboardSets(direction);
        MoveLeaderboardSetAsync(direction, view.destroyCancellationToken).Forget();
      }
      else
      {
        inputDirectionQueue.Enqueue(direction);
      }
    }

    private bool IsPageIndexMovable(Direction direction)
    {
      var sign = direction switch
      {
        Direction.Up => 1.0f,
        Direction.Right => throw new NotImplementedException(),
        Direction.Down => -1.0f,
        Direction.Left => throw new NotImplementedException(),
        _ => throw new NotImplementedException(),
      };

      var modify = -(pageIndex + sign) * PageUnit;
      var min = myRank - 2 + modify;
      var max = myRank + 2 + modify;
      var isLess = min < 1 && max < 1;
      var isOver = min > rankCount && max > rankCount;
      return !isLess && !isOver;
    }

    private void FlipLeaderboardSets(Direction direction)
    {
      var prevSet = view.GetLeaderboardSet(!isDeafultSet);
      var height = view.RectTransform.rect.height;
      var sign = direction switch
      {
        Direction.Up => 1.0f,
        Direction.Right => throw new NotImplementedException(),
        Direction.Down => -1.0f,
        Direction.Left => throw new NotImplementedException(),
        _ => throw new NotImplementedException(),
      };
      prevSet.RootRectTransform.anchoredPosition = new Vector2(0.0f, sign * height);
      isDeafultSet = !isDeafultSet;

      pageIndex += (int)sign;
    }

    private async UniTask MoveLeaderboardSetAsync(Direction direction, CancellationToken token)
    {
      try
      {
        view.GetLeaderboardSet(isDeafultSet).LeaderboardRoot.SetActive(false);
        view.GetLeaderboardSet(isDeafultSet).LoadingRoot.gameObject.SetActive(true);
        UpdatePageIndexRanksAsync(token).Forget();

        isScrolling = true;
        var targetRectTransform = view.GetLeaderboardSet(isDeafultSet).RootRectTransform;
        var targetSetEndPos = Vector2.zero;
        var prevRectTransform = view.GetLeaderboardSet(!isDeafultSet).RootRectTransform;
        var height = view.RectTransform.rect.height;
        var sign = direction switch
        {
          Direction.Up => -1.0f,
          Direction.Right => throw new NotImplementedException(),
          Direction.Down => 1.0f,
          Direction.Left => throw new NotImplementedException(),
          _ => throw new NotImplementedException(),
        };
        var prevEndPos = new Vector2(0.0f, sign * height);
        var duration = model.uiSO.Stage.LeaderboardScrollDuration;
        await DOTween
          .Sequence()
          .Join(targetRectTransform.DOAnchorPos(targetSetEndPos, duration))
          .Join(prevRectTransform.DOAnchorPos(prevEndPos, duration))
          .ToUniTask(TweenCancelBehaviour.Complete, token);

        if (inputDirectionQueue.TryDequeue(out var nextDirection))
        {
          FlipLeaderboardSets(nextDirection);
          MoveLeaderboardSetAsync(nextDirection, token).Forget();
        }
        else
        {
          isScrolling = false;
        }
      }
      catch (OperationCanceledException)
      {

      }
    }

    private async UniTask UpdatePageIndexRanksAsync(CancellationToken token)
    {
      if (pageIndexRankDatas.TryGetValue(pageIndex, out var datas))
        await UpdateLeaderboardViewAsync(datas).AttachExternalCancellation(token);
      else
      {
        var modify = -pageIndex * PageUnit;
        var min = Mathf.Max(1, myRank - 2 + modify);
        var max = Mathf.Min(rankCount, myRank + 2 + modify);

        //Debug.Log($"index: {pageIndex} min: {min} max: {max} count: {rankCount}");

        var entries = await model.leaderBoardService.GetLeaderboardEntriesAsync(model.key, min, max).AttachExternalCancellation(token);
        pageIndexRankDatas[pageIndex] = entries;

        await UpdateLeaderboardViewAsync(entries).AttachExternalCancellation(token);
      }
    }


    private async UniTask ShowGlobalRecordFirstAsync()
    {
      var token = view.GetCancellationTokenOnDestroy();
      var entries = await model.leaderBoardService.GetLeaderboardEntriesAsync(model.key, range).AttachExternalCancellation(token);
      //Debug.Log($"rank entries: {entries.Length}");

      for (int i = 0; i < entries.Length; i++)
      {
        if (entries[i].IsMine)
        {
          myRank = entries[i].Rank;
          break;
        }
      }

      pageIndexRankDatas[0] = entries;

      await UpdateLeaderboardViewAsync(entries).AttachExternalCancellation(token);
    }


    private async UniTask CacheLeaderboardAsync()
    {
      await model.leaderBoardService.CacheLeaderboardAsync(model.key);
      rankCount = await model.leaderBoardService.GetRankCount(model.key);
      cachedRankDatas.Clear();
      pageIndexRankDatas.Clear();
    }

    private async UniTask UpdateLeaderboardViewAsync(ILeaderBoardService.RankData[] datas)
    {
      var isNewScore = model.leaderBoardService.IsNewScoreExist(model.key);

      var targetSet = view.GetLeaderboardSet(isDeafultSet);
      var playerIndex = -1;
      for (int i = 0; i < targetSet.RecordSets.Count; i++)
      {
        var set = targetSet.RecordSets[i];
        if (i < datas.Length)
        {
          var data = datas[i];

          set.Record.text = data.Rank.ToString();
          set.Portrait.sprite = data.Portrait;
          set.NickName.text = data.Name;

          if (model.IsFastest)
          {
            var scoreValue = data.Score * LeaderboardUnit.Unit;
            set.Timer.UpdateText(scoreValue);

            var restartcount = data.Deatils[0];
            set.Restart.UpdateText(restartcount);
          }
          else
          {
            var scoreValue = data.Deatils[0] * LeaderboardUnit.Unit;
            set.Timer.UpdateText(scoreValue);

            var restartcount = data.Score;
            set.Restart.UpdateText(restartcount);
          }

          var rank = data.Rank;
          var isUpper = rank < myRank;
          var isMy = rank == myRank;
          var isLess = rank > myRank;
          var color = isUpper ? model.colorSO.RightColor
                              : isMy ? model.colorSO.CenterColor
                                     : model.colorSO.LeftColor;
          set.RectTransform.localScale = Vector3.one;
          set.UpdateColor(color);

          if (isMy)
            playerIndex = i;
          set.gameObject.SetActive(true);
        }
        else
          set.gameObject.SetActive(false);
      }

      if (playerIndex > -1)
      {
        if (isNewScore)
        {
          newPumpCTS.Create();
          var pumpToken = newPumpCTS.token;
          PumpNewRecordAsync(targetSet.RecordSets[playerIndex].RectTransform, pumpToken).Forget();
        }
        else
          targetSet.RecordSets[playerIndex].RectTransform.localScale = model.uiSO.Stage.NewScoreScaleValue * Vector3.one;
      }

      targetSet.LoadingRoot.gameObject.SetActive(false);
      targetSet.LeaderboardRoot.SetActive(true);
    }

    private async UniTask PumpNewRecordAsync(RectTransform rectTransform, CancellationToken token)
    {
      try
      {
        var duration = model.uiSO.Stage.NewScorePumpDuration;
        var scale = model.uiSO.Stage.NewScoreScaleValue;
        await DOTween
          .Sequence()
          .Append(rectTransform.DOScale(scale, duration))
          .Append(rectTransform.DOScale(1.0f, duration))
          .SetLoops(-1)
          .ToUniTask(TweenCancelBehaviour.Kill, token);
      }
      catch (OperationCanceledException)
      {
        if (this != null)
        {
          rectTransform.localScale = Vector3.one;
        }
      }
    }

    private void UpdateLoadingView()
    {
      view.DefaultSet.LoadingRoot.gameObject.SetActive(true);
      view.DefaultSet.LeaderboardRoot.SetActive(false);
    }

    private async void OnSubscribeDelaySubmitComplete(string key)
    {
      if (model.key == key)
      {
        var entries = await model.leaderBoardService.GetLeaderboardEntriesAsync(model.key, range);

        for (int i = 0; i < entries.Length; i++)
        {
          if (entries[i].IsMine)
          {
            myRank = entries[i].Rank;
            break;
          }
        }

        UpdateLeaderboardViewAsync(entries).Forget();
      }
    }

    private async UniTask MovedoctorAsync(RectTransform rectTransform, CancellationToken token)
    {
      try
      {
        var space = rectTransform.rect.width * 0.5f;
        var anchordPosition = Vector2.zero;
        var sign = 1.0f;
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var width = view.RectTransform.rect.width;
          var leftOutline = -width * 0.5f + space;
          var rightOutline = width * 0.5f - space;
          if (anchordPosition.x < leftOutline)
          {
            anchordPosition = new Vector2(leftOutline, 0.0f);
            rectTransform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
            sign = -1.0f;
          }
          else if (anchordPosition.x > rightOutline)
          {
            anchordPosition = new Vector2(rightOutline, 0.0f);
            rectTransform.eulerAngles = Vector3.zero;
            sign = 1.0f;
          }
          rectTransform.anchoredPosition = anchordPosition;

          anchordPosition += 2.0f * sign * Time.deltaTime * view.DoctorMoveSpeed * Vector2.left;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

  }
}
