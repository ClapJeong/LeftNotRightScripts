namespace LR.Stage.Player
{
  public interface IPlayerEnergyProvider
  {
    public float StageMaxEnergy { get; }

    public bool IsDead { get; }

    public bool IsFull { get; }

    public float TotalEnergy { get; }

    public float LeftEnergy { get; }

    public float RightEnergy { get; }

    public float TotalNormalized { get; }

    public float LeftEnergyNormalized { get; }

    public float RightEnergyNormalized { get; }
  }
}