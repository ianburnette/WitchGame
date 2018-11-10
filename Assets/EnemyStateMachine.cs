using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.Serialization;

public enum EnemyState { Wander,Chase,Stun,Held,Escape }

public class EnemyStateMachine : MonoBehaviour
{
   [SerializeField] Rigidbody rb;
   [SerializeField] CharacterMotor motor;
   [SerializeField] EnemyAI ai;
   [SerializeField] Health health;
   [SerializeField] EnemyWaterInteraction waterInteraction;
   [SerializeField] Pickup pickup;

   [FormerlySerializedAs("currentMoveState")] [SerializeField] EnemyState currentState;

   [SerializeField] float stunTime;
   [SerializeField] float postHeldWakeTime;
   
   public EnemyState CurrentState
   {
      get => currentState;
      set
      {
         currentState = value;
         ActivateState(currentState);
      }
   }

   public void Wander() => CurrentState = EnemyState.Wander;
   public void Chase() => CurrentState = EnemyState.Chase;
   public void Stun() => CurrentState = EnemyState.Stun;
   public void Escape() => CurrentState = EnemyState.Escape;

   void ActivateState(EnemyState state)
   {
      switch (state)
      {
         case EnemyState.Wander: 
            SetActiveEnemy();
            break;
         case EnemyState.Chase:
            SetActiveEnemy();
            break;
         case EnemyState.Stun:
            StunEnemy();
            break;
         case EnemyState.Held:

            break;
         case EnemyState.Escape:
            break;
         default:
            throw new ArgumentOutOfRangeException();
      }
   }

   void SetActiveEnemy()
   {
      rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                       RigidbodyConstraints.FreezeRotationY |
                       RigidbodyConstraints.FreezeRotationZ;
      motor.enabled = ai.enabled = health.enabled = true;
      waterInteraction.enabled = pickup.enabled = false;
   }

   void StunEnemy()
   {
      rb.constraints = RigidbodyConstraints.None;
      motor.enabled = ai.enabled = health.enabled = false;
      waterInteraction.enabled = pickup.enabled = true;
      // TODO: some sort of flashing or indication of when they will get back up
      Invoke(nameof(SetActiveEnemy), stunTime);
   }

   void PickupEnemy()
   {
      CancelInvoke(nameof(SetActiveEnemy));
   }

   void ThrowOrDropEnemy()
   {
      Invoke(nameof(SetActiveEnemy), postHeldWakeTime);
   }

   void EnemyEscape()
   {
      SetActiveEnemy();
      ai.Flee = true;
   }
}
