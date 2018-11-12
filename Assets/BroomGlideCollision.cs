using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomGlideCollision : MonoBehaviour
{
   [SerializeField] PlayerGlideMove parent;

   void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Environment"))
         parent.SomethingEnteredTrigger(other);
   }
}