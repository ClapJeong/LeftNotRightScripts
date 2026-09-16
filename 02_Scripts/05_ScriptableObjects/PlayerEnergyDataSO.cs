using UnityEngine;

namespace LR.Table.Player
{
  [CreateAssetMenu(fileName = "PlayerEnergyDataSO", menuName = "SO/PlayerEnergyData")]
  public class PlayerEnergyDataSO : ScriptableObject
  {
    [field: SerializeField] public float NormalDecreasingValue { get; private set; }
    [field: SerializeField] public float EasyDecreasingValue { get; private set; }

    [field: SerializeField] public float StopModifier { get; private set; }

    [field: SerializeField] public float InvincibleDuration { get; private set; }

    [field: SerializeField]public float InvincibleBlinkInterval { get; private set; }

    [field: SerializeField] public float InvincibleBlinkAlphaMax { get; private set; }

    [field: SerializeField] public float InvincibleBlinkAlphaMin { get; private set; }

    [field: Header("[ Death Bonus ]")]
    [field: SerializeField] public int DeathStep { get; private set; }

    [field: SerializeField] public float DeathBonusValue { get; private set; }
  }
}