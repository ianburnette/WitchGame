using UnityEngine;
using UnityEngine.Experimental.UIElements;

//this allows the player to pick up/throw, and also pull certain objects
//you need to add the tags "Pickup" or "Pushable" to these objects
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerWalkMove))]
public class PlayerObjectInteraction : MonoBehaviour {

	[Header("Behavior variables")]
	[SerializeField] Vector3 throwForce = new Vector3(0, 5, 7);
	[SerializeField] private Vector3 dropForce = new Vector3(0, 0, 2);
	[SerializeField] Vector3 holdPos;
	[SerializeField] Vector3 broomHoldPos;
	[SerializeField] bool showDebug;
	[SerializeField] private float throwThreshold;

	[Header("External References")]
	[SerializeField] Animator animator;
	[SerializeField] int armsAnimationLayer;
	[SerializeField] PlayerMoveBase moveBase;
	[SerializeField] MovementStateMachine movementStateMachine;

	[Header("Internal References")]
	public Collider objectEligibleForPickup;

	public Collider currentlyHeldObject;
	public bool holdingObject;

	[Header("Snapping")]
	private ObjectSnapZone currentZone;

	[SerializeField] private float throwAngle;
	[SerializeField] private float slotForceDamp;
	[SerializeField] private float upMult;

	public Collider CurrentlyHeldObject
	{
		private get { return currentlyHeldObject; }
		set{
			currentlyHeldObject = value;
			holdingObject = currentlyHeldObject != null;}
	}

	public ObjectSnapZone CurrentZone
	{
		get { return currentZone; }
		set { currentZone = value; }
	}

	void Awake() {
		if(animator)
			animator.SetLayerWeight(armsAnimationLayer, 1);
	}

	void OnDrawGizmos() {
		if (!showDebug) return;
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position + holdPos, .3f);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position + broomHoldPos, .3f);
	}

	void UpdateObjectPosition(Transform objTransform, Vector3 position) {
		objTransform.position = position;
		objTransform.rotation = transform.rotation;
	}

	private void Update() {
		if (holdingObject)
			UpdateObjectPosition(currentlyHeldObject.transform,
			                     movementStateMachine.CurrentMovementState == MoveState.Walk
				                     ? transform.position + holdPos
				                     : BroomHoldPosition());
		//TODO make this responsive to other movement states
		if (animator) Animate();
	}

	private Vector3 BroomHoldPosition() =>
		transform.position +
		transform.up * broomHoldPos.y +
		transform.forward * broomHoldPos.z;

	private void Animate() => animator.SetBool("HoldingPickup",
											  (CurrentlyHeldObject != null &&
									           movementStateMachine.CurrentMovementState == MoveState.Walk));

	private void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag("Pickup"))
			objectEligibleForPickup = other;
	}

	public bool PickupObjectInteraction() {
		if (objectEligibleForPickup == null && CurrentlyHeldObject == null) return false;
		if (CurrentlyHeldObject != null)
			LetGoOfPickup();
		else
			LiftPickup(objectEligibleForPickup);
		return true;
	}


	private void OnTriggerExit(Collider other) {
		if (objectEligibleForPickup == other)
			objectEligibleForPickup = null;
	}

	private void LiftPickup(Collider other) {
		CurrentlyHeldObject = other;
		objectEligibleForPickup = null;
		CurrentlyHeldObject.GetComponent<Rigidbody>().isKinematic = true;
		CurrentlyHeldObject.GetComponent<Pickup>().SnapTargetPos = Vector3.zero;
		CurrentlyHeldObject.isTrigger = true;
	}

	public void AccidentallyLetGoOfPickup() => LetGoOfPickup();

	void LetGoOfPickup()
	{
		if (currentlyHeldObject == null) return;
		var rb = ResetAndReturnPickup();
		if (rb == null) return;
		var vel = moveBase.rigid.velocity.magnitude > throwThreshold ? throwForce : dropForce;
		ThrowOrDrop(rb, vel);
		SnapToSlotIfPresent();
		CleanUpCurrentlyHeld();
	}

	Rigidbody ResetAndReturnPickup()
	{
		var r = CurrentlyHeldObject.GetComponent<Rigidbody>();
		r.isKinematic = false;
		return r;
	}

	private void ThrowOrDrop(Rigidbody rb, Vector3 throwOrDropForce)
	{
		//if (currentZone != null)
		//	ThrowToTarget();
		//else
			rb.AddRelativeForce(throwOrDropForce + SlotForce(), ForceMode.VelocityChange);
	}

	private void SnapToSlotIfPresent()
	{
		if (currentZone != null) 
			CurrentlyHeldObject.GetComponent<Pickup>().SnapTo(currentZone);
	}

	private void ThrowToTarget()
	{
		var targetPos = currentZone.CurrentSnapSlot().pos + currentZone.transform.position;
		var targetXZPos = ZeroYAxis(targetPos);
		CurrentlyHeldObject.transform.LookAt(targetXZPos);

		var position = currentlyHeldObject.transform.position;
		var R = Vector3.Distance(ZeroYAxis(position), targetXZPos);
		var G = Physics.gravity.y;
		var tanAlpha = Mathf.Tan(throwAngle * Mathf.Deg2Rad);
		var H = targetPos.y - position.y;

		var Vz0 = G * R * R;
		var Vz1 = H - R * tanAlpha;
		var Vz2 = 2.0f * Vz1;
		var Vz3 = Vz0 / Vz2;
		var Vz = Mathf.Sqrt(Vz3);
		var Vy = tanAlpha * Vz;

		var localVelocity = new Vector3(0f, Vy, Vz);
		var globalVelocity = currentlyHeldObject.transform.TransformDirection(localVelocity);

		currentlyHeldObject.GetComponent<Rigidbody>().velocity = globalVelocity;
	}

	Vector3 ZeroYAxis(Vector3 vec) => new Vector3(vec.x, 0, vec.z);

	private void CleanUpCurrentlyHeld()
	{
		CurrentlyHeldObject.isTrigger = false;
		CurrentlyHeldObject = null;
	}

	private Vector3 SlotForce()
	{
		if (currentZone == null) return Vector3.zero;
		var targetPos = currentZone.CurrentSnapSlot().pos + currentZone.transform.position;
		return ((targetPos - transform.position) * slotForceDamp) + 
		       (Vector3.up * Vector3.Distance(targetPos, transform.position) * upMult);
	}
}
