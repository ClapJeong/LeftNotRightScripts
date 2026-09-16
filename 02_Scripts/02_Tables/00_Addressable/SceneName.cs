using UnityEngine;

[System.Serializable]
public class SceneName
{  
  [field: SerializeField] public string Preloading { get; private set; }
  [field: SerializeField] public string Lobby { get; private set; }
  [field: SerializeField] public string Game { get; private set; }
  [field: SerializeField] public string Epilogue { get; private set; }
}
