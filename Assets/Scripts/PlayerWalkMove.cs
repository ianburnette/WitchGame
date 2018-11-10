﻿using System.Linq;
using Cinemachine;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerWalkMove : MonoBehaviour, ICloudInteractible {

	[Header("Movement Behavior")]
	[SerializeField] LayerMask groundMask;
	public float movementSpeedOnGround = 70f;
	public float movementSpeedInAir = 18f;
	public float tooFastDecelSpeedOnGround = 7.6f;
	public float tooFastDecelSpeedInAir = 1.1f;
	[Range(0f, 5f)] public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f;

	[Header("Jumping Behavior")]
	[SerializeField] float jumpForce = 13f;
	[SerializeField] float jumpReleaseYVelocity = 0.2f;
	[SerializeField] float extraGravity;
	
	[Header("Jump Leniency")]
	[SerializeField] private bool recentlyGrounded;
	[SerializeField] private bool recentValidSlopeAngle;
	[FormerlySerializedAs("jumpLeniancy")] [SerializeField] float jumpLeniency = 0.17f;

	[Header("Attacking Behavior")]
	[SerializeField] bool attacking;
	[SerializeField] float attackingSpeed;
	
	[Header("Cloud Walking Behavior")]
	[SerializeField] float cloudWalkUpForce;
	[SerializeField] float cloudWalkStopDist;
	[SerializeField] float cloudMomentumMult;
	[SerializeField] float cloudMovementDecel;
	[SerializeField] bool inCloud;

	[Header("Passive Behavior")]
	public float maxSpeed = 9;
	public bool currentlyGrounded;
	public float movingPlatformFriction = 7.7f;
	public float movementSensitivity = .25f;

	[Header("Slope Behavior")]
	[SerializeField] float slopeCorrectionAmount;
	[SerializeField] float slopeMovementCorrectionAmount;
	[SerializeField] float stickToGroundForce;
	[SerializeField] float maxWalkableSlopeAngle = 60, slideForce = 35;
	[SerializeField] float inputToSlopeAngle;
	[SerializeField] AnimationCurve slopeMovementAdjustmentCurve;

	[Header("Audio")]
	[SerializeField] AudioSource audioSource;

	[Header("Class References")]
	[SerializeField] PlayerMoveBase MoveBase;
	[SerializeField] PlayerLadderMove ladderMovement;

	[Header("Debug")]
	[SerializeField] Vector3 inputRelativeToWorld;

	[SerializeField] Vector3 inputRelativeToSlope;
	[SerializeField] float slopeInputToSlopeAngle;
	[SerializeField] float slopeAdjustedMovementSpeed;
	[SerializeField] float animationCurveResult;

	[Header("Animation")]
	[SerializeField] float movementSmoothingSpeed;
	[SerializeField] float smoothedPlanarVelocity;

	[Header("Private Variables")]
	[HideInInspector] public int onEnemyBounce;
	float airPressTime;
	Ladder nearbyLadder;
	Vector2 currentInputVector;
	Vector3 movementDirectionRelativeToCamera, moveDirection, movingObjSpeed;
	ICloudInteractible cloudInteractibleImplementation;
	
	public bool Attacking
	{
		get { return attacking; }
		set { attacking = value; }
	}
	

	void OnEnable() {
		PlayerInput.OnJump += JumpPressed;
		PlayerInput.OnJumpRelease += JumpReleased;
		PlayerInput.OnMove += Move;
		PlayerInput.OnBroom += Broom;

		MoveBase.rigid.drag = 0;
		transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
		MoveBase.animator.SetBool("RidingBroom", false);
	}

	void OnDisable() {
		PlayerInput.OnJump -= JumpPressed;
		PlayerInput.OnMove -= Move;
	}

	void Move(Vector2 inputVector) {
		moveDirection = transform.position +
		                MoveBase.MovementRelativeToCamera(inputVector);

		currentInputVector = inputVector;
	}

	void FixedUpdate() {
		if (InCloud) AccountForCloudWalking();
		CalculateGroundedState();
		UpdatePlayerMovement();
		if (MoveBase.animator) Animate();
	}

	void CalculateGroundedState()
	{
		var tempGroundedState = currentlyGrounded;
		currentlyGrounded = InCloud ? InCloud : MoveBase.IsGrounded(MoveBase.col.bounds.extents.y, groundMask);
		if (!currentlyGrounded && tempGroundedState)
			EnactJumpLeniency();
	}

	private void EnactJumpLeniency()
	{
		recentlyGrounded = true;
		recentValidSlopeAngle = MoveBase.SlopeAngle() < maxWalkableSlopeAngle;
		Invoke(nameof(StopJumpLeniency), jumpLeniency);
	}

	void StopJumpLeniency() => recentlyGrounded = false;

	void UpdatePlayerMovement() {
		inputRelativeToWorld = MoveBase.MovementRelativeToCamera(currentInputVector);
		MoveBase.characterMotor.MoveTo(moveDirection,
		                               GetMoveSpeed(), movementSensitivity,
		                               true);
		if (!inCloud)
			MoveBase.characterMotor.MoveRelativeToGround(StickToGround());

		//if (!currentlyGrounded)
		//	EnactExtraGravity();

		if (rotateSpeed != 0 && MoveBase.MovementRelativeToCamera(currentInputVector).magnitude != 0)
			MoveBase.characterMotor.RotateToVelocity(currentlyGrounded ? rotateSpeed : airRotateSpeed, true);
		MoveBase.characterMotor.ManageSpeed(currentlyGrounded ? tooFastDecelSpeedOnGround : tooFastDecelSpeedInAir,
		                                    maxSpeed + movingObjSpeed.magnitude, false);
	}

	void EnactExtraGravity()
	{
		if (MoveBase.rigid.velocity.y < 0)
			MoveBase.characterMotor.MoveVertical(Vector3.down * extraGravity * Time.deltaTime);
	}

	private float GetMoveSpeed() => currentlyGrounded ? MovementSpeedOnGround() : movementSpeedInAir;

	float MovementSpeedOnGround()
	{
		var slopeUp = SlopeUp();
		Debug.DrawRay(transform.position, slopeUp, Color.cyan);
		inputToSlopeAngle = Vector3.Angle(inputRelativeToWorld, slopeUp);
		
		inputRelativeToSlope = Vector3.ProjectOnPlane(inputRelativeToWorld, MoveBase.slopeNormal);
		Debug.DrawRay(transform.position, inputRelativeToSlope, Color.red);
		//slopeInputToSlopeAngle = Vector3.Angle(inputRelativeToSlope, SlopeCorrection());
		if (inputRelativeToWorld.magnitude != 0 && inputToSlopeAngle < 45) {
			animationCurveResult = slopeMovementAdjustmentCurve.Evaluate(inputToSlopeAngle);
		} else
			animationCurveResult = 1;

		slopeAdjustedMovementSpeed = GetMovementSpeedOnGround() * animationCurveResult;
		return slopeAdjustedMovementSpeed;
	}

	float GetMovementSpeedOnGround() => Attacking ? attackingSpeed : movementSpeedOnGround;
	
	Vector3 SlopeUp()
	{
		return MoveBase.slopeAngle > maxWalkableSlopeAngle
			? SlopeCorrection(slideForce)
			: (SlopeCorrection(slopeCorrectionAmount));
	}

	Vector3 StickToGround() => -MoveBase.slopeNormal * stickToGroundForce;
	Vector3 SlopeCorrection(float force) => Vector3.Cross(MoveBase.slopeNormal, SlopeTangent() * force);
	Vector3 SlopeTangent() => new Vector3(-MoveBase.slopeNormal.z, 0, MoveBase.slopeNormal.x);

	public void AccountForCloudWalking() {
		MoveBase.characterMotor.MoveTo(transform.position + Vector3.up,
		                               cloudWalkUpForce + (MoveBase.RigidbodyXZMagnitude(1) * cloudMomentumMult) * Time.deltaTime,
		                               cloudWalkStopDist,
		                               false);
		MoveBase.characterMotor.ManageSpeed(cloudMovementDecel, maxSpeed, true);
	}
	public void EnterCloud() {
		if (!inCloud)
			CloudDamp();
	}
	public void StayInCloud() => InCloud = true;
	public void ExitCloud() => InCloud = false;
	public bool InCloud {
		get { return inCloud; }
		set {
			if (MoveBase.playerAbilities.cloudWalkingUnlocked) {
				inCloud = value;
			}
		}
	}


	void CloudDamp() => MoveBase.OverrideYVelocity(0);

	void Animate() {
		smoothedPlanarVelocity = Mathf.Lerp(smoothedPlanarVelocity,
		                                    MoveBase.PlanarVelocity().magnitude,
		                                    movementSmoothingSpeed * Time.deltaTime);
		MoveBase.animator.SetFloat("XVelocity", smoothedPlanarVelocity);
		MoveBase.animator.SetBool("Grounded", currentlyGrounded);
		MoveBase.animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
		//MoveBase.animator.SetFloat("XVelocity",
		//                           new Vector3(MoveBase.rigid.velocity.x, 0, MoveBase.rigid.velocity.z).normalized.magnitude +
		//                           .1f);
	}


	private void JumpPressed() {
		if (!EligibleForJummp()) return;
		StopJumpLeniency();
		Jump();
	}

	private bool EligibleForJummp() =>
		(currentlyGrounded && MoveBase.SlopeAngle() < maxWalkableSlopeAngle) ||
		(recentlyGrounded && recentValidSlopeAngle);

	void JumpReleased() {
		if (MoveBase.rigid.velocity.y > jumpReleaseYVelocity)
			MoveBase.OverrideYVelocity(jumpReleaseYVelocity);
	}

	void Jump(Vector3 direction) {
		//if (MoveBase.jumpSound) PlayJumpSound();
		MoveBase.OverrideYVelocity(0);
		MoveBase.rigid.AddRelativeForce(direction, ForceMode.Impulse);
		airPressTime = 0f;
	}

	void Jump() => Jump(new Vector3(0, jumpForce, 0));

	void Broom() {
		if (currentlyGrounded && MoveBase.playerAbilities.hoveringUnlocked)
			MoveBase.movementStateMachine.HoverMovement();
		else if (!currentlyGrounded && MoveBase.playerAbilities.glidingUnlocked)
			MoveBase.movementStateMachine.GlideMovement();
	}

	public void JumpInDirection(Vector3 jumpDirection, float jumpHeightModifier) {
		Jump(new Vector3(jumpDirection.x, jumpForce * jumpHeightModifier, jumpDirection.z));
	}

	public void BounceOnEnemy(Vector3 bounceForce) {
		MoveBase.rigid.velocity = new Vector3(MoveBase.rigid.velocity.x, 0f, MoveBase.rigid.velocity.z);
		MoveBase.rigid.AddRelativeForce(bounceForce, ForceMode.Impulse);
	}

	void PlayJumpSound() {
		audioSource.volume = 1;
		audioSource.clip = MoveBase.jumpSound;
		audioSource.Play ();
	}
}
