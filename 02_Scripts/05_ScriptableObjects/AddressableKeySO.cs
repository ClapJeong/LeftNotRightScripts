using UnityEngine;

[CreateAssetMenu(fileName ="AdressableKeySO",menuName ="SO/AdressableKey")]
public class AddressableKeySO: ScriptableObject
{
  [Header("[ Labels ]")]
  [field: SerializeField] public AddressableLabel Label {  get; private set; }

  [Header("[ Paths ]")]
  [field: SerializeField] public AddresasblePath Path {  get; private set; }

  [field: Space(10)]
  [Header("Names")]
  [field: SerializeField] public SceneName SceneName {  get; private set; }

  [field: SerializeField] public GameObjectName GameObjectName {  get; private set; }

  [field: SerializeField] public UIName UIName {  get; private set; }

  [field: SerializeField] public StageName StageName {  get; private set; }

  [field: SerializeField] public AtlasName AtlasName { get; private set; }
}
