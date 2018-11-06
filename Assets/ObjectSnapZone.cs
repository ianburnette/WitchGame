using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectSnapZone : MonoBehaviour
{
    [FormerlySerializedAs("snapPositions")] [SerializeField] private List<SnapSlot> snapSlots;
    public bool eligible;
    public bool Highlighted { get; set; }
    
    private void OnDrawGizmos()
    {
        if (!Highlighted) return;
        Gizmos.color = Color.red;
        foreach (var slot in snapSlots)
        {
            Gizmos.color += Color.gray/2;
            Gizmos.DrawSphere(transform.position + slot.pos, .2f);
        }
    }

    public void EligibleForPlacement(bool state)
    {
        eligible = state;
    }

    public SnapSlot GetAvailableSnapSlot()
    {
        foreach (var slot in snapSlots)
        {
            if (slot.objectInSlot == null)
                return slot;
        }
        return null;
    }

    // TODO: this isn't gonna work long term.
    public SnapSlot CurrentSnapSlot()
    {
        return snapSlots[0];
    }
}

[System.Serializable]
public class SnapSlot
{
    public GameObject objectInSlot;
    public Vector3 pos;
}