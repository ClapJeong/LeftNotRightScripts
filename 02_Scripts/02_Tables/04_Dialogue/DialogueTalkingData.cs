using UnityEngine;
using UnityEngine.Events;

namespace LR.Table.Dialogue
{
  [System.Serializable]
  public class DialogueTalkingData
  {
    private UnityAction onDirty;
    public string SubName
    {
      get => this.subName;
      set
      {
        if (subName == value)
          return;

        subName = value;
        onDirty?.Invoke();
      }
    }
    public DialogueDataEnum.Background.Shadow Shadow
    {
      get => this.shadow;
      set
      {
        if (shadow == value)
          return;

        shadow = value;
        onDirty?.Invoke();
      }
    }
    public bool BackgroundShake
    {
      get => this.backgroundShake;
      set
      {
        if (backgroundShake == value)
          return;

        backgroundShake = value;
        onDirty?.Invoke();
      }
    }
    public DialogueCharacterData left;
    public DialogueCharacterData center;
    public DialogueCharacterData right;

    [SerializeField] private string subName;
    [SerializeField] private DialogueDataEnum.Background.Shadow shadow;
    [SerializeField] private bool backgroundShake;

    public DialogueTalkingData(DialogueTalkingData previousTalkingData, string subName, UnityAction onDirty)
    {
      this.onDirty = onDirty;
      this.subName = subName;
      left = new(
        previousTalkingData != null ? previousTalkingData.left.Portrait : 0,
        previousTalkingData != null ? previousTalkingData.left.DialogueKey : "dialogue_sample", 
        this.onDirty);
      center = new(
        previousTalkingData != null ? previousTalkingData.center.Portrait : 0,
        previousTalkingData != null ? previousTalkingData.center.DialogueKey : "dialogue_sample", 
        this.onDirty);
      right = new(
        previousTalkingData != null ? previousTalkingData.right.Portrait : 0,
        previousTalkingData != null ? previousTalkingData.right.DialogueKey : "dialogue_sample", 
        this.onDirty);
    }

    public void SetOnDirty(UnityAction onDirty)
    {
      this.onDirty = onDirty;
      left.SetOnDirty(onDirty);
      center.SetOnDirty(onDirty);
      right.SetOnDirty(onDirty);
    }

    public DialogueCharacterData GetCharacterData(CharacterPositionType positionType)
      => positionType switch
      {
        CharacterPositionType.Left => left,
        CharacterPositionType.Center => center,
        CharacterPositionType.Right => right,
        _ => throw new System.NotImplementedException(),
      };
  }
}