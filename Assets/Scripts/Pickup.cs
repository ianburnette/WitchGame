﻿using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor.PackageManager;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickup : MonoBehaviour, ICloudInteractible, IVelocityLimiter {

    [Header("Mock Physics Variables")]
    [SerializeField] ObjectWeight weight;

    [Header("Cloud InteractionVariables")]
    [SerializeField] bool inCloud;
    [SerializeField] float cloudUpForce;

    [Header("Physics Variables")]
    [SerializeField] Rigidbody rb;

    [SerializeField] private float baseDrag, cloudDrag, snapDrag;
    [SerializeField] float baseAngularDrag, cloudAngularDrag;
    [SerializeField] private float dragThreshold = 3f; 

    [SerializeField] private float snapForce;
    [SerializeField] float maxMagnitude;

    [Header("Easing")] 
    [SerializeField] GoEaseType myEaseType;
    [SerializeField] float throwTime;
    
    public bool InCloud { get { return inCloud; }
        set {
            inCloud = value;
            rb.drag = inCloud ? cloudDrag : baseDrag;
            rb.angularDrag = inCloud ? cloudAngularDrag : baseAngularDrag;
        }
    }
    public ObjectWeight Weight{
        get { return weight; }
        set { weight = value; }}

    [field: Header("Slot Variables")]
    [field: SerializeField]
    public Vector3 SnapTargetPos { get; set; }

    Vector3 originPosition;

    void Start()
    {
        originPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        if (inCloud && rb != null && Weight == ObjectWeight.Light)
            rb.AddForce(Vector3.up * cloudUpForce);
        else if (SnapTargetPos != Vector3.zero)
            Snap();
        LimitVelocity();
    }

    public void LimitVelocity()
    {
        if (rb.velocity.magnitude > maxMagnitude)
            rb.velocity = rb.velocity.normalized * maxMagnitude;
    }

    private void Snap()
    {
        //rb.AddForce((SnapTargetPos - transform.position) * snapForce);
        //rb.drag = Vector3.Distance(SnapTargetPos, transform.position) < dragThreshold ? snapDrag : baseDrag;
    }

    public void EnterCloud() {
        if (!inCloud) CloudDamp();
    }

    public void SnapTo(ObjectSnapZone zone)
    {
        SnapTargetPos = zone.CurrentSnapSlot().pos + zone.transform.position;
        var position = transform.position;
        var points = new[]
        {
            position,
            ((position + SnapTargetPos) / 2) + (Vector3.up * (Vector3.Distance(position, SnapTargetPos) / 2)),
            SnapTargetPos
        };
        var path = new GoSpline(points);
        var config = new GoTweenConfig();
        config.positionPath(path, false);
        config.easeType = myEaseType;
        Go.to(transform, throwTime, config);
    }

    public void StayInCloud() => InCloud = true;
    public void ExitCloud() => InCloud = false;
    void CloudDamp() => rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

    public void Reset()
    {
        //TODO some sort of reset particles here
        transform.position = originPosition;
        rb.velocity = Vector3.zero;
    }
}

public enum ObjectWeight {
    Heavy,
    Light
}
