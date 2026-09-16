using UnityEngine;

namespace LR.Stage.SignalListener
{
  public class FloatingModule
  {
    [System.Serializable]
    public class Model
    {
      [field: SerializeField] public float Range { get; private set; }
      [field: SerializeField] public float Speed { get; private set; }
    }

    private enum Quater
    {
      LeftUp,
      RightUp,
      LeftDown,
      RightDown,
    }

    private readonly Transform transform;
    private readonly Model model;

    private readonly Vector3 initializedPosition;
    private Quater targetQuater;    
    private Vector3 currentOffset;
    private Vector3 targetOffset;    

    public FloatingModule(Transform transform, Model model)
    {
      this.transform = transform;
      this.model = model;

      initializedPosition = transform.position;
      currentOffset = Vector3.zero;
      targetQuater = (Quater)Random.Range(0, 4);
      targetOffset = GetRandomOffset(targetQuater);
    }

    public void OnUpdate()
    {
      currentOffset = Vector3.MoveTowards(currentOffset, targetOffset, Time.deltaTime * model.Speed);
      if(currentOffset == targetOffset)
      {
        targetQuater = GetRandomQuater(targetQuater);
        targetOffset = GetRandomOffset(targetQuater);
      }
      transform.position = initializedPosition + currentOffset;
    }

    private Quater GetRandomQuater(Quater previousQuater)
    {
      var randomQuater = (Quater)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Quater)).Length);
      while(randomQuater == previousQuater)
        randomQuater = (Quater)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Quater)).Length);
      return randomQuater;
    }

    private Vector3 GetRandomOffset(Quater quater)
    {
      var xSign = quater switch
      {
        Quater.LeftUp => -1,
        Quater.RightUp => 1,
        Quater.LeftDown => -1,
        Quater.RightDown => 1,
        _ => throw new System.NotImplementedException(),
      };
      var ySign = quater switch
      {
        Quater.LeftUp => 1,
        Quater.RightUp => 1,
        Quater.LeftDown => -1,
        Quater.RightDown => -1,
        _ => throw new System.NotImplementedException(),
      };

      var randomOffset = new Vector2(
        xSign * Random.Range(0, model.Range),
        ySign * Random.Range(0, model.Range));
      return randomOffset;
    }
  }
}
