﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLadderInteraction : MonoBehaviour {

    [Header("Ladder Behavior")]
    [SerializeField] bool canGrabLadder;

    [SerializeField] PlayerLadderMove ladderMovement;
    [SerializeField] MovementStateMachine movementStateMachine;

    public void ToggleLadder(bool state, Ladder ladder = null) {
        canGrabLadder  = state;
        ladderMovement.ladder = ladder;
    }

    public void AttemptToGrabLadder() {
        if (ladderMovement.ladder!=null)
            movementStateMachine.GetOnLadder();
    }
}
