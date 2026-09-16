using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.GameScene.GlobalRecord
{
  public class UIGlobalRecordView : BaseUIView
  {
    [System.Serializable]
    public class LeaderboardSet
    {
      [field: SerializeField] public RectTransform RootRectTransform { get; private set; }
      [field: SerializeField] public List<UIGlobalRecordSet> RecordSets { get; private set; }
      [field: SerializeField] public GameObject LeaderboardRoot { get; private set; }
      [field: SerializeField] public RectTransform LoadingRoot { get; private set; }
    }
    [field: SerializeField] public Selectable Selectable { get; private set; }
    [field: SerializeField] public LeaderboardSet DefaultSet { get; private set; }
    [field: SerializeField] public LeaderboardSet AltSet { get; private set; }

    [field: SerializeField] public List<RectTransform> DoctorRectTransforms { get; private set; }
    [field: SerializeField] public float DoctorMoveSpeed {  get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
      gameObject.SetActive(false);
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
      gameObject.SetActive(true);
    }

    public LeaderboardSet GetLeaderboardSet(bool isDefault)
      => isDefault switch
      {
        true => DefaultSet,
        false => AltSet,
      };
  }
}