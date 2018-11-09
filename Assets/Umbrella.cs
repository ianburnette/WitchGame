using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Umbrella : MonoBehaviour
{
    [SerializeField] private float force;

    private void OnTriggerEnter(Collider other)
    {
        var rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        rb.AddForce(force * transform.up, ForceMode.Impulse);
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerObjectInteraction>().AccidentallyLetGoOfPickup();
    }
}
