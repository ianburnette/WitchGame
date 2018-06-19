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
	public Vector3 jumpForce =  new Vector3(0, 13, 0);
	public float jumpLeniancy = 0.17f;
	[HideInInspector] public int onEnemyBounce;

	int onJump;
	bool grounded;
	Transform[] floorCheckers;
	Quaternion screenMovementSpace;
	float airPressTime, curAccel, curDecel, curRotateSpeed, slope;
	Vector3 direction, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;
	[SerializeField] Vector3 slopeNormal;
	[SerializeField] protected Vector3 specificSlopeNormal;
	[SerializeField] LayerMask groundMask;

	CharacterMotor characterMotor;
	EnemyAI enemyAI;
	DealDamage dealDamage;
	Rigidbody rigid;
	AudioSource aSource;

	//setup
	void Awake()
	{
		//usual setup
		dealDamage = GetComponent<DealDamage>();
		characterMotor = GetComponent<CharacterMotor>();
		rigid = GetComponent<Rigidbody>();
		aSource = GetComponent<AudioSource>();

		//gets child objects of floorcheckers, and puts them in an array
		//later these are used to raycast downward and see if we are on the ground
		floorCheckers = new Transform[floorChecks.childCount];
		for (int i=0; i < floorCheckers.Length; i++)
			floorCheckers[i] = floorChecks.GetChild(i);
	}

	//get state of player, values and input
	void Update()
	{
		//stops rigidbody "sleeping" if we don't move, which would stop collision detection
		rigid.WakeUp();
		//handle jumping
		JumpCalculations ();
		//adjust movement values if we're in the air or on the ground
		curAccel = (grounded) ? accel : airAccel;
		curDecel = (grounded) ? decel : airDecel;
		curRotateSpeed = (grounded) ? rotateSpeed : airRotateSpeed;

		//get movement axis relative to camera
		screenMovementSpace = Quaternion.Euler (0, mainCam.eulerAngles.y, 0);
		screenMovementForward = screenMovementSpace * Vector3.forward;
		screenMovementRight = screenMovementSpace * Vector3.right;

		//get movement input, set direction to move in
		var h = Input.GetAxis ("Horizontal");
		var v = Input.GetAxis ("Vertical");

		direction = (screenMovementForward * v) + (screenMovementRight * h);
		moveDirection = transform.position + direction;
	}

	//apply correct player movement (fixedUpdate for physics calculations)
	void FixedUpdate()
	{
		//are we grounded
		grounded = IsGrounded ();
		CorrectForSlope();
		StickToGround();

		characterMotor.MoveTo (moveDirection, curAccel, movementSensitivity, true);
		Debug.DrawRay(transform.position, transform.position+moveDirection, Color.magenta);
		if (rotateSpeed != 0 && direction.magnitude != 0)
			characterMotor.RotateToVelocity(curRotateSpeed, true);//RotateToDirection (moveDirection , curRotateSpeed, true);
		characterMotor.ManageSpeed (curDecel, maximumMovementMagnitude + movingObjSpeed.magnitude, true);
		//set animation values
		if(animator)
		{
			animator.SetFloat("DistanceToTarget", characterMotor.DistanceToTarget);
			animator.SetBool("Grounded", grounded);
			animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
			animator.SetFloat("XVelocity", new Vector3(rigid.velocity.x,0,rigid.velocity.z).normalized.magnitude + .1f);
		}
	}

	private bool IsGrounded()
	{
		//get distance to ground, from centre of collider (where floorcheckers should be)
		float dist = GetComponent<Collider>().bounds.extents.y;
        //check whats at players feet, at each floorcheckers position
        int connectingRays = 0;
        Vector3 totalNormal = Vector3.zero;
		foreach (Transform check in floorCheckers)
		{
			RaycastHit hit;
			if(Physics.Raycast(check.position, Vector3.down * 1, out hit, dist + 0.05f, groundMask))
			{
				totalNormal += hit.normal.normalized; ;
				slope = Vector3.Angle (hit.normal, Vector3.up);
				//slide down slopes
				if(slope > slopeLimit && !hit.transform.CompareTag("Pushable"))
					rigid.AddForce (new Vector3(0f, -slideAmount, 0f), ForceMode.Force);
				//enemy bouncing
				if (hit.transform.CompareTag("Enemy") && rigid.velocity.y < 0)
				{
					enemyAI = hit.transform.GetComponent<EnemyAI>();
					enemyAI.BouncedOn();
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

        float nX = Mathf.Abs(totalNormal.x) > 0 ? totalNormal.x / connectingRays : 0;
        float nY = Mathf.Abs(totalNormal.y) > 0 ? totalNormal.y / connectingRays : 0;
        float nZ = Mathf.Abs(totalNormal.z) > 0 ? totalNormal.z / connectingRays : 0;

        slopeNormal = new Vector3(nX, nY, nZ);

        movingObjSpeed = Vector3.zero;

        if (connectingRays == 0)
            return false;
        else
           return true;

	}

	void CorrectForSlope()
	{
		Debug.DrawRay(transform.position, SlopeCorrection() * slopeCorrectionAmount, Color.red);
		rigid.AddForce(SlopeCorrection() * slopeCorrectionAmount);
	}

	void StickToGround() {
		Debug.DrawRay(transform.position, (slopeNormal * stickToGroundForce), Color.cyan);
		rigid.AddForce(-slopeNormal * stickToGroundForce);
	}

	Vector3 SlopeCorrection() => Vector3.Cross(slopeNormal, SlopeTangent());

	Vector3 SlopeTangent() =>
		new Vector3(-slopeNormal.z, 0, slopeNormal.x);

	private void JumpCalculations() {
		if (!GetComponent<AudioSource>().isPlaying && landSound && rigid.velocity.y < 1) {
			aSource.volume = Mathf.Abs(GetComponent<Rigidbody>().velocity.y) / 40;
			aSource.clip = landSound;
			aSource.Play();
		}

		//if we press jump in the air, save the time
		if (Input.GetButtonDown("Jump") && !grounded)
			airPressTime = Time.time;

		//if were on ground within slope limit
		if (grounded && slope < slopeLimit) {
			//and we press jump, or we pressed jump justt before hitting the ground
			if (Input.GetButtonDown("Jump") || airPressTime + jumpLeniancy > Time.time)
				Jump(jumpForce);
		} else {
			//	movementStateMachine.ChangeState(this);
		}
	}

	public void Jump(Vector3 jumpVelocity)
	{
		if(jumpSound)
		{
			aSource.volume = 1;
			aSource.clip = jumpSound;
			aSource.Play ();
		}
		rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
		rigid.AddRelativeForce (jumpVelocity, ForceMode.Impulse);
		airPressTime = 0f;
	}
}
