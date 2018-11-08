using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider))]
public class Water : MonoBehaviour
{
	public AudioClip splashSound;						//played when objects enter water
	public Vector3 force = new Vector3(0, 16.5f, 0);	//pushForce of the water. This is a vector3 so you can have force in any direction, for example a current or river
	public bool effectPlayerDrag;						//should the players rigidbody be effected by the drag/angular drag values of the water?
	public float resistance = 0.4f;						//the drag applied to rigidbodies in the water (but not player)
	public float angularResistance = 0.2f;				//the angular drag applied to rigidbodies in the water (but not player)

	[FormerlySerializedAs("waterInteractions")] 
	public List<IWaterInteraction> floatingInWater = new List<IWaterInteraction>();
	public List<IWaterInteraction> sinkingInWater = new List<IWaterInteraction>();

	Collider col;

	[SerializeField] float maxDepth = .4f;
	[SerializeField] float surfaceOffset = -0.4f;


	[SerializeField] float maxUpwardCorrectiveSpeed, maxDownwardCorrectiveSpeed;
	[SerializeField] float depthCorrectionSpeed = 2f;
	[SerializeField] float particleSpeedMult = 2f;

	[SerializeField] ParticleSystem particles;
	[SerializeField] private Vector3 depthCorrectionForce;

	//Dictionary<GameObject, float> dragStore = new Dictionary<GameObject, float>();
	//Dictionary<GameObject, float> angularStore = new Dictionary<GameObject, float>();


	void OnEnable() {
		col = GetComponent<Collider>();
		if (!particles) return;
		var particlesMain = particles.main;
		var particleForce = new Vector3(force.x, 0, force.z);
		particlesMain.startSpeed = particleForce.magnitude * particleSpeedMult;
	}

	void FixedUpdate()
	{
		floatingInWater?.ForEach(ApplyFloatingForce);
		sinkingInWater?.ForEach(ApplySinkingForce);
	}

	void ApplyFloatingForce(IWaterInteraction waterInteraction) => 
		waterInteraction.FloatForce(GetWaterForce(waterInteraction), ForceMode.Force);

	Vector3 GetWaterForce(IWaterInteraction waterInteraction) =>
		force + SurfaceCorrection(waterInteraction.Height, waterInteraction.Offset);

	void ApplySinkingForce(IWaterInteraction waterInteraction) => 
		waterInteraction.SinkForce(GetWaterForce(waterInteraction), ForceMode.Force);

	Vector3 SurfaceCorrection(float height, float offset) {
		var depth = Depth(height, offset);
		depthCorrectionForce = Vector3.up * Mathf.Clamp(depth * depthCorrectionSpeed,
										   maxDownwardCorrectiveSpeed,
										   maxUpwardCorrectiveSpeed);
		return depthCorrectionForce;
		//return depth > maxDepth ? depth * depthCorrectionSpeed : 1;
	}

	float Depth(float baseHeight, float offset)
	{
		var res = (HeightOfWaterSurface - offset) - baseHeight;
		return res;
		//Debug.DrawRay(new Vector3(debugPos.x, baseHeight, debugPos.z), Vector3.left, Color.yellow);
		//Debug.DrawRay(new Vector3(debugPos.x, HeightOfWaterSurface - surfaceOffset, debugPos.z), Vector3.right, Color.green);
		//Debug.DrawRay(new Vector3(debugPos.x, baseHeight, debugPos.z), Vector3.up * res, Color.magenta);
	}

	float HeightOfWaterSurface => col.bounds.extents.y;

	void OnTriggerEnter(Collider other)
	{
		var waterInteraction = other.GetComponent<IWaterInteraction>();
		var waterInteractionState = waterInteraction?.OnWaterEnter();
		if (waterInteraction == null) return;
		var toAssign = waterInteractionState == WaterInteractionState.Floating
			? floatingInWater
			: sinkingInWater;
		toAssign.Add(waterInteraction);
	}

	void OnTriggerExit(Collider other)
	{
		var waterInteraction = other.GetComponent<IWaterInteraction>();
		if (waterInteraction != null)
		{
			floatingInWater.Remove(waterInteraction);
			sinkingInWater.Remove(waterInteraction);
		}
		waterInteraction?.OnWaterExit();
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
