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
    [SerializeField] float rotateSpeed;
    [SerializeField] float decelSpeed;
    [SerializeField] float maxSpeed;

    [SerializeField] float minYrotation = -1f, maxYrotation = 1f, yMultiplier = 1f;

    [SerializeField] float rigidbodyDrag = .5f;
    [SerializeField] float distanceMult = 1f;

    [SerializeField] bool canGlide;
    [SerializeField] float glideSpeed = 5f;
    [SerializeField] float glideHoverForce = 25f;

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
        MoveBase.movementStateMachine.NormalMovement();
    }

    void Move(Vector2 movement) {
        var moveDirection = transform.position +
                        MoveBase.MovementRelativeToCamera(movement);
        characterMotor.MoveTo(moveDirection,
                              movementAcceleration,
                              movementStopDistance,
                              true);

    }

    void FixedUpdate() {
        if (MoveBase.IsGrounded(hoverHeight))
            Hover();
        else if (canGlide)
            Glide();

        characterMotor.RotateToVelocity(rotateSpeed, minYrotation, maxYrotation, yMultiplier);
        characterMotor.ManageSpeed(decelSpeed, maxSpeed, false);
    }

    void Hover() => characterMotor.MoveRelativeToGround(Vector3.up * hoverForce / (MoveBase.DistanceToGround() * distanceMult));

    void Glide() {
        Move(Vector3.forward * glideSpeed);
        characterMotor.MoveRelativeToGround(glideHoverForce * Vector3.down);
    }
}
