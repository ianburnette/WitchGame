using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickup : MonoBehaviour, ICloudInteractible {

    [Header("Mock Physics Variables")]
    [SerializeField] ObjectWeight weight;

    [Header("Cloud InteractionVariables")]
    [SerializeField] bool inCloud;
    [SerializeField] float cloudUpForce;

    [Header("Physics Variables")]
    [SerializeField] Rigidbody rb;
    [SerializeField] float baseDrag, cloudDrag;
    [SerializeField] float baseAngularDrag, cloudAngularDrag;

    [Header("Slot Variables")]
    [SerializeField] SnapSlot currentSlot;
    [SerializeField] private float snapForce;
    
    public bool InCloud { get { return inCloud; }
        set {
            inCloud = value;
            rb.drag = inCloud ? cloudDrag : baseDrag;
            rb.angularDrag = inCloud ? cloudAngularDrag : baseAngularDrag;
        }
    }

    public SnapSlot CurrentSlot
    {
        get { return currentSlot; }
        set { currentSlot = value; }
    }

    void OnEnable()
    {
        currentSlot = null;
    }

    void FixedUpdate() {
        if (inCloud && rb != null && weight == ObjectWeight.Light)
            rb.AddForce(Vector3.up * cloudUpForce);
        else if (CurrentSlot.pos != Vector3.zero)
            rb.AddForce((CurrentSlot.pos - transform.position) * snapForce);
        else if (rb == null)
            throw new Exception("Rigidbody not assigned for pickup");
    }

    public void EnterCloud() {
        if (!inCloud) CloudDamp();
    }

    public void SnapTo(SnapSlot slot) => CurrentSlot = slot;

    public void StayInCloud() => InCloud = true;
    public void ExitCloud() => InCloud = false;
    void CloudDamp() => rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
}

public enum ObjectWeight {
    Heavy,
    Light
}
