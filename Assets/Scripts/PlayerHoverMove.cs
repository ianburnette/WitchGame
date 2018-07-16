using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class PlayerHoverMove : MonoBehaviour {

    [Header("Movement Behavior")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float movementSpeed;
    [SerializeField] float groundRepelForce;
    [Range(.01f, 1)] [SerializeField] float movementSensitivity;
    [SerializeField] float maxSpeed;
    [SerializeField] float rotateSpeed;

    [Header("Passive Behavior")]
    [SerializeField] float tooFastDecelSpeed;
    [SerializeField] float heightAboveWhichToGlide;
    [SerializeField] float heightBelowWhichToApplyHoverForce;
    [SerializeField] float rigidbodyDrag = .5f;
    [SerializeField] float distanceFromGroundForceMult = 2f;

    [Header("Animation Variables")]
    [SerializeField] float minYrotation = -1f;
    [SerializeField] float maxYrotation = 1f;
    [SerializeField] float yMultiplier = 1f;

    [Header("Class References")]
    [SerializeField] PlayerMoveBase MoveBase;
    [SerializeField] CharacterMotor characterMotor;

    void OnEnable() {
        PlayerInput.OnJump += JumpPressed;
        PlayerInput.OnMove += Move;
        PlayerInput.OnBroom += BroomPressed;
        MoveBase.rigid.drag = rigidbodyDrag;
        MoveBase.animator.SetBool("RidingBroom", true);
    }

    void OnDisable() {
        PlayerInput.OnJump -= JumpPressed;
        PlayerInput.OnMove -= Move;
        PlayerInput.OnBroom -= BroomPressed;
    }

    void JumpPressed() => MoveBase.movementStateMachine.NormalMovement();
    void BroomPressed() => MoveBase.movementStateMachine.NormalMovement();
    void SwitchToGlide() => MoveBase.movementStateMachine.GlideMovement();
    void Hover() =>
        characterMotor.MoveRelativeToGround(Vector3.up * groundRepelForce /
                                            (MoveBase.DistanceToGround() * distanceFromGroundForceMult));

    void FixedUpdate() {
        if (MoveBase.IsGrounded(heightBelowWhichToApplyHoverForce, groundMask))
            Hover();
        else if (!MoveBase.IsGrounded(heightAboveWhichToGlide, groundMask))
            SwitchToGlide();
        characterMotor.RotateToVelocity(rotateSpeed, minYrotation, maxYrotation, yMultiplier);
        characterMotor.ManageSpeed(tooFastDecelSpeed, maxSpeed, false);
    }

    void Move(Vector2 movement) => characterMotor.MoveTo(MoveBase.MovementRelativeToPlayerAndCamera(movement),
                                                         movementSpeed,
                                                         movementSensitivity,
                                                         ignoreY:true);


}
