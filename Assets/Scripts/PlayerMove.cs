using System.Linq;
using JetBrains.Annotations;
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
	[SerializeField] GroundHitInfo[] groundInfo;
	Quaternion screenMovementSpace;
	float airPressTime, slope;
	Vector3 movementDirectionRelativeToCamera, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;
	[SerializeField] Vector3 slopeNormal;
	[SerializeField] protected Vector3 specificSlopeNormal;
	[SerializeField] LayerMask groundMask;
	[SerializeField] float groundCheckOffset = .05f;

	[SerializeField] CharacterMotor characterMotor;
	[SerializeField] DealDamage dealDamage;
	[SerializeField] Rigidbody rigid;
	[SerializeField] AudioSource aSource;

	EnemyAI currentEnemyAI;
	Collider col;

	void OnEnable() {
		PlayerInput.OnJump += JumpPressed;
		PlayerInput.OnMove += Move;
		groundInfo = new GroundHitInfo[floorCheckers.Length];
		col = GetComponent<Collider>();
	}

	void OnDisable() {
		PlayerInput.OnJump -= JumpPressed;
		PlayerInput.OnMove -= Move;
	}

	void Update() => rigid.WakeUp();

	void Move(Vector2 inputVector) {
		Vector3 movementDir = MovementRelativeToCamera(inputVector);
		UpdateCharacterMotor(transform.position + movementDir, movementDir.magnitude);
	}

	Vector3 MovementRelativeToCamera(Vector2 input) {
		screenMovementSpace = Quaternion.Euler (0, mainCam.eulerAngles.y, 0);
		screenMovementForward = screenMovementSpace * Vector3.forward;
		screenMovementRight = screenMovementSpace * Vector3.right;
		return (screenMovementForward * input.y) + (screenMovementRight * input.x);
	}

	void FixedUpdate()
	{
		grounded = IsGrounded ();
		PlayLandingSoundIfNecessary();

		rigid.AddForce(SlopeCorrection() + StickToGround());


		if (animator) Animate();
	}

	void UpdateCharacterMotor(Vector3 moveDirection, float magnitude) {
		characterMotor.MoveTo (moveDirection, grounded ? accel : airAccel, movementSensitivity, true);
		if (rotateSpeed != 0 && magnitude != 0)
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

	[CanBeNull] GroundHitInfo GetGroundHitInfo(Transform checker) {
		RaycastHit hit;
		var distanceToCheck = col.bounds.extents.y;
		return Physics.Raycast(checker.position, Vector3.down * 1, out hit, distanceToCheck + groundCheckOffset, groundMask) ?
			       new GroundHitInfo(hit.point, hit.normal, hit.transform, hit.transform.GetComponent<Rigidbody>()) :
			       null;
	}

	[CanBeNull] GroundHitInfo EnemyBounceHit() =>
		groundInfo.Where(info => info != null).FirstOrDefault(info => info.transform.CompareTag("Enemy"));


	int PointsOfContact() {
		return groundInfo.Count(t => t != null);
	}

	Vector3 AverageContactNormal() {
		var summedNormal = groundInfo.Where(t => t != null).Aggregate(Vector3.zero, (current, t) => current + t.normal);
		return CalculateSlopeNormal(summedNormal, PointsOfContact());
	}

	bool IsGrounded()
	{
		for (var i = 0; i < floorCheckers.Length; i++)
			groundInfo[i] = GetGroundHitInfo(floorCheckers[i]);
		if (EnemyBounceHit() != null) {
			var enemyTransform = EnemyBounceHit().transform;
			currentEnemyAI = enemyTransform.GetComponent<EnemyAI>();
			currentEnemyAI.BouncedOn();
			dealDamage.Attack(enemyTransform.gameObject, 1, 0f, 0f);
		}
		slopeNormal = AverageContactNormal();
		return PointsOfContact() != 0;
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

public class GroundHitInfo {
	public Vector3 position;
	public Vector3 normal;
	public Transform transform;
	public Rigidbody rigid;

	public GroundHitInfo(Vector3 pos, Vector3 norm, Transform tran, [CanBeNull] Rigidbody rb) {
		position = pos;
		normal = norm;
		transform = tran;
		rigid = rb;
	}
}
