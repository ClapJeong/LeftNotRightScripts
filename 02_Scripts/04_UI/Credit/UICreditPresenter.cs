using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using LR.Manager.Input;
using LR.Manager.UI;
using LR.UI.Enum;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.Credit
{
  public class UICreditPresenter : IUIPresenter
  {
    public class Model
    {
      [Inject] public string addressableKey;
      [Inject] public IGameDataProvider gameDataProvider;
      [Inject] public IInputActionSubscriber inputActionSubscriber;
      [Inject] public UnityAction onAnyPressed;
    }

    private class RectMoveDataSet
    {
      public Vector2 leftTop;
      public Vector2 rightTop;
      public Vector2 leftBottom;
      public Vector2 rightBottom;

      public float horizontalDuration;
      public float verticalDuration;
    }

    private readonly Model model;
    private readonly UICreditView view;

    private readonly SubscribeHandle subscribeHandle;
    private readonly CTSContainer characterMoveCTS = new();

    public UICreditPresenter(Model model, UICreditView view)
    {
      this.model = model;
      this.view = view;

      var isAllComplete = IsAllComplete();

      view
        .ThanksTMP
        .SetEntry(isAllComplete ? "ui_creditAfter" : "ui_creditBefore");

      view.RightyRectTransform.gameObject.SetActive(isAllComplete);
      view.LeftyRectTransform.gameObject.SetActive(isAllComplete);
      view.DoctorRectTransform.gameObject.SetActive(isAllComplete);

      subscribeHandle = new(
        () =>
        {
          model.inputActionSubscriber.SubscribePhase(LRInputType.LeftAny, model.onAnyPressed, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.RightAny, model.onAnyPressed, InputPhase.Performed);
          model.inputActionSubscriber.SubscribePhase(LRInputType.DialogueSkip, model.onAnyPressed, InputPhase.Performed);
        },
        () =>
        {
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.LeftAny, model.onAnyPressed, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.RightAny, model.onAnyPressed, InputPhase.Performed);
          model.inputActionSubscriber.UnsubscribePhase(LRInputType.DialogueSkip, model.onAnyPressed, InputPhase.Performed);
        });

      view.HideAsync(true).Forget();
    }

    public async UniTask ActivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      if (IsAllComplete())
      {
        LayoutRebuilder.ForceRebuildLayoutImmediate(view.RectTransform);

        var leftSet = GetLeftRectSet();
        var rightSet = GetRightSet();

        characterMoveCTS.Cancel();
        characterMoveCTS.Create();
        var moveCTS = characterMoveCTS.token;

        MoveCharacterAsync(view.RightyRectTransform, leftSet, true, moveCTS).Forget();
        MoveCharacterAsync(view.LeftyRectTransform, rightSet, false, moveCTS).Forget();
        MovedoctorAsync(moveCTS).Forget();
      }

      await view.ShowAsync(isImmedieately, token);
      subscribeHandle.Subscribe();
    }    

    public async UniTask DeactivateAsync(bool isImmedieately = false, CancellationToken token = default)
    {
      subscribeHandle.Unsubscribe();
      await view.HideAsync(isImmedieately, token);
      Dispose();
    }

    public IDisposable AttachOnDestroy(GameObject target)
      => target.AttachDisposable(this);

    public void Dispose()
    {
      characterMoveCTS.Dispose();
      subscribeHandle.Dispose();
      if (view)
        view.DestroySelf();
    }

    public VisibleState GetVisibleState()
      => view.GetVisibleState();

    private RectMoveDataSet GetLeftRectSet()
    {
      var leftRectTransform = view.JobOutlineRectTransform;
      var leftWidth = leftRectTransform.rect.width * 0.5f;
      var leftHeight = leftRectTransform.rect.height * 0.5f;
      return new()
      {
        rightTop = new Vector2(leftWidth, leftHeight),
        rightBottom = new Vector2(leftWidth, -leftHeight),
        leftTop = new Vector2(-leftWidth, leftHeight),
        leftBottom = new Vector2(-leftWidth, -leftHeight),

        horizontalDuration = leftWidth / view.Speed * 2.0f,
        verticalDuration = leftHeight / view.Speed * 2.0f,
      };
    }

    private RectMoveDataSet GetRightSet()
    {
      var rightRectTransform = view.NameOutlineRectTransform;
      var rightWidth = rightRectTransform.rect.width * 0.5f;
      var rightHeight = rightRectTransform.rect.height * 0.5f;
      return new()
      {
        rightTop = new Vector2(rightWidth, rightHeight),
        rightBottom = new Vector2(rightWidth, -rightHeight),
        leftTop = new Vector2(-rightWidth, rightHeight),
        leftBottom = new Vector2(-rightWidth, -rightHeight),

        horizontalDuration = rightWidth / view.Speed * 2.0f,
        verticalDuration = rightHeight / view.Speed * 2.0f,
      };
    }

    private async UniTask MoveCharacterAsync(RectTransform characterRectTransform, RectMoveDataSet set, bool isLeft, CancellationToken token)
    {
      var duration = set.horizontalDuration * 0.5f;
      var t = 0.5f;
      characterRectTransform.anchoredPosition = Vector2.Lerp(set.leftTop, set.rightTop, t);
      try
      {
        while (true)
        {
          for(int i = 0; i < 4; i++)
          {
            var beginPoint = i switch
            {
              0 => isLeft ? set.rightTop : set.leftTop,
              1 => isLeft ? set.leftTop : set.rightTop,
              2 => isLeft ? set.leftBottom : set.rightBottom,
              3 => isLeft ? set.rightBottom : set.leftBottom,
              _ => throw new System.NotImplementedException(),
            };
            var endPoint = i switch
            {
              0 => isLeft ? set.leftTop : set.rightTop,
              1 => isLeft ? set.leftBottom : set.rightBottom,
              2 => isLeft ? set.rightBottom : set.leftBottom,
              3 => isLeft ? set.rightTop : set.leftTop,
              _ => throw new System.NotImplementedException(),
            };
            var eulerUnit = isLeft ? 90.0f : -90.0f;
           
            var beforeQuaternion = Quaternion.Euler(0.0f, 0.0f, (i - 1) * eulerUnit);
            var targetQuaternion = Quaternion.Euler(0.0f, 0.0f, i * eulerUnit);
            var afterQuaternion = Quaternion.Euler(0.0f, 0.0f, (i + 1) * eulerUnit);

            var targetDuration = i % 2 == 0 ? set.horizontalDuration
                                            : set.verticalDuration;

            while(duration < targetDuration)
            {
              token.ThrowIfCancellationRequested();

              t = duration / targetDuration;
              characterRectTransform.anchoredPosition = Vector2.Lerp(beginPoint, endPoint, t);

              if(t < view.CornerRange)
              {
                var cornerT = Mathf.InverseLerp(-view.CornerRange, view.CornerRange, t);
                characterRectTransform.rotation = Quaternion.Lerp(beforeQuaternion, targetQuaternion, cornerT);
              }
              else if(t > 1.0f - view.CornerRange)
              {
                var cornerT = Mathf.InverseLerp(1.0f - view.CornerRange, 1.0f + view.CornerRange, t);
                characterRectTransform.rotation = Quaternion.Lerp(targetQuaternion, afterQuaternion, cornerT);
              }
              else
              {
                if(characterRectTransform.rotation != targetQuaternion)
                   characterRectTransform.rotation = targetQuaternion;
              }

              duration += Time.deltaTime;
              await UniTask.Yield();

              //Debug.Log($"i: {i} duration: {duration} targetDuration: {targetDuration}");
            }

            duration = 0.0f;
            t = 0.0f;
          }
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

    private async UniTask MovedoctorAsync(CancellationToken token)
    {
      try
      {
        var space = view.DoctorRectTransform.rect.width * 0.5f;
        var anchordPosition = Vector2.zero;
        var sign = 1.0f;
        while (true)
        {
          token.ThrowIfCancellationRequested();

          var leftOutline = -Screen.width * 0.5f + space;
          var rightOutline = Screen.width * 0.5f - space;
          if (anchordPosition.x < leftOutline)
          {
            anchordPosition = new Vector2(leftOutline, 0.0f);
            view.DoctorRectTransform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
            sign = -1.0f;
          }
          else if(anchordPosition.x > rightOutline)
          {
            anchordPosition = new Vector2(rightOutline, 0.0f);
            view.DoctorRectTransform.eulerAngles = Vector3.zero;
            sign = 1.0f;
          }
          view.DoctorRectTransform.anchoredPosition = anchordPosition;

          anchordPosition += 2.0f * sign * Time.deltaTime * view.Speed * Vector2.left;
          await UniTask.Yield();
        }
      }
      catch (OperationCanceledException) { }
    }

    private bool IsAllComplete()
      => model.gameDataProvider.GetMaxClearIndex() == model.gameDataProvider.StageDataCount;

    public static async UniTask<IUIPresenter> CreateAsync(
      DiContainer diContainer,
      UnityAction onExit)
    {
      var addressableKeySO = diContainer.Resolve<AddressableKeySO>();
      var canvasProvider = diContainer.Resolve<ICanvasProvider>();
      var resourceManager = diContainer.Resolve<IResourceManager>();

      var key =
        addressableKeySO.Path.UI +
        addressableKeySO.UIName.Credit;
      var root = canvasProvider.GetCanvas(RootType.Popup).transform;
      var model = diContainer.Instantiate<UICreditPresenter.Model>(new object[]
      {
        key,
        onExit
      });
      var view = await resourceManager.CreateAssetAsync<UICreditView>(key, root);
      var presenter = new UICreditPresenter(model, view);

      return presenter;
    }
  }
}