using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour {

    [Header("Reference Classes")]
    [SerializeField] PlayerObjectInteraction objectInteraction;
    [SerializeField] PlayerLadderInteraction ladderInteraction;
    [SerializeField] PlayerDialogueInteraction dialogueInteraction;

    void OnEnable() {
        PlayerInput.OnInteract += InteractPressed;
    }

    void OnDisable() {
        PlayerInput.OnInteract -= InteractPressed;
    }

    void InteractPressed() {
        if (objectInteraction.PickupObjectInteraction()) return;
        if (dialogueInteraction.AttemptToOpenDialogue()) return;
        if (dialogueInteraction.AttemptToGoToNextDialogue()) return;
    }
}
