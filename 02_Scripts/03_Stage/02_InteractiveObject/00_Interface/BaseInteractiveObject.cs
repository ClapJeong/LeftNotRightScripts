using LR.Manager.GameDataManager;
using LR.Manager.Sound;
using LR.Manager.Stage;
using UnityEditor;
using UnityEngine;

namespace LR.Stage.InteractiveObject
{
  public abstract class BaseInteractiveObject : MonoBehaviour, IInteractiveObject
  {
    public bool DisableOnEasy => disableOnEasy;
    [SerializeField] private bool disableOnEasy = false;
    public bool EnableOnHard => enableOnHard;
    [SerializeField] private bool enableOnHard = false;

    private void Awake()
    {
      if(disableOnEasy || enableOnHard)
      {
        var currentDifficulty = GlobalManager.instance.GameDataService.Value.CurrentDifficulty;
        if (!IsEnableDifficulty(currentDifficulty))
          gameObject.SetActive(false);
      }      
    }

    public bool IsEnableDifficulty(IDifficultyService.Difficulty difficulty)
    {
      if (disableOnEasy)
        return difficulty != IDifficultyService.Difficulty.Easy;
      else if (enableOnHard)
        return difficulty == IDifficultyService.Difficulty.Hard;
      else
        return true;
    }


    public abstract void Initialize(StageManager stageManager, ISFXController sfxController);

    public abstract void Enable(bool isEnable);

    public abstract void Restart();

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
      if (disableOnEasy)
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
