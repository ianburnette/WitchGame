using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
  [SerializeField] bool evasion;
  
  void OnTriggerEnter(Collider other)
  {
    if (evasion) return;
    var enemy = other.GetComponent<EnemyStateMachine>();
    if (enemy.MyState == EnemyState.Wander)
      other.GetComponent<EnemyStateMachine>().MyState = EnemyState.Chase;
  }

  void OnTriggerExit(Collider other)
  {
    if (!evasion) return;
    var enemy = other.GetComponent<EnemyStateMachine>();
    if (enemy.MyState == EnemyState.Chase)
      enemy.MyState = EnemyState.Wander;

  }
}
