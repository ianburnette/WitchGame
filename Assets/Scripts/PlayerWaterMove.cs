using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaterMove : MonoBehaviour {

	[Header("Movement Behavior")] [SerializeField]
	float swimMovementSpeed = 70f;
	[SerializeField] float rotateSpeed;
	[SerializeField] float tooFastDecelSpeed = 7.6f;
	[SerializeField] private float movementSmoothingSpeed = 6; 

	[Header("Jumping")] [SerializeField] float jumpForce = 13;

	[Header("Passive Behavior")]
	[SerializeField] float maxSpeed = 9;
	[SerializeField] float movementSensitivity = .25f;
	[SerializeField] float jumpCooldown = 1f;

	[Header("Class References")] [SerializeField]
	PlayerMoveBase MoveBase;

	[Header("Private Variables")]
	public Vector2 currentInputVector;
	public Vector3 moveDirection;
	float lastJumpTime;
	private float smoothedPlanarVelocity;
	private static readonly int InWater = Animator.StringToHash("InWater");

	void OnEnable() {
		PlayerInput.OnJump += JumpPressed;
		PlayerInput.OnMove += Move;
		MoveBase.animator.SetBool(InWater, true);
	}

	void OnDisable() {
		PlayerInput.OnJump -= JumpPressed;
		PlayerInput.OnMove -= Move;
		MoveBase.animator.SetBool(InWater, false);
	}

	void Move(Vector2 inputVector) {
		moveDirection = transform.position +
		                MoveBase.MovementRelativeToCamera(inputVector);
		currentInputVector = inputVector;
	}

	void FixedUpdate() {
		UpdatePlayerMovement();
		Animate();
	}

	private void Animate()
	{
		smoothedPlanarVelocity = Mathf.Lerp(smoothedPlanarVelocity,
											MoveBase.PlanarVelocity().magnitude,
											movementSmoothingSpeed * Time.deltaTime);
		MoveBase.animator.SetFloat("XVelocity", smoothedPlanarVelocity);
	}

	void UpdatePlayerMovement() {
		MoveBase.characterMotor.MoveTo(moveDirection, swimMovementSpeed, movementSensitivity, true);
		if (rotateSpeed != 0 && MoveBase.MovementRelativeToCamera(currentInputVector).magnitude != 0)
			MoveBase.characterMotor.RotateToVelocity(rotateSpeed, true);
		MoveBase.characterMotor.ManageSpeed(tooFastDecelSpeed, maxSpeed, false);
	}

	void JumpPressed() {
		if (Time.time - lastJumpTime > jumpCooldown)
			Jump();
	}

	void Jump() {
		lastJumpTime = Time.time;
		MoveBase.rigid.velocity = new Vector3(MoveBase.rigid.velocity.x,
		                                      0f,
		                                      MoveBase.rigid.velocity.z);
		MoveBase.rigid.AddRelativeForce(MoveBase.rigid.velocity.x,
		                                jumpForce,
		                                MoveBase.rigid.velocity.z,
		                                ForceMode.Impulse);
	}
}
