using UnityEngine;

[ExecuteInEditMode]
public class Cloud : CloudBase {
	public Vector3 Pos { get { return transform.position; } }
	public float Scale { get { return transform.localScale.magnitude; } }

	void OnTriggerEnter(Collider other) => other.GetComponent<ICloudInteractible>().EnterCloud();

	void OnTriggerStay(Collider other) => other.GetComponent<ICloudInteractible>().StayInCloud();

	void OnTriggerExit(Collider other) => other.GetComponent<ICloudInteractible>().ExitCloud();

	public Vector3 Grow(float speed) {
		transform.localScale += Vector3.one * speed * Time.deltaTime;
		return transform.localScale;
	}
}
