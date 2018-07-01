﻿using System;
using System.Linq;
using JetBrains.Annotations;
using RootMotion.FinalIK;
using UnityEngine;

[RequireComponent(typeof(DealDamage))]
public class PlayerMoveBase : MonoBehaviour {
    public Transform mainCam, floorChecks;
    public Animator animator;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public MovementStateMachine movementStateMachine;

    public Transform[] floorCheckers;
    public GroundHitInfo[] groundInfo;

    public Vector3 slopeNormal;
    public float slopeAngle;

    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckOffset = .05f;

    public Rigidbody rigid;
    public Collider col;

    Quaternion screenMovementSpace;
    Vector3 screenMovementForward, screenMovementRight;

    public CharacterMotor characterMotor;

    [SerializeField] DealDamage dealDamage;
    EnemyAI currentEnemyAI;

    public bool currentlyGrounded;
    public GroundType myGroundType;

    void OnEnable() {
        col = GetComponent<Collider>();
        groundInfo = new GroundHitInfo[floorCheckers.Length];
    }

    void Update() => rigid.WakeUp();

    public bool IsGrounded(float distanceToCheck) {
        for (var i = 0; i < floorCheckers.Length; i++)
            groundInfo[i] = GroundChecking.GetGroundHitInfo(floorCheckers[i], distanceToCheck + groundCheckOffset, groundMask);
        if (EnemyBounceHit() != null) {
            var enemyTransform = EnemyBounceHit().transform;
            currentEnemyAI = enemyTransform.GetComponent<EnemyAI>();
            currentEnemyAI.BouncedOn();
            dealDamage.Attack(enemyTransform.gameObject, 1, 0f, 0f);
        }
        slopeNormal = AverageContactNormal();
        slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
        currentlyGrounded = PointsOfContact() != 0;
        return currentlyGrounded;
    }

    /*public GroundType CurrentGroundType() {
        //myGroundType = GroundType.None;
        var groundHits = 0;
        var cloudHits = 0;
        foreach (var t in groundInfo) {
            if (t.groundType == GroundType.Ground)
                groundHits++;
            if (t.groundType == GroundType.Cloud)
                cloudHits++;
        }

        if (groundHits > 0) myGroundType = GroundType.Ground;
        if (groundHits > 0) myGroundType = GroundType.Cloud;
        myGroundType = GroundType.None;
        return myGroundType;
    }*/

    public int PointsOfContact() => groundInfo.Count(t => t != null);

    /*
    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        if (groundInfo.Length == 0) return;
        foreach (var info in groundInfo)
            if (groundInfo!=null && info!=null)
                Gizmos.DrawSphere(info.position, .2f);
    }
    */

    [CanBeNull]
    public float DistanceToGround() {
        var averageDist = groundInfo.
                          Where(info => info != null).
                          Sum(info => Vector3.Distance(info.position, transform.position));
        return averageDist /= PointsOfContact();
    }

    [CanBeNull] GroundHitInfo EnemyBounceHit() =>
        groundInfo.Where(info => info != null).FirstOrDefault(info => info.transform.CompareTag("Enemy"));


    public Vector3 MovementRelativeToPlayerAndCamera(Vector2 input) =>
        transform.position + MovementRelativeToCamera(input);

    public Vector3 MovementRelativeToCamera(Vector2 input) {
        screenMovementSpace = Quaternion.Euler(0, mainCam.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;
        return (screenMovementForward * input.y) + (screenMovementRight * input.x);
    }

    public float SlopeAngle() => Vector3.Angle (slopeNormal, Vector3.up);

    static Vector3 CalculateSlopeNormal(Vector3 totalNormal, int rayCount) =>
        new Vector3(Mathf.Abs(totalNormal.x) > 0 ? totalNormal.x / rayCount : 0,
                    Mathf.Abs(totalNormal.y) > 0 ? totalNormal.y / rayCount : 0,
                    Mathf.Abs(totalNormal.z) > 0 ? totalNormal.z / rayCount : 0);

    Vector3 AverageContactNormal() {
        var summedNormal = groundInfo.Where(t => t != null).Aggregate(Vector3.zero, (current, t) => current + t.normal);
        return CalculateSlopeNormal(summedNormal, PointsOfContact());
    }

    public void OverrideYVelocity(float newYVel) =>
        rigid.velocity = new Vector3(rigid.velocity.x, newYVel, rigid.velocity.z);

    public float RigidbodyXZMagnitude(float min) {
        var mag = new Vector3(rigid.velocity.x, 0, rigid.velocity.z).magnitude;
        return mag > min ? mag : min;
    }
}