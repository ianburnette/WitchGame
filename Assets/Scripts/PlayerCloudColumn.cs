using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCloudColumn : MonoBehaviour {

    [SerializeField] CloudColumn cloudColumn;

    [SerializeField] Vector3 cloudOffsetFromPlayer;
    [SerializeField] Vector3 playerOffsetFromCloud;
    [SerializeField] MovementStateMachine movementStateMachine;
    [SerializeField] PlayerMoveBase moveBase;

    public Vector3 CloudOffsetFromPlayer {
        get {
            //TODO make movement-state-dependent offsets return here
            return transform.position + cloudOffsetFromPlayer;
        }
    }

    void OnEnable() {
        PlayerInput.OnMagic += MagicPressed;
    }

    void OnDisable() {
        PlayerInput.OnMagic -= MagicPressed;
    }

    void MagicPressed() {
        if (cloudColumn!=null &&
            movementStateMachine.CurrentMovementState == MoveState.Walk &&
            moveBase.currentlyGrounded)
            if (cloudColumn.AttemptToToggleNewColumn())
                SetPosition();
    }

    void SetPosition() {
        transform.position += playerOffsetFromCloud;
    }

}
