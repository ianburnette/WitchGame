using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomethingEnterTrigger : MonoBehaviour
{
   [SerializeField] Umbrella umbrella;

   void OnTriggerEnter(Collider other) => umbrella.SomethingEnteredTrigger(other);
}
