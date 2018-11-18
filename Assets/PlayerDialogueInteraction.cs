using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueInteraction : MonoBehaviour
{
    public void AttemptToGoToNextDialogue()
    {
        if (DialogueBase.instance.InDialogue)
            DialogueBase.instance.ShowNextDialogue();
    }
}
