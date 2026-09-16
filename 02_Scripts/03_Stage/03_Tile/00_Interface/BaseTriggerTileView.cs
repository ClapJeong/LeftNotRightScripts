using LR.Manager.GameDataManager;
using LR.Stage.TriggerTile.Enum;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace LR.Stage.TriggerTile
{
  public abstract class BaseTriggerTileView : MonoBehaviour, ITriggerTileView
  {
    public bool DisableOnEasy => disableOnEasy;
    [SerializeField] private bool disableOnEasy = false;
    public bool EnableOnHard => enableOnHard;
    [SerializeField] private bool enableOnHard = false;

    protected virtual void Awake()
    {
      if (!disableOnEasy && !enableOnHard)
        return;

      var currentDifficulty = GlobalManager.instance.GameDataService.Value.CurrentDifficulty;
      if (!IsEnableDifficulty(currentDifficulty))
        gameObject.SetActive(false);
    }

    public bool IsEnableDifficulty(IDifficultyService.Difficulty difficulty)
    {
      if (disableOnEasy)
        return difficulty != IDifficultyService.Difficulty.Easy;
      else if(enableOnHard)
        return difficulty == IDifficultyService.Difficulty.Hard;
      else
        return true;
    }

    public abstract TriggerTileType GetTriggerType();

    public abstract void SubscribeOnEnter(UnityAction<Collider2D> onEnter);

    public abstract void SubscribeOnExit(UnityAction<Collider2D> onExit);

    public abstract void UnsubscribeOnEnter(UnityAction<Collider2D> onEnter);

    public abstract void UnsubscribeOnExit(UnityAction<Collider2D> onExit);

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
      if(disableOnEasy)
      {
        var labelCenterStyle = new GUIStyle(EditorStyles.label)
        {
          alignment = TextAnchor.MiddleCenter,
          fontSize = 30,
        };
        labelCenterStyle.normal.textColor = Color.red;
        Handles.Label(transform.position, "X", labelCenterStyle);
      }

      if (enableOnHard)
      {
        var labelCenterStyle = new GUIStyle(EditorStyles.label)
        {
          alignment = TextAnchor.MiddleCenter,
          fontSize = 30,
        };
        labelCenterStyle.normal.textColor = Color.green;
        Handles.Label(transform.position, "+", labelCenterStyle);
      }
    }
#endif
  }
}
