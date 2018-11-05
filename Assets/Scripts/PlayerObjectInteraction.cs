using UnityEngine;

//this allows the player to pick up/throw, and also pull certain objects
//you need to add the tags "Pickup" or "Pushable" to these objects
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerWalkMove))]
public class PlayerObjectInteraction : MonoBehaviour {

	[Header("Behavior variables")]
	[SerializeField] Vector3 throwForce = new Vector3(0, 5, 7);
	[SerializeField] Vector3 holdPos;
	[SerializeField] float dropForwardMult;
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

	[field: Header("Snapping")]
	[field: SerializeField]
	public SnapSlot CurrentSlot { get; set; }

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

	void Update() {
		if (currentlyHeldObject != null)
			UpdateObjectPosition(currentlyHeldObject.transform,
			                     movementStateMachine.CurrentMovementState == MoveState.Walk
				                     ? transform.position + holdPos
				                     : BroomHoldPosition());
		//TODO make this responsive to other movement states
		if (animator) Animate();
	}

	Vector3 BroomHoldPosition() =>
		transform.position +
		transform.up * broomHoldPos.y +
		transform.forward * broomHoldPos.z;

	void Animate() => animator.SetBool("HoldingPickup",
	                                   (currentlyHeldObject != null &&
	                                    movementStateMachine.CurrentMovementState == MoveState.Walk));

	void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag("Pickup"))
			objectEligibleForPickup = other;
	}

	public bool PickupObjectInteraction() {
		if (objectEligibleForPickup == null && currentlyHeldObject == null) return false;
		if (currentlyHeldObject != null)
			LetGoOfPickup();
		else
			LiftPickup(objectEligibleForPickup);
		return true;
	}

	private void LetGoOfPickup()
	{
		if (moveBase.rigid.velocity.magnitude > throwThreshold)
			ThrowPickup();
		else
			DropPickup();
	}

	void OnTriggerExit(Collider other) {
		if (objectEligibleForPickup == other)
			objectEligibleForPickup = null;
	}

	void LiftPickup(Collider other) {
		currentlyHeldObject = other;
		objectEligibleForPickup = null;
		currentlyHeldObject.GetComponent<Rigidbody>().isKinematic = true;
		currentlyHeldObject.GetComponent<Pickup>().CurrentSlot = null;
		currentlyHeldObject.isTrigger = true;
	}

	public void DropPickup()
	{
		var r = currentlyHeldObject.GetComponent<Rigidbody>();
		r.isKinematic = false;
		currentlyHeldObject.transform.position = transform.position + transform.forward * dropForwardMult;
		currentlyHeldObject.isTrigger = false;
		currentlyHeldObject = null;
	}
	
	public void ThrowPickup() {
		var r = currentlyHeldObject.GetComponent<Rigidbody>();
		r.isKinematic = false;
		if (CurrentSlot != null)
		{
			r.AddRelativeForce(throwForce + (transform.position - CurrentSlot.pos));
			currentlyHeldObject.GetComponent<Pickup>().SnapTo(CurrentSlot);
		}
		else
			r.AddRelativeForce(throwForce, ForceMode.VelocityChange);

		currentlyHeldObject.isTrigger = false;
		currentlyHeldObject = null;
	}
}
