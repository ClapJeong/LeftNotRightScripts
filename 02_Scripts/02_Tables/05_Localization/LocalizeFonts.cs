using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class LocalizeFonts
{
  [System.Serializable]
  public class Set
  {
    [field: SerializeField] public Locale Locale { get; private set; }
    [field: SerializeField] public TMP_FontAsset FontAsset { get; private set; }
  }

  [field: SerializeField] public List<Set> FontSets { get; private set; }
}
