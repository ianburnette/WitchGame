using UnityEngine;

public class DealDamage : MonoBehaviour
{
	Health health;

	public void Attack(GameObject victim, int dmg, float pushHeight, float pushForce)
	{
		health = victim.GetComponent<Health>();

		var pushDir = (victim.transform.position - transform.position);
		pushDir.y = 0f;
		pushDir.y = pushHeight * 0.1f;

		Push(victim, pushDir, new Vector2(pushForce, pushHeight));
			
		if(health && !health.flashing)
			health.currentHealth -= dmg;
	}

	public void HitEnemy(GameObject enemy, float pushHeight, float pushForce)
	{
		enemy.GetComponent<EnemyStateMachine>().MyState = EnemyState.Stun;
		var pushDir = (enemy.transform.position - transform.position);
		pushDir.y = pushHeight * 0.1f;
		Push(enemy, pushDir, new Vector2(pushForce, pushHeight));
	}

	public void Attack(GameObject victim, int dmg, float pushHeight, float pushForce, Vector3 hitPosition)
	{
		var pushDir = -victim.transform.forward;
		pushDir.y = pushHeight;

		Push(victim, pushDir, new Vector2(pushForce, pushHeight));
	}

	void Push(GameObject pushTarget, Vector3 pushDirection, Vector2 pushForce)
	{
		var rb = pushTarget.GetComponent<Rigidbody>();
		if (!rb || rb.isKinematic) return;
		rb.velocity = new Vector3(0, 0, 0);
		rb.AddForce (pushDirection.normalized * pushForce.x, ForceMode.VelocityChange);
		rb.AddForce (Vector3.up * pushForce.y, ForceMode.VelocityChange);
	}

	public void Interact(GameObject toInteractWith) => toInteractWith.GetComponent<IInteractable>().Interact();
}
