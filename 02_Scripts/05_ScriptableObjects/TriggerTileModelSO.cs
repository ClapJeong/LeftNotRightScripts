using UnityEngine;
using LR.Table.TriggerTile;

[CreateAssetMenu(fileName = "TriggerTileSO", menuName = "SO/TriggerTile")]
public class TriggerTileModelSO : ScriptableObject
{
  [field: SerializeField] public ClearTriggerData ClearTrigger {  get; private set; }

  [field: Space(15)]
  [field: SerializeField] public SpikeTriggerData SpikeTrigger { get; private set; }

  [field: Space(15)]
  [field: SerializeField] public DefaultEnergyItemTriggerData DefaultEnergyItemTriggerData { get; set; }
  [field: Space(15)]
  [field: SerializeField] public InputtingEnergyItemTriggerData InputtingEnergyItemTriggerData { get; set; }

  [field: Space(15)]
  [field: SerializeField] public SignalTriggerData SignalTriggerData {  get; set; }
  [field: Space(15)]
  [field: SerializeField] public PortalTriggerData PortalTriggerData { get; set; }
  [field: SerializeField] public ConveyorTriggerTileData ConveyorTriggerTileData { get; private set; }
}
