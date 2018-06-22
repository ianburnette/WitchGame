using System.Linq;
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
	[SerializeField] AudioSource aSource;
	Vector2 currentMovementVector;

	[SerializeField] bool canGrabLadder;
	[SerializeField] PlayerLadderMove ladderMovement;
	Ladder nearbyLadder;

	void OnEnable() {
		PlayerInput.OnJump += JumpPressed;
		PlayerInput.OnGrab += GrabPressed;
		PlayerInput.OnMove += Move;
		MoveBase.rigid.drag = 0;
		transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
	}

	void OnDisable() {
		PlayerInput.OnJump -= JumpPressed;
		PlayerInput.OnGrab -= GrabPressed;
		PlayerInput.OnMove -= Move;
	}

	void GrabPressed() {
		MoveBase.movementStateMachine.GetOnLadder();
	}

	public void ToggleLadder(bool state, Ladder ladder = null) {
		canGrabLadder  = state;
		ladderMovement.ladder = ladder;
	}

	void Move(Vector2 inputVector) {
		moveDirection = transform.position + MoveBase.MovementRelativeToCamera(inputVector);
		currentMovementVector = inputVector;
	}

	void FixedUpdate() {
		grounded = MoveBase.IsGrounded(MoveBase.col.bounds.extents.y);
		PlayLandingSoundIfNecessary();
		UpdatePlayerMovement();
		if (MoveBase.animator) Animate();
	}

	void UpdatePlayerMovement() {
		MoveBase.characterMotor.MoveTo(moveDirection, grounded ? accel : airAccel, movementSensitivity, true);
		MoveBase.characterMotor.MoveRelativeToGround(SlopeCorrection()+StickToGround());
		if (rotateSpeed != 0 && MoveBase.MovementRelativeToCamera(currentMovementVector).magnitude != 0)
			MoveBase.characterMotor.RotateToVelocity(grounded ? rotateSpeed : airRotateSpeed, true);
		MoveBase.characterMotor.ManageSpeed(grounded ? decel : airDecel, maximumMovementMagnitude + movingObjSpeed.magnitude, false);

	}

	void Animate() {
		MoveBase.animator.SetFloat("DistanceToTarget", MoveBase.characterMotor.DistanceToTarget);
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
		//	airPressTime = Time.time;
		//TODO make sure to put jump leniancy back in
			MoveBase.movementStateMachine.OnBroom();

		else if (MoveBase.SlopeAngle() < slopeLimit)
			Jump();

		//TODO figure out how to put the state change in here
	}

	void Jump(Vector3 direction) {
		if (MoveBase.jumpSound) PlayJumpSound();
		MoveBase.rigid.velocity = new Vector3(MoveBase.rigid.velocity.x, 0f, MoveBase.rigid.velocity.z);
		MoveBase.rigid.AddRelativeForce(direction, ForceMode.Impulse);
		airPressTime = 0f;
	}

	void Jump() => Jump(new Vector3(MoveBase.rigid.velocity.x, jumpForce, MoveBase.rigid.velocity.z));

	public void JumpInDirection(Vector3 jumpDirection, float jumpHeightModifier) {
		Jump(new Vector3(jumpDirection.x, jumpForce * jumpHeightModifier, jumpDirection.z));
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
