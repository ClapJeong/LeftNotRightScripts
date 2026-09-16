using UnityEngine;
using UnityEngine.Events;
using static DialogueDataEnum;

namespace LR.Table.Dialogue
{
  [System.Serializable]
  public class DialogueCharacterData: IDirtyPatcher
  {
    private UnityAction onDirty;

    [SerializeField] private int portrait;
    [SerializeField] private Portrait.Emotion portaritEmotion;
    [SerializeField] private Portrait.ChangeType portraitChangeType;
    [SerializeField] private Portrait.AnimationType portraitAnimationType;
    [SerializeField] private string dialogueKey;

    public int Portrait 
    {  
      get => this.portrait;
      set 
      {
        if (portrait == value)
          return;

        portrait = value;
        onDirty?.Invoke();        
      }}
    public Portrait.Emotion PortraitEmotion
    {
      get => this.portaritEmotion;
      set
      {
        if (portaritEmotion == value)
          return;

        onDirty?.Invoke();
        portaritEmotion = value;
      }
    }

    public Portrait.ChangeType PortraitChangeType
    {
      get => this.portraitChangeType;
      set
      {
        if (portraitChangeType == value)
          return;

        onDirty?.Invoke();
        portraitChangeType = value;
      }
    }
    public Portrait.AnimationType PortraitAnimationType
    {
      get => this.portraitAnimationType;
      set
      {
        if (portraitAnimationType == value)
          return;

        portraitAnimationType = value;
        onDirty?.Invoke();        
      }
    }

    public string DialogueKey
    {
      get => this.dialogueKey;
      set
      {
        if (dialogueKey == value)
          return;

        dialogueKey = value;
        onDirty?.Invoke();        
      }
    }

    public DialogueCharacterData(int defaultPortrait, string defaultDialogue, UnityAction onDirty)
    {
      this.onDirty = onDirty;
      portrait = defaultPortrait;
      dialogueKey = defaultDialogue;
      onDirty?.Invoke();
    }

    public void SetOnDirty(UnityAction onDirty)
      => this.onDirty = onDirty;
  }
}