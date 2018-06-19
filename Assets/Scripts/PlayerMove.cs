using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(DealDamage))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
	public Transform mainCam, floorChecks;
	public Animator animator;
	public AudioClip jumpSound;
	public AudioClip landSound;
	MovementStateMachine movementStateMachine;

	//movement
	public float accel = 70f;
	public float airAccel = 18f;
	public float decel = 7.6f;
	public float airDecel = 1.1f;
	[Range(0f, 5f)]
	public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f;
	public float maximumMovementMagnitude = 9;
	public float slopeLimit = 60, slideAmount = 35;
	public float movingPlatformFriction = 7.7f;				//you'll need to tweak this to get the player to stay on moving platforms properly

	public float movementSensitivity = .25f;

	[SerializeField] float slopeCorrectionAmount;
	[SerializeField] float stickToGroundForce;

	//jumping
	public float jumpForce = 13f;
	public float jumpLeniancy = 0.17f;
	[HideInInspector] public int onEnemyBounce;

	int onJump;
	bool grounded;
	[SerializeField] Transform[] floorCheckers;
	Quaternion screenMovementSpace;
	float airPressTime, slope;
	Vector3 movementDirectionRelativeToCamera, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;
	[SerializeField] Vector3 slopeNormal;
	[SerializeField] protected Vector3 specificSlopeNormal;
	[SerializeField] LayerMask groundMask;

	[SerializeField] CharacterMotor characterMotor;
	[SerializeField] DealDamage dealDamage;
	[SerializeField] Rigidbody rigid;
	[SerializeField] AudioSource aSource;

	EnemyAI currentEnemyAI;

	void OnEnable() {
		PlayerInput.OnJump += JumpPressed;
		PlayerInput.OnMove += Move;
	}

	void OnDisable() {
		PlayerInput.OnJump -= JumpPressed;
		PlayerInput.OnMove -= Move;
	}

	void Update() => rigid.WakeUp();

	void Move(Vector2 inputVector) {
		MovementRelativeToCamera();
		movementDirectionRelativeToCamera = (screenMovementForward * inputVector.y) +
		                                    (screenMovementRight * inputVector.x);
		moveDirection = transform.position + movementDirectionRelativeToCamera;
	}

	void MovementRelativeToCamera() {
		screenMovementSpace = Quaternion.Euler (0, mainCam.eulerAngles.y, 0);
		screenMovementForward = screenMovementSpace * Vector3.forward;
		screenMovementRight = screenMovementSpace * Vector3.right;
	}

	//apply correct player movement (fixedUpdate for physics calculations)
	void FixedUpdate()
	{
		grounded = IsGrounded ();
		PlayLandingSoundIfNecessary();

		rigid.AddForce(SlopeCorrection() + StickToGround());
		UpdateCharacterMotor();

		if (animator) Animate();
	}

	void UpdateCharacterMotor() {
		characterMotor.MoveTo (moveDirection, grounded ? accel : airAccel, movementSensitivity, true);
		if (rotateSpeed != 0 && movementDirectionRelativeToCamera.magnitude != 0)
			characterMotor.RotateToVelocity(grounded ? rotateSpeed : airRotateSpeed, true);
		characterMotor.ManageSpeed (grounded ? decel : airDecel, maximumMovementMagnitude + movingObjSpeed.magnitude, true);
	}

	void Animate() {
		animator.SetFloat("DistanceToTarget", characterMotor.DistanceToTarget);
		animator.SetBool("Grounded", grounded);
		animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
		animator.SetFloat("XVelocity", new Vector3(rigid.velocity.x,0,rigid.velocity.z).normalized.magnitude + .1f);
	}

	void PlayLandingSoundIfNecessary() {
		if (aSource.isPlaying || !landSound || !(rigid.velocity.y < 1)) return;
		aSource.volume = Mathf.Abs(rigid.velocity.y) / 40;
		aSource.clip = landSound;
		aSource.Play();
	}

	bool IsGrounded()
	{
		//get distance to ground, from centre of collider (where floorcheckers should be)
		float dist = GetComponent<Collider>().bounds.extents.y;
        //check whats at players feet, at each floorcheckers position
        int connectingRays = 0;
        Vector3 summedNormal = Vector3.zero;
		foreach (Transform check in floorCheckers)
		{
			RaycastHit hit;
			if(Physics.Raycast(check.position, Vector3.down * 1, out hit, dist + 0.05f, groundMask))
			{
				summedNormal += hit.normal.normalized; ;
				slope = Vector3.Angle (hit.normal, Vector3.up);
				//slide down slopes
				if(slope > slopeLimit && !hit.transform.CompareTag("Pushable"))
					rigid.AddForce (new Vector3(0f, -slideAmount, 0f), ForceMode.Force);
				//enemy bouncing
				if (hit.transform.CompareTag("Enemy") && rigid.velocity.y < 0)
				{
					currentEnemyAI = hit.transform.GetComponent<EnemyAI>();
					currentEnemyAI.BouncedOn();
					onEnemyBounce ++;
					dealDamage.Attack(hit.transform.gameObject, 1, 0f, 0f);
				}
				else
					onEnemyBounce = 0;
				//moving platforms
				if (hit.transform.CompareTag("MovingPlatform") || hit.transform.CompareTag("Pushable")) {
					movingObjSpeed = hit.transform.GetComponent<Rigidbody>().velocity;
					movingObjSpeed.y = 0f;
					//9.5f is a magic number, if youre not moving properly on platforms, experiment with this number
					rigid.AddForce(movingObjSpeed * movingPlatformFriction * Time.fixedDeltaTime, ForceMode.VelocityChange);
				} else
					movingObjSpeed = Vector3.zero;

			   connectingRays++;
			}
		}

		slopeNormal = CalculateSlopeNormal(summedNormal, connectingRays);

        movingObjSpeed = Vector3.zero;

        return connectingRays != 0;
	}

	static Vector3 CalculateSlopeNormal(Vector3 totalNormal, int rayCount) =>
		new Vector3(Mathf.Abs(totalNormal.x) > 0 ? totalNormal.x / rayCount : 0,
					Mathf.Abs(totalNormal.y) > 0 ? totalNormal.y / rayCount : 0,
					Mathf.Abs(totalNormal.z) > 0 ? totalNormal.z / rayCount : 0);
	Vector3 StickToGround() => -slopeNormal * stickToGroundForce;
	Vector3 SlopeCorrection() => Vector3.Cross(slopeNormal, SlopeTangent() * slopeCorrectionAmount);
	Vector3 SlopeTangent() => new Vector3(-slopeNormal.z, 0, slopeNormal.x);

	void JumpPressed() {
		if (!grounded)
			airPressTime = Time.time;

		else if (slope < slopeLimit)
			Jump();
		//movementStateMachine.ChangeState(this);
		//TODO figure out how to put the state change in here
	}

	void Jump() {
		if (jumpSound) PlayJumpSound();
		rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
		rigid.AddRelativeForce (Vector3.up * jumpForce, ForceMode.Impulse);
		airPressTime = 0f;
	}

	public void BounceOnEnemy(Vector3 bounceForce) {
		rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
		rigid.AddRelativeForce (bounceForce, ForceMode.Impulse);
	}

	void PlayJumpSound() {
		aSource.volume = 1;
		aSource.clip = jumpSound;
		aSource.Play ();
	}
}
