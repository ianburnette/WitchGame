using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class PlayerBroomMove : MonoBehaviour {

    [SerializeField] PlayerMoveBase MoveBase;
    [SerializeField] CharacterMotor characterMotor;

    [SerializeField] float hoverForce;
    [SerializeField] float movementAcceleration;
    [SerializeField] float movementStopDistance;
    [SerializeField] float hoverHeight;
    [SerializeField] float hoverHeightUpperLimit;

    [SerializeField] float rotateSpeed;
    [SerializeField] float decelSpeed;
    [SerializeField] float maxSpeed;

    [SerializeField] float verticalRotationSpeedThreshold;

    [SerializeField] float rigidbodyDrag = .5f;

    void OnEnable() {
        PlayerInput.OnJump += JumpPressed;
        PlayerInput.OnMove += Move;
        MoveBase.rigid.drag = rigidbodyDrag;
        MoveBase.animator.SetBool("RidingBroom", true);
    }

    void OnDisable() {
        PlayerInput.OnJump -= JumpPressed;
        PlayerInput.OnMove -= Move;
        MoveBase.animator.SetBool("RidingBroom", false);
    }

    void JumpPressed() {
        MoveBase.movementStateMachine.ChangeState(this);
    }

    void Move(Vector2 input) {
        var moveDirection = transform.position +
                        MoveBase.MovementRelativeToCamera(input);
        characterMotor.MoveTo(moveDirection,
                              movementAcceleration,
                              movementStopDistance,
                              true);
        if (rotateSpeed != 0 &&
            MoveBase.MovementRelativeToCamera(moveDirection).magnitude != 0)
            characterMotor.RotateToVelocity(rotateSpeed, moveDirection.magnitude > verticalRotationSpeedThreshold);
        characterMotor.ManageSpeed(decelSpeed, maxSpeed, false);

    }

    void FixedUpdate() {
        MoveBase.IsGrounded(hoverHeight);
        characterMotor.
            MoveRelativeToGround(Vector3.up *
                                 hoverForce /
                                 (MoveBase.DistanceToGround() <
                                  hoverHeightUpperLimit ?
                                     MoveBase.DistanceToGround() : .001f ));
    }
}
