using UnityEngine;
using UnityEngine.Localization;
using ScriptableEvent;
using TMPro;
using UnityEngine.UI;
using System.Linq;

namespace LR.UI.Debugging
{
  public class DebuggingUI : MonoBehaviour
  {
    [SerializeField] private KeyCode enableKeyCode = KeyCode.F1;
    [SerializeField] private GameObject root;
    [SerializeField] private ScriptableEventSO scriptableEventSO;
    [SerializeField] private TextMeshProUGUI selectedStageIndexText;
    [SerializeField] private TextMeshProUGUI clearStageCountTMP;

    [Header("[ Dialogue Conditions ]")]
    [SerializeField] private GameObject conditionArea;
    [SerializeField] private TextMeshProUGUI currentDialogueConditions;
    [SerializeField] private TMP_InputField conditionKeyInputField;
    [SerializeField] private TMP_InputField conditionLeftInputField;
    [SerializeField] private TMP_InputField conditionRightInputField;

    private void Awake()
    {
      root.SetActive(false);
    }

    private void Update()
    {
      if (UnityEngine.Input.GetKeyDown(enableKeyCode))
      {
        root.SetActive(!root.activeInHierarchy);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
      }

      if (GlobalManager.instance.GameDataService.Value != null)
      {
        var gameDataService = GlobalManager.instance.GameDataService.Value;
        var chapter = gameDataService.GetSelectedChapter();
        var stage = gameDataService.GetSelectedStage();
        selectedStageIndexText.text = $"current: {chapter}/{stage}";

        var clearIndex = gameDataService.GetMaxClearIndex();
        clearStageCountTMP.text = $"clear: {Mathf.Max(0, clearIndex)}";
      }
    }

    public void OnLocaleButtonClicked(Locale locale)
      => scriptableEventSO.OnLocaleChanged(locale);

    public void OnStageButtonClicked(int stageEventType)
      => scriptableEventSO.OnStageEvent((StageEventType)stageEventType);

    public void OnGameDataButtonClicked(int gameDataEventType)
    {
      var type = (GameDataEventType)gameDataEventType;
      switch (type)
      {
        default:
          {
            scriptableEventSO.OnGameDataEvent(type);
          }
          break;
      }
    }

    public void OnLeftEnergyButtonClicked(float value)
      => scriptableEventSO.OnLeftEnergyChanged(value);

    public void OnRightEnergyButtonClicked(float value)
      => scriptableEventSO.OnRightEnergyChanged(value);
  }
}