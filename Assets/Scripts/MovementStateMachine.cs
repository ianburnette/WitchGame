using System.Linq;
using UnityEngine;

public class MovementStateMachine : MonoBehaviour {

	[SerializeField] Behaviour[] movementBehaviours;
	[SerializeField] Behaviour currentMovementState;

	public bool glidingUnlocked;
	public bool hoveringUnlocked;

	public Behaviour CurrentMovementState {
		get {
			return currentMovementState;
		}
		set {
			currentMovementState = value;
			foreach (var moveBehaviour in movementBehaviours)
				moveBehaviour.enabled = moveBehaviour == value;
		}
	}

	public void GlideMovement() => CurrentMovementState = movementBehaviours.OfType<PlayerGlideMove>().First();

	public void HoverMovement() => CurrentMovementState = movementBehaviours.OfType<PlayerHoverMove>().First();

	public void NormalMovement() => CurrentMovementState = movementBehaviours.OfType<PlayerMove>().First();

	public void GetOnLadder() => CurrentMovementState = movementBehaviours.OfType<PlayerLadderMove>().First();
}
