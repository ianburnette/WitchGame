using System.Linq;
using UnityEngine;

public class MovementStateMachine : MonoBehaviour {

	[SerializeField] Behaviour[] movementBehaviours;
	[SerializeField] Behaviour currentMovementState;

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

	public void OnBroom() => CurrentMovementState = movementBehaviours.OfType<PlayerBroomMove>().First();

	public void NormalMovement() => CurrentMovementState = movementBehaviours.OfType<PlayerMove>().First();

	public void GetOnLadder() => CurrentMovementState = movementBehaviours.OfType<PlayerLadderMove>().First();
}
