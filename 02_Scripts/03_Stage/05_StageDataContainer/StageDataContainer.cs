using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LR.Stage.TriggerTile;
using LR.Stage.InteractiveObject;
using LR.Stage.Complete;
using UnityEngine.Rendering.Universal;
using LR.Manager.GameDataManager;

namespace LR.Stage.StageDataContainer
{
  public class StageDataContainer : MonoBehaviour
  {
    [System.Serializable]
    public struct EnergySet
    {
      public float minimum;
      public float extra;      
    }
    public bool IsDoctorExist => stageCompleteController.IsDoctorExist;

    public int beforeDialogueIndex = -1;
    public int afterDialogueIndex = -1;
    public float cameraSize;
    public StageGimmick stageGimmick;
    public EnergySet easyNormalEnergySet;
    public EnergySet hardEnergySet;
    public Transform leftPlayerBeginTransform;
    public Transform rightPlayerBeginTransform;
    public Light2D leftExhaustLight;
    public ClearTriggerTileView leftPlayerClearTileView;
    public ClearTriggerTileView rightPlayerClearTileView;
    public Light2D rightExhaustLight;
    public Transform playerRoot;
    public GameObject staticObstacle;
    public Transform otherObjectsRoot;
    public StageCompleteController stageCompleteController;
    public Transform lightRoot;
    public Light2D doctorLight;    

    public List<ITriggerTileView> TriggerTiles => otherObjectsRoot.GetComponentsInChildren<ITriggerTileView>().ToList();
    public List<SignalListener.SignalListener> SignalListeners => otherObjectsRoot.GetComponentsInChildren<SignalListener.SignalListener>().ToList();
    public List<IInteractiveObject> InteractiveObject => otherObjectsRoot.GetComponentsInChildren<IInteractiveObject>().ToList();

    private void OnValidate()
    {
      if (Camera.main != null)
        Camera.main.orthographicSize = cameraSize;
    }

    private void OnDrawGizmos()
    {
      if (cameraSize <= 0.0f)
        return;

      Gizmos.color = Color.aquamarine;
      var widthRatio = 16.0f / 9.0f;
      var widthHalf = cameraSize * widthRatio;
      var heightHalf = cameraSize;

      var leftTop = new Vector2(-widthHalf, heightHalf);
      var rightTop = new Vector2(widthHalf, heightHalf);
      var leftBottom = new Vector2(-widthHalf, -heightHalf);
      var rightBottom = new Vector2(widthHalf, -heightHalf);

      Gizmos.DrawLine(leftTop, rightTop);
      Gizmos.DrawLine(leftTop, leftBottom);
      Gizmos.DrawLine(rightBottom, rightTop);
      Gizmos.DrawLine(rightBottom, leftBottom);

      Gizmos.color = Color.yellow;
      Gizmos.DrawLine(Vector2.up * (heightHalf + 2.0f), Vector2.down * (heightHalf + 2.0f));
      Gizmos.DrawLine(Vector2.left * (widthHalf + 2.0f), Vector2.right * (widthHalf + 2.0f));
    }

    public float GetTotalEnergy(IDifficultyService.Difficulty difficulty)
      => difficulty switch
      {
        IDifficultyService.Difficulty.Easy => (easyNormalEnergySet.minimum + easyNormalEnergySet.extra) * 2.0f,
        IDifficultyService.Difficulty.Normal => easyNormalEnergySet.minimum + easyNormalEnergySet.extra,
        IDifficultyService.Difficulty.Hard => hardEnergySet.minimum + hardEnergySet.extra,
        _ => throw new System.NotImplementedException(),
      };
  }
}