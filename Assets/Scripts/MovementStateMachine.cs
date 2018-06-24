using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MoveState { Walk, Glide, Hover, Swim, Ladder }

public class MovementStateMachine : MonoBehaviour {

	[SerializeField] Behaviour[] movementBehaviours;
	[SerializeField] Behaviour currentMovementState;

	public bool glidingUnlocked;
	public bool hoveringUnlocked;

	public Dictionary<MoveState, Behaviour> MovementBehaviourDictionary = new Dictionary<MoveState, Behaviour>();

	public MoveState CurrentMovementState {
		get {
			return currentMovementState;
		}
		set {
			switch (value) {
				case MoveState.Glide:
					currentMovementState = movementBehaviours.OfType<PlayerGlideMove>().First();
					break;
				case MoveState.Walk:
					currentMovementState = movementBehaviours.OfType<PlayerWalkMove>().First();
					break;
				case MoveState.Hover:
					currentMovementState = movementBehaviours.OfType<PlayerHoverMove>().First();
					break;
				case MoveState.Ladder:
					currentMovementState = movementBehaviours.OfType<PlayerLadderMove>().First();
					break;
				case MoveState.Swim:
					currentMovementState = movementBehaviours.OfType<PlayerWaterMove>().First();
					break;
			}
			currentMovementState = value;
			foreach (var moveBehaviour in movementBehaviours)
				moveBehaviour.enabled = moveBehaviour == value;
		}
	}

	/*
	public void GlideMovement() => CurrentMovementState = movementBehaviours.OfType<PlayerGlideMove>().First();

	public void HoverMovement() => CurrentMovementState = movementBehaviours.OfType<PlayerHoverMove>().First();

	public void NormalMovement() => CurrentMovementState = movementBehaviours.OfType<PlayerMove>().First();

	public void GetOnLadder() => CurrentMovementState = movementBehaviours.OfType<PlayerLadderMove>().First();

	public void GetInWater() => CurrentMovementState = movementBehaviours.OfType<PlayerSwim>().First();
	*/

	public void GlideMovement() => CurrentMovementState = MoveState.Glide;
	public void HoverMovement() => CurrentMovementState = MoveState.Hover;
	public void NormalMovement() => CurrentMovementState = MoveState.Walk;
	public void GetOnLadder() => CurrentMovementState = MoveState.Ladder;
	public void GetInWater() => CurrentMovementState = MoveState.Swim;
}


