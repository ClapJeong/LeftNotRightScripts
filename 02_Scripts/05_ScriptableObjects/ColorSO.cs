using LR.Manager.GameDataManager;
using LR.Stage.Player.Enum;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorSO", menuName = "SO/Color")]

public class ColorSO : ScriptableObject
{
  [field: SerializeField] public Color LeftColor { get; private set; }
  [field: SerializeField] public Color RightColor { get; private set; }

  public Color GetPlayerColor(PlayerType playerType)
    => GetPlayerColor(playerType == PlayerType.Left);

  public Color GetPlayerColor(bool isLeft)
    => isLeft ? LeftColor : RightColor;

  [field: SerializeField] public Color CenterColor {  get; private set; }

  [field: SerializeField] public List<Color> SignalColors {  get; private set; }

  [field: SerializeField] public Color Easy { get; private set; }
  [field: SerializeField] public Color Normal { get; private set; }
  [field: SerializeField] public Color Hard { get; private set; }

  public Color GetDifficultyClearColor(IDifficultyService.Difficulty difficulty)
    => difficulty switch
    {
      IDifficultyService.Difficulty.Easy => Easy,
      IDifficultyService.Difficulty.Normal => Normal,
      IDifficultyService.Difficulty.Hard => Hard,
      _ => throw new System.NotImplementedException(),
    };
}

