using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Table.Dialogue
{
  [System.Serializable]
  public class DialogueData : IDirtyPatcher
  {
    private UnityAction onDirty;

    [SerializeField] private List<DialogueTalkingData> talkingDatas = new();
    public List<DialogueTalkingData> TalkingDatas => talkingDatas;

    public DialogueData(UnityAction onDirty)
    {
      this.onDirty = onDirty;
      talkingDatas.Add(new DialogueTalkingData(null, "1", this.onDirty));
    }

    public void AddTalkingData()
    {
      var previousTalkingData = talkingDatas.LastOrDefault();
      talkingDatas.Add(new DialogueTalkingData(previousTalkingData, (talkingDatas.Count+1).ToString(), onDirty));
      onDirty?.Invoke();
    }

    public void RemoveTalkingData(DialogueTalkingData talkingData)
    {
      if(talkingDatas.Contains(talkingData))
        talkingDatas.Remove(talkingData);
      onDirty?.Invoke();
    }

    public void SetOnDirty(UnityAction onDirty)
    {
      this.onDirty = onDirty;
      foreach(var talkingData in talkingDatas)
        talkingData.SetOnDirty(onDirty);
    }
  }
}