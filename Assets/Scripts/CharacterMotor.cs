using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMotor : MonoBehaviour, IVelocityLimiter {
	[HideInInspector] public Vector3 currentSpeed;
	[HideInInspector] public float DistanceToTarget;
	[SerializeField] float maxMagnitude = 50f;
	[FormerlySerializedAs("rigid")] [SerializeField] Rigidbody rb;

	public Vector3 relativePosition;

	void Awake() {
		rb.interpolation = RigidbodyInterpolation.Interpolate;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}

	void FixedUpdate() => LimitVelocity();

	public bool MoveTo(Vector3 destination, float acceleration, float stopDistance, bool ignoreY) {
		relativePosition = (destination - transform.position);
		if (ignoreY)
			relativePosition.y = 0;

		DistanceToTarget = relativePosition.magnitude;
		if (DistanceToTarget <= stopDistance)
			return true;
		rb.AddForce(relativePosition.normalized * acceleration * Time.deltaTime, ForceMode.VelocityChange);
		return false;
	}

	public void MoveVertical(Vector3 destination) {
		relativePosition = (destination - transform.position);
		transform.Translate(destination);
	}

	public void RotateToVelocity(float turnSpeed, bool ignoreY) {
		var dir = new Vector3(rb.velocity.x, ignoreY ? 0f : rb.velocity.y, rb.velocity.z);
		RotateToDirection(turnSpeed, dir);
	}

	public void RotateToDirection(float turnSpeed, Vector3 velocity) {
		if (!(velocity.magnitude > 0.1)) return;
		var dirQ = Quaternion.LookRotation(velocity);
		var slerp = Quaternion.Slerp(transform.rotation, dirQ, velocity.magnitude * turnSpeed * Time.deltaTime);
		rb.MoveRotation(slerp);
	}

	public void RotateToVelocity(float turnSpeed, float minYvel, float maxYvel, float yMultiplier) {
		var currentY = Scale(minYvel, maxYvel, rb.velocity.y);
		RotateToDirection(turnSpeed, new Vector3(rb.velocity.x, currentY * yMultiplier * rb.velocity.magnitude, rb.velocity.z));
	}

	public float Scale(float newMin, float newMax, float oldValue) {
		const float oldMax = 10f;
		const float oldMin = 0f;
		const float oldRange = (oldMax - oldMin);
		var newRange = (newMax - newMin);
		var newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

		return(newValue);
	}

public void RotateToDirection(Vector3 lookDir, float turnSpeed, bool ignoreY)
	{
		var newDir = lookDir - transform.position;
		if (ignoreY) newDir.y = 0;
		var dirQ = Quaternion.LookRotation (newDir);
		var slerp = Quaternion.Slerp (transform.rotation, dirQ, turnSpeed * Time.deltaTime);
		rb.MoveRotation (slerp);
	}

	public void ManageSpeed(float deceleration, float maxSpeed, bool ignoreY)
	{
		currentSpeed = rb.velocity;
		if (ignoreY)
			currentSpeed.y = 0;

		if (!(currentSpeed.magnitude > 0)) return;
		rb.AddForce ((currentSpeed * -1) * deceleration * Time.deltaTime, ForceMode.VelocityChange);
		if (rb.velocity.magnitude > maxSpeed)
			rb.AddForce ((currentSpeed * -1) * deceleration * Time.deltaTime, ForceMode.VelocityChange);
	}

	public void MoveRelativeToGround(Vector3 groundForce) => rb.AddForce(groundForce);

	public void SetVelocity(Vector3 direction) =>
		rb.velocity = direction;


	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		if (relativePosition != transform.position)
			Gizmos.DrawSphere(transform.position + relativePosition, .3f);
	}

	public void LimitVelocity()
	{
		if (rb.velocity.magnitude > maxMagnitude)
			rb.velocity = rb.velocity.normalized * maxMagnitude;
	}
}
