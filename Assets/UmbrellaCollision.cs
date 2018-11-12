using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UmbrellaCollision : MonoBehaviour
{
    [SerializeField] Umbrella parent;

    void OnTriggerEnter(Collider other) => parent.SomethingEnteredTrigger(other);
}
