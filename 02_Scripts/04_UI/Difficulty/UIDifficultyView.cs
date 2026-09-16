using Cysharp.Threading.Tasks;
using LR.Manager.GameDataManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace LR.UI.DifficultySettting
{
  public class UIDifficultyView : BaseUIView
  {
    [System.Serializable]
    public class DifficultyButtonSet
    {
      [field: SerializeField] public UISubmitDirectionSet SubmitDirectionSet { get; private set; }
      [field: SerializeField] public IDifficultyService.Difficulty Difficulty { get; private set; }
      [field: SerializeField] public Selectable Selectable { get; private set; }
      [field: SerializeField] public GameObject Description { get; private set; }
      [field: SerializeField] public CanvasGroup DescriptionCanvasGroup { get; private set; }
      [field: SerializeField] public RectTransform DescriptionRectTransform { get; private set; }
    }

    [field: SerializeField] public List<DifficultyButtonSet> DifficultyButtonSets {  get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }
  }
}