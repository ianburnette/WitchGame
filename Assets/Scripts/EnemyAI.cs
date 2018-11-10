using UnityEngine;
using System.Collections;
using Cinemachine;

//simple "platformer enemy" AI
[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(DealDamage))]
public class EnemyAI : MonoBehaviour
{
	public float acceleration = 35f;					//acceleration of enemy movement
	public float deceleration = 8f;						//deceleration of enemy movement
	public float rotateSpeed = 0.7f;					//how fast enemy can rotate
	public float speedLimit = 10f;						//how fast enemy can move
	public Vector3 bounceForce = new Vector3(0, 13, 0);	//force to apply to player when player jumps on enemies head
	public AudioClip bounceSound;						//sound when you bounce on enemies
	public float pushForce = 10f;						//how far away to push the player when they are attacked by the enemy
	public float pushHeight = 7f;						//how high to push the player when they are attacked by the enemy
	public int attackDmg = 1;							//how much damage to deal to the player when theyre attacked by this enem
	public bool chase = true;							//should this enemy chase objects inside its sight?
	public bool ignoreY = true;							//ignore Y axis when chasing? (this would be false for say.. a flying enemy)
	public float chaseStopDistance = 0.7f;				//stop this far away from object when chasing it
	public GameObject sightBounds;						//trigger for sight bounds
	public GameObject attackBounds;						//trigger for attack bounds (player is hurt when they enter these bounds)
	public Animator animatorController;					//object which holds the animator for this enem

	private TriggerParent sightTrigger;
	private TriggerParent attackTrigger;
	PlayerWalkMove playerWalkMove;
	private CharacterMotor characterMotor;
	private DealDamage dealDamage;

	[SerializeField] Vector3 wanderBasePosition;
	[SerializeField] float wanderDistance;
	[SerializeField] float wanderTime;
	bool wander = true;
	public Vector3 targetPosition;
	[SerializeField] float wanderStopDistance;
	
	public bool Flee
	{
		set
		{
			sightTrigger.enabled = false;
			attackTrigger.enabled = false;
			chase = false;
		}
	}

	//setup
	void Awake()
	{
		characterMotor = GetComponent<CharacterMotor>();
		dealDamage = GetComponent<DealDamage>();

		if (sightBounds) sightTrigger = sightBounds.GetComponent<TriggerParent>();
		if(attackBounds) attackTrigger = attackBounds.GetComponent<TriggerParent>();
		wanderBasePosition = transform.position;
		InvokeRepeating(nameof(Wander), 0, wanderTime);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(targetPosition, .3f);
	}

	void Wander()
	{
		var pos = wanderBasePosition + RandomPosition(wanderDistance);
		Physics.Raycast(wanderBasePosition, pos, out var hit);
		if (hit.transform == null)
			targetPosition = pos;
		else
			Wander();
	}

	Vector3 RandomPosition(float range) => new Vector3(RandomPoint(range), RandomPoint(range), RandomPoint(range));

	float RandomPoint(float range) => Random.Range(-range, range); 
	
	void Update()
	{
		if (sightTrigger && sightTrigger.colliding && chase && sightTrigger.hitObject != null && sightTrigger.hitObject.activeInHierarchy)
		{
			transform.LookAt(sightTrigger.hitObject.transform);
			characterMotor.MoveTo (sightTrigger.hitObject.transform.position, acceleration, chaseStopDistance, ignoreY);
			if(animatorController) animatorController.SetBool("Moving", true);
			wander = false;
		}
		else
		{
			if(animatorController) animatorController.SetBool("Moving", false);
			wander = true;
		}

		if (wander)
		{
			transform.LookAt(targetPosition);
			characterMotor.MoveTo(targetPosition, acceleration, wanderStopDistance, ignoreY);
		}

		if (attackTrigger && attackTrigger.collided)
		{
			dealDamage.Attack(attackTrigger.hitObject, attackDmg, pushHeight, pushForce);
			if(animatorController)
				animatorController.SetBool("Attacking", true);
		}
		else if(animatorController)
			animatorController.SetBool("Attacking", false);
	}

	void FixedUpdate()
	{
		characterMotor.ManageSpeed(deceleration, speedLimit, ignoreY);
		characterMotor.RotateToVelocity (rotateSpeed, ignoreY);
	}

	public void BouncedOn()
	{
		if(!playerWalkMove)
			playerWalkMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerWalkMove>();
		if (bounceSound)
			AudioSource.PlayClipAtPoint(bounceSound, transform.position);
		if(playerWalkMove)
		{
			var bounceMultiplier = new Vector3(0f, 1.5f, 0f) * playerWalkMove.onEnemyBounce;
			playerWalkMove.BounceOnEnemy (bounceForce + bounceMultiplier);
		}
	}
}
