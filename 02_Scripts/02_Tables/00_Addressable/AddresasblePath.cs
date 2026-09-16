using LR.Stage.Player.Enum;
using UnityEngine;

[System.Serializable]
public class AddresasblePath
{
  [field: SerializeField] public string Player {  get; private set; }
  [field: SerializeField] public string GameObjects { get; private set; }
  [field: SerializeField] public string UI {  get; private set; }
  [field: SerializeField] public string Scene {  get; private set; }
  [field: SerializeField] public string Stage {  get; private set; }  
  [field: SerializeField] public string Effect {  get; private set; }
  [field: SerializeField] public string SpriteAtlas { get; private set; }
}
