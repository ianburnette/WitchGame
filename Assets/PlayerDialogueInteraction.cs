using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueInteraction : MonoBehaviour
{
    NpcDialogue currentDialogue;
    public bool AttemptToOpenDialogue()
    {
        if (!DialogueBase.instance.InRange || DialogueBase.instance.InDialogue) return false;
        DialogueBase.instance.ShowBubble();
        InputPrompt.instance.Prompt = null;
        return true;
    }
    
    public bool AttemptToGoToNextDialogue()
    {
        if (!DialogueBase.instance.InDialogue) return false;
        DialogueBase.instance.ShowNextDialogue();
        return true;
    }
}
