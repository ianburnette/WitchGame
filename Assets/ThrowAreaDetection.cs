using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowAreaDetection : MonoBehaviour
{
    [SerializeField] private PlayerObjectInteraction objectInteraction;
    [SerializeField] private ObjectSnapZone currentZone;
    private void OnTriggerEnter(Collider other)
    {
        var thisZone = other.GetComponent<ObjectSnapZone>();
        if (thisZone == null) return;
        objectInteraction.CurrentZone = thisZone;
        thisZone.Highlighted = true;
        currentZone = thisZone;
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentZone == null) return;
        if (other.gameObject != currentZone.gameObject) return;
        objectInteraction.CurrentZone = null;
        currentZone.Highlighted = false;
        currentZone = null;
        //TODO: make sure to account for overlapping snap zones here.
    }
}
