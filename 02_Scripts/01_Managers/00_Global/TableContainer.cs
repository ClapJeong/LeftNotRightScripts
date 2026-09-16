using UnityEngine;
using LR.Stage.Player.Enum;
using LR.Table.Player;


public class TableContainer : MonoBehaviour
{  

  [field: SerializeField] public AddressableKeySO AddressableKeySO {  get; private set; }

  [field: SerializeField] public PlayerEnergyDataSO PlayerEnergyDataSO { get; set; }

  [field: SerializeField] public PlayerModelSO PlayerModelSO { get; private set; }

  [field: SerializeField] public TriggerTileModelSO TriggerTileModelSO {  get; private set; }

  [field: SerializeField] public UISO UISO {  get; private set; }

  [field: SerializeField] public LocalizationSO LocalizationSO { get; private set; }

  [field: SerializeField] public DialogueUIDataSO DialogueUIDataSO { get; private set; }

  [field: SerializeField] public EffectTableSO EffectTableSO { get; private set; }

  [field: SerializeField] public StageGimmickSO StageGimmickSO { get; private set; }

  [field: SerializeField] public CameraDataSO CameraDataSO { get; private set; }

  [field: SerializeField] public ColorSO ColorSO { get; private set; }

  [field: SerializeField] public SoundSO SoundSO  { get; private set; }

  [field: SerializeField] public StageCompleteDataSO StageCompleteDataSO { get; private set; }

  [field: SerializeField] public StageShowDataSO StageShowDataSO { get; private set; }

  [field: SerializeField] public WallHitObjectSO WallHitObjectSO { get; private set; }

  [field: SerializeField] public GlobalModifierSO GlobalModifierSO { get; private set; }
}
