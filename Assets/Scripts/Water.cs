using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class Water : MonoBehaviour
{
	public AudioClip splashSound;						//played when objects enter water
	public Vector3 force = new Vector3(0, 16.5f, 0);	//pushForce of the water. This is a vector3 so you can have force in any direction, for example a current or river
	public bool effectPlayerDrag;						//should the players rigidbody be effected by the drag/angular drag values of the water?
	public float resistance = 0.4f;						//the drag applied to rigidbodies in the water (but not player)
	public float angularResistance = 0.2f;				//the angular drag applied to rigidbodies in the water (but not player)

	List<Rigidbody> rigidbodiesInWaterFloat = new List<Rigidbody>();
	List<Rigidbody> rigidbodiesInWaterSink = new List<Rigidbody>();

	Collider col;

	[SerializeField] float maxDepth = .4f;
	[SerializeField] float depthCorrectionSpeed = 2f;
	[SerializeField] float particleSpeedMult = 2f;

	[SerializeField] ParticleSystem particles;

	//Dictionary<GameObject, float> dragStore = new Dictionary<GameObject, float>();
	//Dictionary<GameObject, float> angularStore = new Dictionary<GameObject, float>();


	void OnEnable() {
		col = GetComponent<Collider>();
		if (!particles) return;
		var particlesMain = particles.main;
		var particleForce = new Vector3(force.x, 0, force.z);
		particlesMain.startSpeed = particleForce.magnitude * particleSpeedMult;
	}

	void FixedUpdate() => rigidbodiesInWaterFloat.ForEach(ApplyWaterForce);

	void ApplyWaterForce(Rigidbody rb) => rb.AddForce(force * DepthCorrectIfNeccessary(rb.transform.position.y),
	                                                  ForceMode.Force);

	float DepthCorrectIfNeccessary(float height) {
		var depth = Depth(height);
		return depth > maxDepth ? depth * depthCorrectionSpeed : 1;
	}

	float Depth(float baseHeight) => baseHeight + col.bounds.extents.y;

	void OnTriggerEnter(Collider other) {
		var rb = other.GetComponent<Rigidbody>();
		if (rb == null)
			return;
		if (rb.transform.CompareTag("Player")) {
			other.GetComponent<MovementStateMachine>()?.WaterMovement();
			if (other.GetComponent<PlayerAbilities>().underwaterUnlocked) {
				rigidbodiesInWaterSink.Add(rb);
				return;
			}
		}
		rigidbodiesInWaterFloat.Add(rb);
	}

	void OnTriggerExit(Collider other) {
		var rb = other.GetComponent<Rigidbody>();
		if (rb == null)
			return;
		if (rigidbodiesInWaterFloat.Contains(rb))
			rigidbodiesInWaterFloat.Remove(rb);
		if (rigidbodiesInWaterSink.Contains(rb))
			rigidbodiesInWaterSink.Remove(rb);
		other.GetComponent<MovementStateMachine>()?.NormalMovement();
	}

	//sets drag on objects entering water
	//void OnTriggerEnter(Collider other)
	//{
	//	//rigidbody entered water?
	//	Rigidbody r = other.GetComponent<Rigidbody>();
	//	if(r)
	//	{
//
	//		//stop if we arent effecting player
	//		if (r.tag == "Player" && !effectPlayerDrag)
	//			return;
//
//
	//		//store objects default drag values
	//		//dragStore.Add (r.gameObject, r.drag);
	//		//angularStore.Add(r.gameObject, r.angularDrag);
////
	//		////apply new drag values to object
	//		//r.drag = resistance;
	//		//r.angularDrag = angularResistance;
	//	}
	//	else if(splashSound)
	//		AudioSource.PlayClipAtPoint(splashSound, other.transform.position);
	//}

	//reset drag on objects leaving water


	//Rigidbody r = other.GetComponent<Rigidbody>();
		//if(r)
		//{
		//	//stop if we arent effecting player
		//	if(r.CompareTag("Player") && !effectPlayerDrag)
		//		return;
//
//
		//	//see if we've stored this objects default drag values
		//	if (dragStore.ContainsKey(r.gameObject) && angularStore.ContainsKey(r.gameObject))
		//	{
		//		//restore values
		//		r.drag = dragStore[r.gameObject];
		//		r.angularDrag = angularStore[r.gameObject];
		//		//remove stored values for this object
		//		dragStore.Remove(r.gameObject);
		//		angularStore.Remove (r.gameObject);
		//	}
		//	else
		//	{
		//		//restore default values incase we cant find it in list (for whatever reason)
		//		r.drag = 0f;
		//		r.angularDrag = 0.05f;
		//		print ("Object left water: couldn't get drag values, restored to defaults");
		//	}


}
