using UnityEngine;

//this class holds movement functions for a rigidbody character such as player, enemy, npc..
//you can then call these functions from another script, in order to move the character
[RequireComponent(typeof(Rigidbody))]
public class CharacterMotor : MonoBehaviour
{
	[HideInInspector]
	public Vector3 currentSpeed;
	[HideInInspector]
	public float DistanceToTarget;
	[SerializeField] Rigidbody rigid;

	void Awake()
	{
		rigid.interpolation = RigidbodyInterpolation.Interpolate;
		rigid.constraints = RigidbodyConstraints.FreezeRotation;
	}

	public bool MoveTo(Vector3 destination, float acceleration, float stopDistance, bool ignoreY)
	{

		Vector3 relativePos = (destination - transform.position);
		if(ignoreY)
			relativePos.y = 0;

		DistanceToTarget = relativePos.magnitude;
		if (DistanceToTarget <= stopDistance)
			return true;
		rigid.AddForce(relativePos.normalized * acceleration * Time.deltaTime, ForceMode.VelocityChange);
		return false;
	}

	public void RotateToVelocity(float turnSpeed, bool ignoreY)
	{
		var dir = new Vector3(rigid.velocity.x, ignoreY ? 0f : rigid.velocity.y, rigid.velocity.z);

		if (!(dir.magnitude > 0.1)) return;
		var dirQ = Quaternion.LookRotation (dir);
		var slerp = Quaternion.Slerp (transform.rotation, dirQ, dir.magnitude * turnSpeed * Time.deltaTime);
		rigid.MoveRotation(slerp);
	}

	public void RotateToDirection(Vector3 lookDir, float turnSpeed, bool ignoreY)
	{
		var newDir = lookDir - transform.position;
		if (ignoreY) newDir.y = 0;
		var dirQ = Quaternion.LookRotation (newDir);
		var slerp = Quaternion.Slerp (transform.rotation, dirQ, turnSpeed * Time.deltaTime);
		rigid.MoveRotation (slerp);
	}

	public void ManageSpeed(float deceleration, float maxSpeed, bool ignoreY)
	{
		currentSpeed = rigid.velocity;
		if (ignoreY)
			currentSpeed.y = 0;

		if (!(currentSpeed.magnitude > 0)) return;
		rigid.AddForce ((currentSpeed * -1) * deceleration * Time.deltaTime, ForceMode.VelocityChange);
		if (rigid.velocity.magnitude > maxSpeed)
			rigid.AddForce ((currentSpeed * -1) * deceleration * Time.deltaTime, ForceMode.VelocityChange);
	}

	public void MoveRelativeToGround(Vector3 groundForce) {
		rigid.AddForce(groundForce);
	}
}
