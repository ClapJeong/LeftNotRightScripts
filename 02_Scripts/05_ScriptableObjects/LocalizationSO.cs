using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationTableSO", menuName = "SO/Localization")]

public class LocalizationSO : ScriptableObject
{
  [field: SerializeField] public TableName TableName {  get; private set; }

  [field: SerializeField] public LocalizeFonts LocalizeFonts { get; private set; }
}
