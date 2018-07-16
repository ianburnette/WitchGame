using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnderwaterMove : MonoBehaviour {

	[Header("Movement Behavior")]
	[SerializeField] LayerMask groundMask;

	[SerializeField] float movementSpeed;
	[SerializeField] float groundRepelForce;
	[Range(.01f, 1)] [SerializeField] float movementSensitivity;
	[SerializeField] float maxSpeed;
	[SerializeField] float rotateSpeed;

	[Header("Flight Behavior")]
	[SerializeField] bool ascending;
	[SerializeField] bool descending;
	[SerializeField] float ascendSpeed, descendSpeed;

	[Header("Passive Behavior")]
	[SerializeField] float tooFastDecelSpeed;
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
		PlayerInput.OnJumpRelease += JumpReleased;
		PlayerInput.OnMove += Move;
		PlayerInput.OnBroom += BroomPressed;
		PlayerInput.OnAttack += AttackPressed;
		PlayerInput.OnAttackRelease += AttackReleased;

		MoveBase.animator.SetBool("RidingBroom", true);
	}

	void OnDisable() {
		PlayerInput.OnJump -= JumpPressed;
		PlayerInput.OnMove -= Move;
		PlayerInput.OnBroom -= BroomPressed;
	}

	void JumpPressed() => ascending = true;
	void JumpReleased() => @ascending = false;
	void AttackPressed() => @descending = true;
	void AttackReleased() => @descending = false;

	void BroomPressed() => MoveBase.movementStateMachine.WaterMovement();

	void Move(Vector2 movement) => characterMotor.MoveTo(MoveBase.MovementRelativeToPlayerAndCamera(movement),
	                                                     movementSpeed,
	                                                     movementSensitivity,
	                                                     ignoreY:true);

	void FixedUpdate() {
		if (MoveBase.IsGrounded(heightBelowWhichToApplyHoverForce, groundMask))
			Hover();
		characterMotor.RotateToVelocity(rotateSpeed, minYrotation, maxYrotation, yMultiplier);
		characterMotor.ManageSpeed(tooFastDecelSpeed, maxSpeed, false);
		FlightMovement();
	}

	void FlightMovement() {
		if (AscendingOrDescending())
			characterMotor.MoveRelativeToGround(Vector3.up * FlightForce());
	}

	float FlightForce() {
		if (@ascending)
			return ascendSpeed;
		if (@descending)
			return descendSpeed;
		return 0;
	}

	bool AscendingOrDescending() => (@ascending && !@descending) || (@descending && !@ascending);

	void Hover() {
		characterMotor.MoveRelativeToGround(Vector3.up * groundRepelForce /
		                                    (MoveBase.DistanceToGround() * distanceFromGroundForceMult));

	}
}
