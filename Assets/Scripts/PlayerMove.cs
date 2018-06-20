﻿using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour {
	[SerializeField] PlayerMoveBase MoveBase;

	//movement
	public float accel = 70f;
	public float airAccel = 18f;
	public float decel = 7.6f;
	public float airDecel = 1.1f;
	[Range(0f, 5f)] public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f;
	public float maximumMovementMagnitude = 9;
	public float slopeLimit = 60, slideAmount = 35;
	public float
		movingPlatformFriction = 7.7f; //you'll need to tweak this to get the player to stay on moving platforms properly
	public float movementSensitivity = .25f;
	[SerializeField] float slopeCorrectionAmount;
	[SerializeField] float stickToGroundForce;

	//jumping
	[SerializeField] float jumpForce = 13f;
	[SerializeField] float jumpLeniancy = 0.17f;
	[HideInInspector] public int onEnemyBounce;
	bool grounded;
	float airPressTime;
	Vector3 movementDirectionRelativeToCamera, moveDirection, movingObjSpeed;
	[SerializeField] protected Vector3 specificSlopeNormal;
	[SerializeField] CharacterMotor characterMotor;
	[SerializeField] AudioSource aSource;

	void OnEnable() {
		PlayerInput.OnJump += JumpPressed;
		PlayerInput.OnMove += Move;
	}

	void OnDisable() {
		PlayerInput.OnJump -= JumpPressed;
		PlayerInput.OnMove -= Move;
	}

	void Move(Vector2 inputVector) {
		moveDirection = transform.position + MoveBase.MovementRelativeToCamera(inputVector);
		characterMotor.MoveTo(moveDirection, grounded ? accel : airAccel, movementSensitivity, true);
		if (rotateSpeed != 0 && MoveBase.MovementRelativeToCamera(inputVector).magnitude != 0)
			characterMotor.RotateToVelocity(grounded ? rotateSpeed : airRotateSpeed, true);
		characterMotor.ManageSpeed(grounded ? decel : airDecel, maximumMovementMagnitude + movingObjSpeed.magnitude, true);
	}

	void FixedUpdate() {
		grounded = MoveBase.IsGrounded();
		PlayLandingSoundIfNecessary();
		MoveBase.rigid.AddForce(SlopeCorrection() + StickToGround());
		if (MoveBase.animator) Animate();
	}

	void Animate() {
		MoveBase.animator.SetFloat("DistanceToTarget", characterMotor.DistanceToTarget);
		MoveBase.animator.SetBool("Grounded", grounded);
		MoveBase.animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
		MoveBase.animator.SetFloat("XVelocity",
		                           new Vector3(MoveBase.rigid.velocity.x, 0, MoveBase.rigid.velocity.z).normalized.magnitude +
		                           .1f);
	}

	void PlayLandingSoundIfNecessary() {
		if (aSource.isPlaying || !MoveBase.landSound || !(MoveBase.rigid.velocity.y < 1)) return;
		aSource.volume = Mathf.Abs(MoveBase.rigid.velocity.y) / 40;
		aSource.clip = MoveBase.landSound;
		aSource.Play();
	}

	Vector3 StickToGround() => -MoveBase.slopeNormal * stickToGroundForce;
	Vector3 SlopeCorrection() => Vector3.Cross(MoveBase.slopeNormal, SlopeTangent() * slopeCorrectionAmount);
	Vector3 SlopeTangent() => new Vector3(-MoveBase.slopeNormal.z, 0, MoveBase.slopeNormal.x);

	void JumpPressed() {
		if (!grounded)
			airPressTime = Time.time;
		//TODO make sure to put jump leniancy back in

		else if (MoveBase.SlopeAngle() < slopeLimit)
			Jump();
		//movementStateMachine.ChangeState(this);
		//TODO figure out how to put the state change in here
	}

	void Jump() {
		if (MoveBase.jumpSound) PlayJumpSound();
		MoveBase.rigid.velocity = new Vector3(MoveBase.rigid.velocity.x, 0f, MoveBase.rigid.velocity.z);
		MoveBase.rigid.AddRelativeForce(Vector3.up * jumpForce, ForceMode.Impulse);
		airPressTime = 0f;
	}

	public void BounceOnEnemy(Vector3 bounceForce) {
		MoveBase.rigid.velocity = new Vector3(MoveBase.rigid.velocity.x, 0f, MoveBase.rigid.velocity.z);
		MoveBase.rigid.AddRelativeForce(bounceForce, ForceMode.Impulse);
	}

	void PlayJumpSound() {
		aSource.volume = 1;
		aSource.clip = MoveBase.jumpSound;
		aSource.Play ();
	}
}
