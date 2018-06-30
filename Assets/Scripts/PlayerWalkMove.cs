using System.Linq;
using System.Runtime.Remoting.Messaging;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerWalkMove : MonoBehaviour, ICloudInteractible {

	[Header("Movement Behavior")]
	public float movementSpeedOnGround = 70f;
	public float movementSpeedInAir = 18f;
	public float tooFastDecelSpeedOnGround = 7.6f;
	public float tooFastDecelSpeedInAir = 1.1f;
	[Range(0f, 5f)] public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f;

	[Header("Jumping Behavior")]
	[SerializeField] float jumpForce = 13f;
	[SerializeField] float jumpLeniancy = 0.17f;
	[SerializeField] float jumpReleaseYVelocity = 0.2f;

	[Header("Cloud Walking Behavior")]
	[SerializeField] float cloudWalkUpForce;
	[SerializeField] float cloudWalkStopDist;
	[SerializeField] float cloudMomentumMult;
	[SerializeField] float cloudMovementDecel;
	[SerializeField] bool inCloud;

	[Header("Passive Behavior")]
	public float maxSpeed = 9;

	public float movingPlatformFriction = 7.7f;
	public float movementSensitivity = .25f;

	[Header("Slope Behavior")]
	[SerializeField] float slopeCorrectionAmount;
	[SerializeField] float stickToGroundForce;
	public float maxWalkableSlopeAngle = 60, slideForce = 35;

	[Header("Audio")]
	[SerializeField] AudioSource audioSource;

	[Header("Class References")]
	[SerializeField] PlayerMoveBase MoveBase;
	[SerializeField] PlayerLadderMove ladderMovement;

	[Header("Private Variables")]
	[HideInInspector] public int onEnemyBounce;
	float airPressTime;
	Ladder nearbyLadder;
	Vector2 currentInputVector;
	Vector3 movementDirectionRelativeToCamera, moveDirection, movingObjSpeed;
	ICloudInteractible cloudInteractibleImplementation;

	void OnEnable() {
		PlayerInput.OnJump += JumpPressed;
		PlayerInput.OnJumpRelease += JumpReleased;
		PlayerInput.OnMove += Move;
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
		PlayLandingSoundIfNecessary();
		if (InCloud) AccountForCloudWalking();
		UpdatePlayerMovement();
		if (MoveBase.animator) Animate();
	}

	public bool Grounded() => InCloud ? InCloud : MoveBase.IsGrounded(MoveBase.col.bounds.extents.y);

	void UpdatePlayerMovement() {
		MoveBase.characterMotor.MoveTo(moveDirection + (inCloud ? Vector3.zero : SlopeCorrection()),
		                               Grounded() ? movementSpeedOnGround : movementSpeedInAir, movementSensitivity,
		                               true);
		if (rotateSpeed != 0 && MoveBase.MovementRelativeToCamera(currentInputVector).magnitude != 0)
			MoveBase.characterMotor.RotateToVelocity(Grounded() ? rotateSpeed : airRotateSpeed, true);
		MoveBase.characterMotor.ManageSpeed(Grounded() ? tooFastDecelSpeedOnGround : tooFastDecelSpeedInAir,
		                                    maxSpeed + movingObjSpeed.magnitude, false);
	}

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
			if (MoveBase.movementStateMachine.cloudWalkingUnlocked) {
				inCloud = value;
			}
		}
	}
	void CloudDamp() => MoveBase.OverrideYVelocity(0);

	void Animate() {
		MoveBase.animator.SetFloat("DistanceToTarget", MoveBase.characterMotor.DistanceToTarget);
		MoveBase.animator.SetBool("Grounded", Grounded());
		MoveBase.animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
		MoveBase.animator.SetFloat("XVelocity",
		                           new Vector3(MoveBase.rigid.velocity.x, 0, MoveBase.rigid.velocity.z).normalized.magnitude +
		                           .1f);
	}

	void PlayLandingSoundIfNecessary() {
		if (audioSource.isPlaying || !MoveBase.landSound || !(MoveBase.rigid.velocity.y < 1)) return;
		audioSource.volume = Mathf.Abs(MoveBase.rigid.velocity.y) / 40;
		audioSource.clip = MoveBase.landSound;
		audioSource.Play();
	}

	Vector3 SlopeCorrection() {
		if (MoveBase.slopeAngle > maxWalkableSlopeAngle)
			return SlopeCorrection(slideForce); //MoveBase.characterMotor.MoveRelativeToGround(SlopeCorrection(slideForce));
		return SlopeCorrection(slopeCorrectionAmount) + StickToGround();
		//MoveBase.characterMotor.MoveRelativeToGround(SlopeCorrection(slopeCorrectionAmount) + StickToGround());
	}

	Vector3 StickToGround() => -MoveBase.slopeNormal * stickToGroundForce;
	Vector3 SlopeCorrection(float force) => Vector3.Cross(MoveBase.slopeNormal, SlopeTangent() * force);
	Vector3 SlopeTangent() => new Vector3(-MoveBase.slopeNormal.z, 0, MoveBase.slopeNormal.x);

	void JumpPressed() {
		if (!Grounded()) {
			//TODO make sure to put jump leniancy back in			//	airPressTime = Time.time;
			if (MoveBase.movementStateMachine.glidingUnlocked)
				MoveBase.movementStateMachine.GlideMovement();
			else if (MoveBase.movementStateMachine.hoveringUnlocked)
				MoveBase.movementStateMachine.HoverMovement();
		} else if (MoveBase.SlopeAngle() < maxWalkableSlopeAngle)
			Jump();
	}

	void JumpReleased() {
		if (MoveBase.rigid.velocity.y > jumpReleaseYVelocity)
			MoveBase.OverrideYVelocity(jumpReleaseYVelocity);
	}

	void Jump(Vector3 direction) {
		if (MoveBase.jumpSound) PlayJumpSound();
		MoveBase.OverrideYVelocity(0);//MoveBase.rigid.velocity = new Vector3(MoveBase.rigid.velocity.x, 0f, MoveBase.rigid.velocity.z);
		MoveBase.rigid.AddRelativeForce(direction, ForceMode.Impulse);
		airPressTime = 0f;
	}

	void Jump() => Jump(new Vector3(0, jumpForce, 0));

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
