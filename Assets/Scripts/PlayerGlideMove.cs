using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGlideMove : MonoBehaviour {

    [Header("Movement Behavior")]
    [SerializeField] float glideSpeed = 5f;
    [SerializeField] float turnSpeed;
    [SerializeField] float groundRepelForce = 25f;
    [SerializeField] LayerMask groundMask;

    [Header("Passive Behavior")]
    [SerializeField] float heightToSwitchToHover;
    [SerializeField] float heightToSwitchToWalking;
    [SerializeField] const float RigidbodyDrag = .5f;

    [Header("Class References")]
    [SerializeField] PlayerMoveBase MoveBase;
    [SerializeField] CharacterMotor characterMotor;

    void OnEnable() {
        PlayerInput.OnJump += JumpPressed;
        PlayerInput.OnMove += Move;
        MoveBase.rigid.drag = RigidbodyDrag;
        MoveBase.animator.SetBool("RidingBroom", true);
    }

    void OnDisable() {
        PlayerInput.OnJump -= JumpPressed;
        PlayerInput.OnMove -= Move;
    }

    void FixedUpdate() {
        Glide();
    }

    void JumpPressed() => MoveBase.movementStateMachine.NormalMovement();

    void Glide() {
        if (!MoveBase.IsGrounded(MinimumHeightToGlideAbove, groundMask)) {
            characterMotor.SetVelocity(transform.forward * glideSpeed);
            characterMotor.MoveRelativeToGround(groundRepelForce * Vector3.down);
        } else if (MoveBase.playerAbilities.hoveringUnlocked)
            MoveBase.movementStateMachine.HoverMovement();
        else
            MoveBase.movementStateMachine.NormalMovement();
    }

    public float MinimumHeightToGlideAbove {
        get { return MoveBase.playerAbilities.hoveringUnlocked ? heightToSwitchToHover : heightToSwitchToWalking; }
    }

    void Move(Vector2 movement) {
        if (movement != Vector2.zero)
            characterMotor.RotateToDirection
                (MoveBase.MovementRelativeToPlayerAndCamera(movement),
                 turnSpeed,
                 ignoreY: true);
    }
}
