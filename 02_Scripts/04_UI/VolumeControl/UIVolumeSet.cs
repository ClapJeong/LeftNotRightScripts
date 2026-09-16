using DG.Tweening;
using LR.Manager.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LR.UI.VolumeControl
{
  public class UIVolumeSet : MonoBehaviour
  {
    private enum IconIndexType
    {
      Less,
      Target,
      More,
    }

    [System.Serializable]
    public class VolumeIconSet
    {
      [field: SerializeField] public Image Image {  get; private set; }
      [field: SerializeField] public RectTransform Content { get; private set; }      
    }
    [field: SerializeField] public RectTransform RectTransform { get; private set; }
    [field: SerializeField] public Selectable Selectable { get; private set; }
    [SerializeField] private VolumeType volumeType;
    [SerializeField] private List<VolumeIconSet> iconSets;

    private readonly Dictionary<VolumeIconSet, Sequence> sequenceMap = new();
    private ColorSO colorSO;
    private UISO uiSO;
    private IVolumeSubscriber volumeSubscriber;

    private int volumeIndex;

    [Inject]
    public void Initialize(
      IVolumeProvider volumeProvider,
      IVolumeSubscriber volumeSubscriber,
      ColorSO colorSO,
      UISO uiSO)
    {
      this.colorSO = colorSO;
      this.uiSO = uiSO;
      this.volumeSubscriber = volumeSubscriber;

      volumeIndex = Mathf.RoundToInt(volumeProvider.GetNormalizedVolume(volumeType) / uiSO.VolumeControl.VolumeUnit) - 1;

      for (int i = 0; i < iconSets.Count; i++)
      {
        var iconIndexType = i < volumeIndex ? IconIndexType.Less
                                            : i == volumeIndex ? IconIndexType.Target
                                                               : IconIndexType.More;
        iconSets[i].Image.color = iconIndexType switch
        {
          IconIndexType.Less => colorSO.LeftColor,
          IconIndexType.Target => colorSO.CenterColor,
          IconIndexType.More => colorSO.RightColor,
          _ => throw new System.NotImplementedException(),
        };
        iconSets[i].Image.SetAlpha(iconIndexType == IconIndexType.More ? uiSO.VolumeControl.DisableAlpha : 1.0f);
        iconSets[i].Content.localScale = Vector3.one * (iconIndexType == IconIndexType.More ? uiSO.VolumeControl.DisableScale : 1.0f);
      }

      volumeSubscriber.SubscribeOnVolumeChanged(OnVolumeChanged);
    }

    private void OnDestroy()
    {
      foreach (var sequence in sequenceMap.Values)
        sequence?.Kill();

      volumeSubscriber.UnsubscribeOnVolumeChanged(OnVolumeChanged);
    }

    private void OnVolumeChanged(VolumeType volumeType, float value)
    {
      if (this.volumeType != volumeType)
        return;

      var targetIndex = Mathf.RoundToInt(value / uiSO.VolumeControl.VolumeUnit) - 1;
      
      if (targetIndex > volumeIndex)
        OnVolumeUp(targetIndex);
      else if(targetIndex < volumeIndex)
        OnVolumeDown(targetIndex);

      volumeIndex = targetIndex;
    }

    private void OnVolumeUp(int targetVolumeIndex)
    {
      if (volumeIndex > -1)
      {
        var currntIconSet = iconSets[volumeIndex];
        ActivateIconSet(currntIconSet, false);
        currntIconSet.Image.color = colorSO.LeftColor;
      }

      var addCount = targetVolumeIndex - volumeIndex;
      for (int i = 0; i < addCount; i++)
      {
        var index = volumeIndex + (i + 1);
        if (index < iconSets.Count)
        {
          var targetIconSet = iconSets[index];
          ActivateIconSet(targetIconSet, selectThisSet: i == addCount - 1);
        }
      }
    }

    private void OnVolumeDown(int targetVolumeIndex)
    {
      if (volumeIndex > -1)
      {
        var currntIconSet = iconSets[volumeIndex];
        DeactivateIconSet(currntIconSet);
      }

      var subCount = volumeIndex - targetVolumeIndex;
      for (int i = 0; i < subCount; i++)
      {
        var index = volumeIndex - (i + 1);
        if (index > -1)
        {
          if (index == targetVolumeIndex)
          {
            iconSets[index].Image.color = colorSO.CenterColor;
          }
          else
          {
            var targetIconSet = iconSets[index];
            DeactivateIconSet(targetIconSet);
          }
        }          
      }
    }

    private void ActivateIconSet(VolumeIconSet set, bool selectThisSet)
    {
      ClearSequence(set);

      set.Image.color = selectThisSet ? colorSO.CenterColor
                                      : colorSO.LeftColor;

      var duration = uiSO.VolumeControl.ScaleDuration;
      sequenceMap[set] = DOTween
        .Sequence()
        .Join(set.Content.DOScale(1.0f, duration))
        .Join(set.Image.DOFade(1.0f, duration))
        .OnComplete(() =>
        {
          ClearSequence(set);
        });
    }

    private void DeactivateIconSet(VolumeIconSet set)
    {
      ClearSequence(set);

      set.Image.color = colorSO.RightColor;

      var duration = uiSO.VolumeControl.ScaleDuration;
      sequenceMap[set] = DOTween
        .Sequence()
        .Join(set.Content.DOScale(uiSO.VolumeControl.DisableScale, duration))
        .Join(set.Image.DOFade(uiSO.VolumeControl.DisableAlpha, duration))
        .OnComplete(() =>
        {
          ClearSequence(set);
        });
    }

    private void ClearSequence(VolumeIconSet set)
    {
      if (sequenceMap.TryGetValue(set, out var existSequence))
      {
        existSequence?.Kill();
        sequenceMap.Remove(set);
      }
    }
  }
}