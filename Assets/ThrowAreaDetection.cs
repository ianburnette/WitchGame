using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowAreaDetection : MonoBehaviour
{
    [SerializeField] private PlayerObjectInteraction objectInteraction;
    private void OnTriggerEnter(Collider other)
    {
        var snapZone = other.GetComponent<ObjectSnapZone>();
        if (snapZone == null) return;
        objectInteraction.CurrentSlot = snapZone.GetAvailableSnapSlot();
    }

    private void OnTriggerExit(Collider other)
    {
        //TODO: make sure to account for overlapping snap zones here.
        objectInteraction.CurrentSlot = null;
    }
}
