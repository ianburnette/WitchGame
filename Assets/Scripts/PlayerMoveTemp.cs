using UnityEngine;
using System.Collections;

//handles player movement, utilising the CharacterMotor class
[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(DealDamage))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMoveTemp : MonoBehaviour
{
	//setup
	public Transform mainCam, floorChecks;		//main camera, and floorChecks object. FloorChecks are raycasted down from to check the player is grounded.
	public Animator animator;			//object with animation controller on, which you want to animate
	public AudioClip jumpSound;					//play when jumping
	public AudioClip landSound;					//play when landing on ground

	//movement
	public float accel = 70f;					//acceleration/deceleration in air or on the ground
	public float airAccel = 18f;
	public float decel = 7.6f;
	public float airDecel = 1.1f;
    [Range(0f, 5f)]
    public float customRotateSpeed;
	public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f;	//how fast to rotate on the ground, how fast to rotate in the air
	public float maxSpeed = 9;								//maximum speed of movement in X/Z axis
	public float slopeLimit = 40, slideAmount = 35;			//maximum angle of slopes you can walk on, how fast to slide down slopes you can't
	public float movingPlatformFriction = 7.7f;				//you'll need to tweak this to get the player to stay on moving platforms properly
    public float groundCheckDist = 1f;

    //jumping
    public Vector3 jumpForce =  new Vector3(0, 13, 0);		//normal jump force
	public Vector3 secondJumpForce = new Vector3(0, 13, 0); //the force of a 2nd consecutive jump
	public Vector3 thirdJumpForce = new Vector3(0, 13, 0);	//the force of a 3rd consecutive jump
	public float jumpDelay = 0.1f;							//how fast you need to jump after hitting the ground, to do the next type of jump
    public float freezeDelay = 0.1f;
	public float jumpLeniancy = 0.17f;						//how early before hitting the ground you can press jump, and still have it work
	[HideInInspector]
	public int onEnemyBounce;

	private int onJump;
	private bool grounded;
	private Transform[] floorCheckers;
	private Quaternion screenMovementSpace;
	private float airPressTime, groundedCount, inAirCount, curAccel, curDecel, curRotateSpeed, slope;
	protected Vector3 direction, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;
    private float speedMagnitude;

	private CharacterMotor characterMotor;
	private EnemyAI enemyAI;
	private DealDamage dealDamage;
	protected Rigidbody rigid;
	private AudioSource aSource;

    //custom variables
    [SerializeField]
    private float directionSpeed = 3f;
    private float horizontal, vertical;
    [SerializeField]
    CameraSelector cameraScript;
	/*[SerializeField]
	private LedgeHandling ledgeHandling;*/
    [SerializeField]
    protected float stopDistance;
    [SerializeField]
    private float locomotionThreshold = .1f;
    [SerializeField]
    protected AnimationCurve accelCurve;
    [SerializeField]
    protected Vector3 slopeNormal;
    [SerializeField]
    protected Vector3 specificSlopeNormal;
    [SerializeField]
    protected float slopeCorrectionAmt;
    [SerializeField]
    private Vector3 testVector;
    [SerializeField]
    private LayerMask mask;
    private Vector3 lastFramePosition;

    //Ledges
    [SerializeField] float heightCorrectionSpeed;


    //public properties
    public float SpeedMagnitude {get{return characterMotor.currentSpeed.magnitude;}}
    public float LocomotionThreshold{get{return locomotionThreshold;}}

    //setup
    void Awake()
	{
		//create single floorcheck in centre of object, if none are assigned
		if(!floorChecks)
		{
			floorChecks = new GameObject().transform;
			floorChecks.name = "FloorChecks";
			floorChecks.parent = transform;
			floorChecks.position = transform.position;
			GameObject check = new GameObject();
			check.name = "Check1";
			check.transform.parent = floorChecks;
			check.transform.position = transform.position;
			Debug.LogWarning("No 'floorChecks' assigned to PlayerMove script, so a single floorcheck has been created", floorChecks);
		}
		//assign player tag if not already
		if(!CompareTag("Player"))
		{
			tag = "Player";
			Debug.LogWarning ("PlayerMove script assigned to object without the tag 'Player', tag has been assigned automatically", transform);
		}
		//usual setup
		mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
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
		horizontal = Input.GetAxis ("Horizontal");
		vertical = Input.GetAxis ("Vertical");

        var speed = new Vector2(horizontal, vertical).sqrMagnitude;

		if (cameraScript.CurrentCamState == CameraSelector.CamState.FirstPerson) return;
		StickToWorldSpace(transform, mainCam.transform, ref direction, ref speed);
		moveDirection = transform.position + direction;

		//float directionFloat = 0f;
        //StickToWorldSpace(this.transform, mainCam.transform, ref directionFloat, ref speed);
        //direction = (screenMovementForward * vertical) + (screenMovementRight * horizontal);
    }

    public void StickToWorldSpace(Transform root, Transform camera, ref Vector3 directionOut, ref float speedOut)
    {
        Vector3 rootDirection = root.forward;
        Vector3 stickDirection = new Vector3(horizontal, 0, vertical);

        speedOut = stickDirection.sqrMagnitude;

        //Get camera rotation
        Vector3 CameraDirection = camera.forward;
        CameraDirection.y = 0.0f;
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, CameraDirection);

        //Convert joystick input into worldspace coordinates
        Vector3 moveDirection = referentialShift * stickDirection;
        Vector3 axisSign = Vector3.Cross(moveDirection, rootDirection);

           Debug.DrawRay(new Vector3(root.position.x, root.position.y + 1f, root.position.z), moveDirection, Color.green);
           Debug.DrawRay(new Vector3(root.position.x, root.position.y + 1f, root.position.z), axisSign, Color.cyan);
           Debug.DrawRay(new Vector3(root.position.x, root.position.y + 1f, root.position.z), rootDirection, Color.red);
           Debug.DrawRay(new Vector3(root.position.x, root.position.y + 1f, root.position.z), stickDirection, Color.blue);

        rotateSpeed = cameraScript.CurrentCamState == CameraSelector.CamState.Follow ? 0f : customRotateSpeed;
		var angleRootToMove = Vector3.Angle(rootDirection, moveDirection) * (axisSign.y >= 0 ? -1f : 1f);
		angleRootToMove /= 180f;
		directionOut = moveDirection;
    }

    void CorrectForSlope(Vector3 slopeNormal)
    {
       // Debug.DrawRay(transform.position, (slopeNormal), Color.red);
        Vector3 slopeTangent = SlopeTangentAnalysis(slopeNormal);
        //Debug.DrawRay(transform.position, (slopeTangent), Color.blue);
        //print("slope normal = " + slopeNormal);
        Vector3 slopeCorrection = Vector3.Cross(slopeNormal, slopeTangent);
        //Debug.DrawRay(transform.position, slopeCorrection * slopeCorrectionAmt, Color.red);
        rigid.AddForce(slopeCorrection * slopeCorrectionAmt);

        //new Vector3(slopeNormal.z, 0, slopeNormal.x);


        Vector3 test = Quaternion.AngleAxis(90,slopeNormal).eulerAngles;




    }

    Vector3 SlopeTangentAnalysis(Vector3 incomingNormal)
    {
        Vector3 tangentResult = Vector3.zero;
        tangentResult = new Vector3(-incomingNormal.z, 0, incomingNormal.x);
        return tangentResult;
    }

	//apply correct player movement (fixedUpdate for physics calculations)
	void FixedUpdate()
	{
		//are we grounded
		grounded = IsGrounded ();
        //move, rotate, manage speed
        Vector3 adjustedMoveDirection = moveDirection.normalized;
        Vector3 movementVector = transform.position - moveDirection;
        CorrectForSlope(slopeNormal);
        if (grounded && movementVector.magnitude <= .01f && inAirCount > freezeDelay)
        {
            rigid.isKinematic = true;
        }else if (movementVector.magnitude >= .01f && grounded)
        {
            rigid.isKinematic = false;
            //print(specificSlopeNormal);

            //if (slopeNormal != Vector3.up)

        }

        Vector3 slopeMovementVector = -Vector3.ProjectOnPlane(movementVector, slopeNormal);
        //characterMotor.MoveTo(moveDirection, accelCurve.Evaluate(direction.magnitude), stopDistance, true);

        Debug.DrawRay(transform.position + Vector3.up, slopeNormal, Color.white);
        Debug.DrawRay(transform.position + Vector3.up, -Vector3.ProjectOnPlane(movementVector, slopeNormal), Color.magenta);
        //Debug.DrawRay(transform.position, transform.position - moveDirection , Color.green);
        //Debug.DrawRay(transform.position, (transform.forward ), Color.blue);
        // var newVector = Quaternion.AngleAxis(testVector.x, slopeNormal);
        // Vector3 newVector = new Vector3(slopeNormal.z, 0, slopeNormal.x);


        // Debug.DrawRay(transform.position, (moveDirection - transform.position) + slopeNormal, Color.magenta);



        //Debug.Log ("accelerating with " + accelCurve.Evaluate(direction.magnitude)+ " magnitude");
        //float accel = (grounded) ? accelCurve.Evaluate(direction.magnitude) : accelCurve.Evaluate(direction.magnitude) * airAccel;
        Vector3 prevVector = slopeMovementVector;
        characterMotor.MoveTo(slopeMovementVector, accel, stopDistance, false);


        //characterMotor.MoveTo (moveDirection, curAccel, stopDistance, true);
        //print("movedirection is " + moveDirection);
        if (rotateSpeed != 0 && direction.magnitude != 0)
            characterMotor.RotateToDirection(moveDirection, curRotateSpeed * 5, true);

        characterMotor.ManageSpeed (curDecel, maxSpeed + movingObjSpeed.magnitude, true);
		//set animation values
		if(animator)
		{
            animator.SetFloat("Speed", direction.magnitude);
            //animator.SetFloat("DistanceToTarget", direction.magnitude);
            // animator.SetFloat("Turn", moveDirection.x);
            //  animator.SetBool("OnGround", grounded);
            // animator.SetBool("Grounded", grounded);
            //animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);

		}
        lastFramePosition = transform.position;
	}


	//prevents rigidbody from sliding down slight slopes (read notes in characterMotor class for more info on friction)
	void OnCollisionStay(Collision other)
	{

        //only stop movement on slight slopes if we aren't being touched by anything else
        if (other.collider.tag != "Untagged" || grounded == false)
        {

            return;
        }
		//if no movement should be happening, stop player moving in Z/X axis
		if(direction.magnitude == 0 && slope < slopeLimit && rigid.velocity.magnitude < 2)
		{
           // print("in collision function setting");
            //it's usually not a good idea to alter a rigidbodies velocity every frame
            //but this is the cleanest way i could think of, and we have a lot of checks beforehand, so it should be ok
            rigid.velocity = Vector3.zero;
		}
	}

	//returns whether we are on the ground or not
	//also: bouncing on enemies, keeping player on moving platforms and slope checking
	private bool IsGrounded()
	{
		//get distance to ground, from centre of collider (where floorcheckers should be)
		float dist = GetComponent<Collider>().bounds.extents.y;
        //check whats at players feet, at each floorcheckers position
        int connectingRays = 0;
        Vector3 totalNormal = Vector3.zero;
        int loopCount = 0;
		foreach (Transform check in floorCheckers)
		{
			RaycastHit hit;
			if(Physics.Raycast(check.position, Vector3.down * groundCheckDist, out hit, dist + 0.05f, mask))
			{
                //Debug.DrawRay(check.position, Vector3.down, Color.green);
				if(!hit.transform.GetComponent<Collider>().isTrigger)
				{
                    //slope control
                    //slopeNormal = hit.normal.normalized;

                    totalNormal += hit.normal.normalized; ;
                    slope = Vector3.Angle (hit.normal, Vector3.up);
					//slide down slopes
					if(slope > slopeLimit && hit.transform.tag != "Pushable")
					{
						Vector3 slide = new Vector3(0f, -slideAmount, 0f);
						rigid.AddForce (slide, ForceMode.Force);
					}
					//enemy bouncing
					if (hit.transform.tag == "Enemy" && rigid.velocity.y < 0)
					{
						enemyAI = hit.transform.GetComponent<EnemyAI>();
						enemyAI.BouncedOn();
						onEnemyBounce ++;
						dealDamage.Attack(hit.transform.gameObject, 1, 0f, 0f);
					}
					else
						onEnemyBounce = 0;
					//moving platforms
					if (hit.transform.tag == "MovingPlatform" || hit.transform.tag == "Pushable")
					{
						movingObjSpeed = hit.transform.GetComponent<Rigidbody>().velocity;
						movingObjSpeed.y = 0f;
						//9.5f is a magic number, if youre not moving properly on platforms, experiment with this number
						rigid.AddForce(movingObjSpeed * movingPlatformFriction * Time.fixedDeltaTime, ForceMode.VelocityChange);
					}
					else
					{
						movingObjSpeed = Vector3.zero;
					}
                    //yes our feet are on something
                    //return true;
                    connectingRays++;
                    if (loopCount == 5 )
                    {
                        specificSlopeNormal = hit.normal;
                    }
				}
			}
            else
            {
                Debug.DrawRay(check.position, Vector3.down, Color.red);
            }
            loopCount++;
		}

        float nX = Mathf.Abs(totalNormal.x) > 0 ? totalNormal.x / connectingRays : 0;
        float nY = Mathf.Abs(totalNormal.y) > 0 ? totalNormal.y / connectingRays : 0;
        float nZ = Mathf.Abs(totalNormal.z) > 0 ? totalNormal.z / connectingRays : 0;

        slopeNormal = new Vector3(nX, nY, nZ);

        float sX = Mathf.Abs(specificSlopeNormal.x) > 0 ? specificSlopeNormal.x / 2 : 0;
        float sY = Mathf.Abs(specificSlopeNormal.y) > 0 ? specificSlopeNormal.y / 2 : 0;
        float sZ = Mathf.Abs(specificSlopeNormal.z) > 0 ? specificSlopeNormal.z / 2 : 0;

       // specificSlopeNormal = new Vector3(sX, sY, sZ);

        movingObjSpeed = Vector3.zero;
        //no none of the floorchecks hit anything, we must be in the air (or water)
        if (connectingRays == 0)
            return false;
        else
           return true;

	}

	//jumping
	private void JumpCalculations()
	{
		//keep how long we have been on the ground
		groundedCount = (grounded) ? groundedCount += Time.deltaTime : 0f;
        inAirCount = (grounded) ? 0f : groundedCount += Time.deltaTime;

        //play landing sound
        if (groundedCount < 0.25 && groundedCount != 0 && !GetComponent<AudioSource>().isPlaying && landSound && GetComponent<Rigidbody>().velocity.y < 1)
		{
			aSource.volume = Mathf.Abs(GetComponent<Rigidbody>().velocity.y)/40;
			aSource.clip = landSound;
			aSource.Play ();
		}
		//if we press jump in the air, save the time
		if (Input.GetButtonDown ("Jump") && !grounded)
			airPressTime = Time.time;

		//if were on ground within slope limit
		if (grounded && slope < slopeLimit)
		{
			//and we press jump, or we pressed jump justt before hitting the ground
			if (Input.GetButtonDown ("Jump") || airPressTime + jumpLeniancy > Time.time)
			{
				//increment our jump type if we haven't been on the ground for long
				onJump = (groundedCount < jumpDelay) ? Mathf.Min(2, onJump + 1) : 0;
				//execute the correct jump (like in mario64, jumping 3 times quickly will do higher jumps)
				if (onJump == 0)
						Jump (jumpForce);
				else if (onJump == 1)
						Jump (secondJumpForce);
				else if (onJump == 2){
						Jump (thirdJumpForce);
						onJump --;
				}
			}
		}
	}

	//push player at jump force
	public void Jump(Vector3 jumpVelocity)
	{
        animator.SetTrigger("Jump");
        rigid.isKinematic = false;
        if (jumpSound)
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
