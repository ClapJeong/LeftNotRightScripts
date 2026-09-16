using LR.Manager.GameDataManager;
using System.Collections.Generic;
using UnityEngine;

namespace LR.Manager.Stage
{
  public class FloorSetter : MonoBehaviour
  {
    [System.Serializable]
    public class FloorSet
    {
      [field: SerializeField] public GameObject Floor { get; private set; }
      [field: SerializeField] public List<int> Chapters { get; private set; }
    }
    [SerializeField] private List<FloorSet> floorSets;

    public void UpdateFloor(int chapter)
    {
      foreach (var floorSet in floorSets)
        floorSet.Floor.SetActive(floorSet.Chapters.Contains(chapter));
    }
  }
}
