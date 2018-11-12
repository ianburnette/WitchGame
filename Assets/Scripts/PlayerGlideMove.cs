using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
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
    
    [Header("Crashing Behavior")]
    [SerializeField] Vector2 crashForce;
    [SerializeField] Collider crashTrigger;

    [Header("Effects")] [SerializeField] TrailRenderer glideTrail;

    [Header("Class References")]
    [SerializeField] PlayerMoveBase MoveBase;
    [SerializeField] CharacterMotor characterMotor;
    [SerializeField] DealDamage playerDealDamage;

    void OnEnable() {
        PlayerInput.OnBroom += JumpPressed;
        PlayerInput.OnMove += Move;
        MoveBase.rigid.drag = RigidbodyDrag;
        MoveBase.animator.SetBool("RidingBroom", true);
        MoveBase.LockCamAndResetOnGround();
        crashTrigger.enabled = true;
        glideTrail.emitting = true;
    }

    void OnDisable() {
        PlayerInput.OnBroom -= JumpPressed;
        PlayerInput.OnMove -= Move;
        crashTrigger.enabled = false;
        glideTrail.emitting = false;
    }

    void FixedUpdate() => Glide();
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

    public void SomethingEnteredTrigger(Collider other)
    {
        print("hit " + other.transform);
        MoveBase.movementStateMachine.NormalMovement();
        playerDealDamage.Attack(gameObject, 0, crashForce.y, crashForce.x, other.ClosestPoint(transform.position));
    }
}
