using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour {

    [Header("Reference Classes")]
    [SerializeField] PlayerObjectInteraction objectInteraction;
    [SerializeField] PlayerLadderInteraction ladderInteraction;

    void OnEnable() {
        PlayerInput.OnGrab += GrabPressed;
    }

    void OnDisable() {
        PlayerInput.OnGrab -= GrabPressed;
    }

    void GrabPressed() {
        if (objectInteraction.PickupObjectInteraction()) return;
        ladderInteraction.AttemptToGrabLadder();
    }
}
