using LR.Stage.Player.Enum;
using UnityEngine;

[System.Serializable]
public class AddressableLabel
{
  [field: SerializeField] public string Preload { get; private set;  }

  [field: SerializeField] public string Stage { get; private set;  }

  [field: SerializeField] public string Dialogue { get; private set; }
}
