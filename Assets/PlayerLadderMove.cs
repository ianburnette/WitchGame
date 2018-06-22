using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLadderMove : MonoBehaviour {
    [SerializeField] PlayerMoveBase MoveBase;
    [SerializeField] float climbSpeed;
    [SerializeField] float climbAccuracy;

    void OnEnable() {
        PlayerInput.OnMove += Move;
    }

    void OnDisable() {
        PlayerInput.OnMove -= Move;
    }

    void Move(Vector2 movementInput) {
        MoveBase.characterMotor.MoveTo(Vector3.up * movementInput.y, climbSpeed, climbAccuracy, false);
    }
}
