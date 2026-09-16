using System.Collections.Generic;
using UnityEngine;

namespace LR.UI
{
  public class UIEasyHatView : MonoBehaviour
  {
    [System.Serializable]
    public class HatPositionSet
    {
      [field: SerializeField] public List<int> Indexes { get; private set; }
      [field: SerializeField] public Vector2 AnchoredPosition { get; private set; }
    }

    [field: SerializeField] public RectTransform EasyHat { get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    [field: SerializeField] public List<HatPositionSet> HatPositionSets { get; private set; }

    private Vector2 initializedAnchroedPosition;

    public void Initialize(bool isEnable)
    {
      EasyHat.gameObject.SetActive(isEnable);
      initializedAnchroedPosition = EasyHat.anchoredPosition;
    }

    public void UpdateHatPosition(int portraitIndex)
    {
      if (!EasyHat.gameObject.activeSelf)
        return;

      foreach (var set in HatPositionSets)
      {
        if (set.Indexes.Contains(portraitIndex))
        {
          EasyHat.anchoredPosition = set.AnchoredPosition;
          return;
        }
      }

      EasyHat.anchoredPosition = initializedAnchroedPosition;
    }
  }
}