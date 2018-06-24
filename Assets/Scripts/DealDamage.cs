using UnityEngine;

public class DealDamage : MonoBehaviour
{
	private Health health;

	public void Attack(GameObject victim, int dmg, float pushHeight, float pushForce)
	{
		health = victim.GetComponent<Health>();

		Vector3 pushDir = (victim.transform.position - transform.position);
		pushDir.y = 0f;
		pushDir.y = pushHeight * 0.1f;
		if (victim.GetComponent<Rigidbody>() && !victim.GetComponent<Rigidbody>().isKinematic)
		{
			victim.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
			victim.GetComponent<Rigidbody>().AddForce (pushDir.normalized * pushForce, ForceMode.VelocityChange);
			victim.GetComponent<Rigidbody>().AddForce (Vector3.up * pushHeight, ForceMode.VelocityChange);
		}

		if(health && !health.flashing)
			health.currentHealth -= dmg;
	}
}
