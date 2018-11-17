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
   [SerializeField] Enemy enemy;
   [SerializeField] EnemyWaterInteraction waterInteraction;
   [SerializeField] Pickup pickup;

   [SerializeField] float stunTime;
   [SerializeField] float postHeldWakeTime;
   
   private EnemyState myState;
   public EnemyState customState;

   public EnemyState MyState
   {
      get => myState;
      set
      {
         customState = value;
         myState = value;
         switch (myState)
         {
            case EnemyState.Wander:
               ActivateEnemy(true);
               enemy.Wander();
               break;
            case EnemyState.Chase:
               ActivateEnemy(true);
               if (!waterInteraction.inWater)
                  enemy.Chase();
               else
                  MyState = EnemyState.Wander;
               break;
            case EnemyState.Stun:
               ActivateEnemy(false);
               StunEnemy();
               break;
            case EnemyState.Held:
               PickupEnemy();
               break;
            case EnemyState.Escape:
               break;
            default:
               throw new ArgumentOutOfRangeException();
         }
      }
   }

   void Start()
   {
      MyState = customState;
   }
      
   void SetActiveEnemy() => MyState = EnemyState.Wander;

   void Update()
   {
      if (MyState != customState)
         MyState = customState;
   }

   void ActivateEnemy(bool state)
   {
      enemy.enabled = state;
      waterInteraction.enabled = pickup.enabled = rb.useGravity = !state;
      enemy.tag = state ? "Enemy" : "Pickup";
   }

   void StunEnemy()
   {
      
      // TODO: some sort of flashing or indication of when they will get back up
      Invoke(nameof(SetActiveEnemy), stunTime);
   }

   void PickupEnemy()
   {
      CancelInvoke(nameof(SetActiveEnemy));
   }

   public void ThrowOrDropEnemy()
   {
      MyState = EnemyState.Stun;
      Invoke(nameof(SetActiveEnemy), postHeldWakeTime);
   }

   void EnemyEscape()
   {
      SetActiveEnemy();
     
   }
}
