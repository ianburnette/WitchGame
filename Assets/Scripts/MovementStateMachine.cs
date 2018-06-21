using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementStateMachine : MonoBehaviour {
	[SerializeField] PlayerMove playerMove;
	[SerializeField] PlayerBroomMove playerBroom;
	[SerializeField] Behaviour currentMovementState;
	public Behaviour CurrentMovementState {
		get {
			return currentMovementState;
		}
		set {
			currentMovementState = value;
			if (currentMovementState == playerBroom)
				playerMove.enabled = false;
			else
				playerBroom.enabled = false;
		}
	}

	public void ChangeState(Behaviour from) {
		CurrentMovementState = from == playerMove ? (Behaviour)playerBroom : playerMove;
		from.enabled = false;
		CurrentMovementState.enabled = true;
	}
}
