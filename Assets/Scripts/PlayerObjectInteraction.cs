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
	[SerializeField] Vector3 broomHoldPos;
	[SerializeField] bool showDebug;

	[Header("External References")]
	[SerializeField] Animator animator;
	[SerializeField] int armsAnimationLayer;
	[SerializeField] PlayerWalkMove playerWalkMove;
	[SerializeField] MovementStateMachine movementStateMachine;

	[Header("Internal References")]
	public Collider objectEligibleForPickup;
	public Collider currentlyHeldObject;

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
			                     movementStateMachine.CurrentMovementState == playerWalkMove
				                     ? transform.position + holdPos
				                     : BroomHoldPosition());
		if (animator) Animate();
	}

	Vector3 BroomHoldPosition() =>
		transform.position +
		transform.up * broomHoldPos.y +
		transform.forward * broomHoldPos.z;

	void Animate() => animator.SetBool("HoldingPickup",
	                                   (currentlyHeldObject != null &&
	                                    movementStateMachine.CurrentMovementState == playerWalkMove));

	void OnTriggerEnter (Collider other) =>
		objectEligibleForPickup = other.CompareTag("Pickup") ? other : null;

	public bool PickupObjectInteraction() {
		if (objectEligibleForPickup == null && currentlyHeldObject == null) return false;
		if (currentlyHeldObject!=null)
			ThrowPickup();
		else
			LiftPickup(objectEligibleForPickup);
		return true;
	}

	void OnTriggerExit(Collider other) {
		if (objectEligibleForPickup == other)
			objectEligibleForPickup = null;
	}

	void LiftPickup(Collider other) {
		currentlyHeldObject = other;
		objectEligibleForPickup = null;
		currentlyHeldObject.GetComponent<Rigidbody>().isKinematic = true;
		currentlyHeldObject.isTrigger = true;
	}

	public void ThrowPickup() {
		Rigidbody r = currentlyHeldObject.GetComponent<Rigidbody>();
		r.isKinematic = false;
		r.AddRelativeForce (throwForce, ForceMode.VelocityChange);

		currentlyHeldObject.isTrigger = false;
		currentlyHeldObject = null;
	}
}
