using Cysharp.Threading.Tasks;
using DG.Tweening;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using LR.Manager.Sound;
using LR.Table.Dialogue;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;

namespace LR.UI.GameScene.Dialogue.Character
{
  public class TextController
  {
    private const string ItalicTag = "<i>";
    private const char Dot = '.';
    private const string LeftColorOpenTag = "<c_left>";
    private const string RightColorOpenTag = "<c_right>";
    private const string DoctorColorOpenTag = "<c_doc>";
    private const string ColorTagFormat = "<color=#{0}>";
    private const string ColorCloseTagBefore = "</c>";
    private const string ColorCloseTagAfter = "</color>";
    private readonly string leftColorTag;
    private readonly string rightColorTag;
    private readonly string docColorTag;

    private readonly CharacterPositionType positionType;
    private readonly CanvasGroup dialogueBackground;    
    private readonly LocalizeStringEvent dialogueLocalize;
    private readonly TextMeshProUGUI dialogueTMP;
    private readonly TextAnimator_TMP animatorTMP;
    private readonly TypewriterComponent typewriter;
    private readonly ISFXController sfxController;
    private readonly SoundSO soundSO;

    private readonly CTSContainer textCTS = new();

    private bool isTyping = false;

    public TextController(
      CharacterPositionType positionType,
      UITextPresentationData tableData,
      CanvasGroup dialogueBackground,
      LocalizeStringEvent dialogueLocalize, 
      TextMeshProUGUI dialogueTMP,
      TextAnimator_TMP animatorTMP,
      TypewriterComponent typewriter,
      ISFXController sfxController,
      SoundSO soundSO)
    {
      this.positionType = positionType;
      this.dialogueBackground = dialogueBackground;
      this.dialogueLocalize = dialogueLocalize;
      this.dialogueTMP = dialogueTMP;      
      this.animatorTMP = animatorTMP;
      this.typewriter = typewriter;
      this.sfxController = sfxController;
      this.soundSO = soundSO;

      dialogueBackground.alpha = 0.0f;
      leftColorTag = string.Format(ColorTagFormat, ColorUtility.ToHtmlStringRGB(tableData.LeftColor));
      rightColorTag = string.Format(ColorTagFormat, ColorUtility.ToHtmlStringRGB(tableData.RightColor));
      docColorTag = string.Format(ColorTagFormat, ColorUtility.ToHtmlStringRGB(tableData.DoctorColor));
    }


    public async UniTask SetDialogueAsync(string key, UnityAction onNewText)
    {
      textCTS.Cancel();
      textCTS.Create();

      if (string.IsNullOrWhiteSpace(key))
      {
        ClearText();
        await dialogueBackground.DOFade(0.0f, 0.1f);        
      }
      else if(key == dialogueLocalize.StringReference.TableEntryReference.Key)
      {
      }
      else
      {
        onNewText?.Invoke();

        var isTalkingSFX = false;
        var token = textCTS.token;

        isTyping = true;
        await dialogueBackground.DOFade(1.0f, 0.1f);
        typewriter.onTextShowed.AddListener(OnTextShowed);
        await SetLocalizeKeyAsync(key, token);
        var fianlText = dialogueTMP.text;

        if (!fianlText.Contains(ItalicTag))
        {
          typewriter.onCharacterVisible.AddListener(PlayTalkingSFX);
          isTalkingSFX = true;
        }          

        try
        {          
          await UniTask.WaitUntil(() => isTyping == false, PlayerLoopTiming.Update, token);
        }
        catch (OperationCanceledException)
        {          
          animatorTMP.SetText(fianlText);
          isTyping = false;
        }
        finally
        {
          typewriter.onTextShowed.RemoveListener(OnTextShowed);
          if (isTalkingSFX)
            typewriter.onCharacterVisible.RemoveListener(PlayTalkingSFX);
        }
      }
    }


    private void PlayTalkingSFX(CharacterData data)
    {
      if (char.IsWhiteSpace(data.info.character) || data.info.character == Dot)
        return;

      sfxController.PlayOnce(positionType switch
      {
        CharacterPositionType.Left => AudioSourceType.LeftUI,
        CharacterPositionType.Center => AudioSourceType.CenterUI,
        CharacterPositionType.Right => AudioSourceType.RightUI,
        _ => throw new NotImplementedException(),
      }, soundSO.GetRandomTalkingSFX(positionType));
    }

    private void OnTextShowed()
    {
      isTyping = false;
    }

    private async UniTask SetLocalizeKeyAsync(string key, CancellationToken token)
    {
      string resolvedText = null;

      void OnUpdate(string value) => resolvedText = value;

      try
      {
        dialogueLocalize.OnUpdateString.AddListener(OnUpdate);
        dialogueLocalize.SetEntry(key);

        await UniTask.WaitUntil(
            () => resolvedText != null,
            cancellationToken: token);
      }
      catch (OperationCanceledException) { }
      finally
      {
        dialogueLocalize.OnUpdateString.RemoveListener(OnUpdate);

        var processed = ReplaceTags(dialogueTMP.text);

        typewriter.ShowText(processed);
      }
    }

    private string ReplaceTags(string text)
    {
      text = text.Replace(LeftColorOpenTag, leftColorTag);

      text = text.Replace(RightColorOpenTag, rightColorTag);

      text = text.Replace(DoctorColorOpenTag, docColorTag);

      text = text.Replace(ColorCloseTagBefore, ColorCloseTagAfter);

      return text;
    }

    public void CompleteDialogueImmediately()
    {
      textCTS.Cancel();
    }


    public void ClearText()
    {
      dialogueLocalize.SetEntry("");
      dialogueTMP.text = "";
    }
  }
}
