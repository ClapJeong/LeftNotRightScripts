using LR.Manager.GameDataManager;
using LR.Stage.StageDataContainer;
using LR.Table.Dialogue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace LR.Manager.Stage
{
  public class DialogueDataContainer : IDialoguePlayableProvider
  {
    [Inject] private readonly IGameModeService gameModeService = null;
    [Inject] private readonly IGameDataProvider gameDataProvider = null;

    private DialogueData beforeDialogueData;
    private DialogueData afterDialogueData;


    public void CacheDialogueDatas(
      List<AsyncOperationHandle> handles,
      StageDataContainer stageDataContainer)
    {
      foreach (var handle in handles)
      {
        var data = handle.Result as TextAsset;
        var index = int.Parse(data.name);
        if (index == stageDataContainer.beforeDialogueIndex)
          beforeDialogueData = JsonUtility.FromJson<DialogueData>(data.text);
        else if (index == stageDataContainer.afterDialogueIndex)
          afterDialogueData = JsonUtility.FromJson<DialogueData>(data.text);
      }
    }

    #region IDialoguePlayableProvider
    public bool IsFirstDialogueExist()
      => TryGetBeforeDialogueData(out var _);

    public bool TryGetBeforeDialogueData(out DialogueData dialogueData)
    {
      if (gameModeService.IsSpeedRun)
      {
        dialogueData = null;
        return false;
      }

      dialogueData = beforeDialogueData;
      var chapter = gameDataProvider.GetSelectedChapter();
      var stage = gameDataProvider.GetSelectedStage();
      var isClearStage = gameDataProvider.IsClearStage(chapter, stage, out var _);
      var isDialogueDataExist = beforeDialogueData != null;      
      return isClearStage == false && isDialogueDataExist;
    }

    public bool TryGetAfterDialogueData(out DialogueData dialogueData, bool isThisStageFirst)
    {
      if (gameModeService.IsSpeedRun)
      {
        dialogueData = null;
        return false;
      }

      dialogueData = afterDialogueData;    
      if (isThisStageFirst)
      {
        var isDialogueDataExist = afterDialogueData != null;
        return isDialogueDataExist;
      }
      else
      {
        var chapter = gameDataProvider.GetSelectedChapter();
        var stage = gameDataProvider.GetSelectedStage();
        var isClearStage = gameDataProvider.IsClearStage(chapter, stage, out var _);
        var isDialogueDataExist = afterDialogueData != null;
        return isClearStage == false && isDialogueDataExist;
      }
    }
    #endregion
  }
}
