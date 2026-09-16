using Cysharp.Threading.Tasks;
using LR.UI.GameScene.GlobalRecord;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI
{
  public class UISpeedrunLeaderboardView : BaseUIView
  {
    public enum SpeedrunLeaderboardType
    {
      Safest,
      Fastest,
    }
    [System.Serializable]
    public struct HeaderSet
    {
      [field: SerializeField] public SpeedrunLeaderboardType Type { get; private set; }
      [field: SerializeField] public GameObject Header { get; private set; }
    }

    [System.Serializable]
    public struct LeaderboardSet
    {      
      [field: SerializeField] public RectTransform RootRectTransform { get; private set; }
      [field: SerializeField] public List<UISpeedrunRankViewSet> RecordSets { get; private set; }
      [field: SerializeField] public GameObject LeaderboardRoot { get; private set; }
      [field: SerializeField] public RectTransform LoadingRoot { get; private set; }
    }
    [field: SerializeField] public Selectable Selectable { get; private set; }
    [field: SerializeField] public LeaderboardSet DefaultSet { get; private set; }
    [field: SerializeField] public LeaderboardSet AltSet { get; private set; }

    [field: Space(5)]

    [field: SerializeField] public List<RectTransform> DoctorRectTransforms { get; private set; }
    [field: SerializeField] public float DoctorMoveSpeed { get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

    [SerializeField] private SpeedrunLeaderboardType leaderboardType;
    [SerializeField] private List<HeaderSet> headerSets;

    private void OnValidate()
    {
      foreach (var headerSet in headerSets)
        headerSet.Header.SetActive(headerSet.Type == leaderboardType);
    }

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