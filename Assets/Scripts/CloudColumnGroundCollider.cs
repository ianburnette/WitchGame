using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudColumnGroundCollider : MonoBehaviour {

	[SerializeField] CloudColumnWorldCollider worldCollider;

	public bool detectingGround;

	void OnTriggerStay(Collider other) {
		worldCollider.detect = false;
		detectingGround = true;
	}

	void OnTriggerExit(Collider other) {
		worldCollider.detect = true;
		detectingGround = false;
	}

	void Update() {
		transform.rotation = Quaternion.identity;
	}
}
