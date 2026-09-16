namespace LR.Manager.Analystic
{
  public interface IDialogueAnalysticService
  {
    public void SendDialogueEvent(int index, bool isSkipped);
  }

  public class DialogueSkipEvent : Unity.Services.Analytics.Event
  {
    public DialogueSkipEvent() : base("dialogueEvent")
    {

    }

    public int DialogueIndex { set { SetParameter("dialogueIndex", value); } }
    public bool DialogueSkipped { set { SetParameter("dialogueSkipped", value); } }
  }

}
