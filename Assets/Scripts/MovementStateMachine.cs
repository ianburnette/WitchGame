﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MoveState { Walk = 0, Glide, Hover, Swim, Ladder, Underwater, Invalid }

public class MovementStateMachine : MonoBehaviour {

	[SerializeField] Behaviour[] movementBehaviours;
	[SerializeField] Behaviour currentMovementBehavior;
	[SerializeField] PlayerAbilities playerAbilities;

	public MoveState CurrentMovementState {
		get {
			for (var i = 0; i<movementBehaviours.Length; i++)
				if (movementBehaviours[i] == currentMovementBehavior)
					return (MoveState)i;
			return MoveState.Invalid;
		}
		set {
			currentMovementBehavior = movementBehaviours[(int)value];
			foreach (var movementBehavior in movementBehaviours)
				movementBehavior.enabled = movementBehavior == currentMovementBehavior;
		}
	}

	public void GlideMovement() => CurrentMovementState = MoveState.Glide;
	public void HoverMovement() => CurrentMovementState = MoveState.Hover;
	public void NormalMovement() => CurrentMovementState = MoveState.Walk;
	public void GetOnLadder() => CurrentMovementState = MoveState.Ladder;
	public void GetInWater() => CurrentMovementState =
		                            playerAbilities.underwaterUnlocked ?
										MoveState.Underwater : MoveState.Swim;
	public void GetOutOfWater() => CurrentMovementState =
		                               CurrentMovementState == MoveState.Underwater ?
			                               MoveState.Hover : MoveState.Walk;
	public void GoUnderwater() => CurrentMovementState = MoveState.Underwater;
}


