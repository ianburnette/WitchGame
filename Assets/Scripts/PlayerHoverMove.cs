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
    [SerializeField] float minDistFromGround, maxDistFromGround;

    [Header("Animation Variables")]
    [SerializeField] float minYrotation = -1f;
    [SerializeField] float maxYrotation = 1f;
    [SerializeField] float yMultiplier = 1f;
    [SerializeField] float slopeMatchSpeed = 2f;
    
    [Header("Velocity Facing")]
    [SerializeField] float minSpeedForVelocityFacing; 
    [SerializeField] float minVelocityFacingTurnSpeed; 
    [SerializeField] float maxVeloctiyFacingTurnSpeed; 


    [Header("Class References")]
    [SerializeField] PlayerMoveBase MoveBase;
    [SerializeField] CharacterMotor characterMotor;

    void OnEnable() {
        PlayerInput.OnJump += JumpPressed;
        PlayerInput.OnMove += Move;
        PlayerInput.OnBroom += BroomPressed;
        MoveBase.rigid.drag = rigidbodyDrag;
        MoveBase.animator.SetBool("RidingBroom", true);
        MoveBase.MatchSlopeAngle(true, onBack:false,speed:slopeMatchSpeed);
    }

    void OnDisable() {
        PlayerInput.OnJump -= JumpPressed;
        PlayerInput.OnMove -= Move;
        PlayerInput.OnBroom -= BroomPressed;
        MoveBase.MatchSlopeAngle(false);
    }

    void JumpPressed() => MoveBase.movementStateMachine.NormalMovement();
    void BroomPressed() => MoveBase.movementStateMachine.NormalMovement();
    void SwitchToGlide() => MoveBase.movementStateMachine.GlideMovement();
    void Hover()
    {
        characterMotor.MoveRelativeToGround(Vector3.up * groundRepelForce /
                                            Mathf.Clamp(MoveBase.DistanceToGround(), minDistFromGround, maxDistFromGround));
        var rigid = MoveBase.rigid;
        //characterMotor.RotateToVelocity());
    }

    float GetVelocityFacingSpeed(Rigidbody rigid)
    {
        if (rigid.velocity.magnitude > minSpeedForVelocityFacing)
            return Mathf.Clamp(rigid.velocity.magnitude, minVelocityFacingTurnSpeed, maxVeloctiyFacingTurnSpeed);
        else return 0;
    }

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
