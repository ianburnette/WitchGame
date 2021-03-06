﻿using System;
using System.Linq;
using Cinemachine;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(DealDamage))]
public class PlayerMoveBase : MonoBehaviour {
    public Transform mainCam, floorChecks;
    public Animator animator;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public MovementStateMachine movementStateMachine;
    public PlayerAbilities playerAbilities;

    public Transform[] floorCheckers;
    public GroundHitInfo[] groundInfo;

    public Vector3 slopeNormal;
    public float slopeAngle;

    [SerializeField] float groundCheckOffset = .05f;

    public Rigidbody rigid;
    public Collider col;
    [SerializeField] PhysicMaterial frictionPlayerMat, frictionlessPlayerMat;
    [FormerlySerializedAs("rbVel")] public float rbVelMagnitude;

    Quaternion screenMovementSpace;
    Vector3 screenMovementForward, screenMovementRight;

    public CharacterMotor characterMotor;
    public PlayerCameraReference camReferenceTransform;

    [SerializeField] DealDamage dealDamage;
    EnemyAI currentEnemyAi;

    public bool ResetCamOnGrounded { get; set; }
    public float camLockResetTime = .4f;
    public bool currentlyGrounded;
    public GroundType myGroundType;

    [Header("Slope Matching")] 
    [SerializeField] float slopeMatchSpeed;
    public bool matchSlope;
    [SerializeField] Transform model;
    [SerializeField] Vector3 upVector;
    [SerializeField] float slopeLookForwardSpeed;
    [SerializeField] bool lookForward;

    void OnEnable() {
        col = GetComponent<Collider>();
        groundInfo = new GroundHitInfo[floorCheckers.Length];
    }

    void Update()
    {
        rigid.WakeUp();
        rbVelMagnitude = rigid.velocity.magnitude;
        if (matchSlope)
            MatchSlope();
    }

    public bool IsGrounded(float distanceToCheck, LayerMask groundMask) {
        for (var i = 0; i < floorCheckers.Length; i++)
            groundInfo[i] = GroundChecking.GetGroundHitInfo(floorCheckers[i], distanceToCheck + groundCheckOffset, groundMask);
        if (EnemyBounceHit() != null) {
            var enemyTransform = EnemyBounceHit().transform;
            currentEnemyAi = enemyTransform.GetComponent<EnemyAI>();
            currentEnemyAi.BouncedOn();
            dealDamage.Attack(enemyTransform.gameObject, 1, 0f, 0f);
        }
        slopeNormal = AverageContactNormal();
        slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
        currentlyGrounded = PointsOfContact() != 0;
        if (currentlyGrounded && ResetCamOnGrounded)
            Invoke(nameof(LockCamNow), camLockResetTime);
        if (!currentlyGrounded)
            CancelInvoke(nameof(LockCamNow));
        col.material = currentlyGrounded ? frictionPlayerMat : frictionlessPlayerMat;
        return currentlyGrounded;
    }

    public int PointsOfContact() => groundInfo.Count(t => t != null);

    
   //void OnDrawGizmos() {
   //    Gizmos.color = Color.green;
   //    if (groundInfo.Length == 0) return;
   //    foreach (var info in groundInfo)
   //        if (groundInfo!=null && info!=null)
   //            Gizmos.DrawSphere(info.position, .2f);
   //}
    

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

    public Vector3 PlanarVelocity() => new Vector3(rigid.velocity.x, 0, rigid.velocity.z);

    public void LockCamAndResetOnGround()
    {
        camReferenceTransform.LockToPlayer = true;
        ResetCamOnGrounded = true;
    }

    void LockCamNow() => camReferenceTransform.LockToPlayer = false;

    public void MatchSlopeAngle(bool state, bool onBack=false, float speed=0, bool lookForward=false)
    {
        matchSlope = state;
        slopeMatchSpeed = speed;
        upVector = onBack ? model.transform.forward : model.transform.up;
        if (!state)
        {
            model.localRotation = Quaternion.Euler(Vector3.zero);
            transform.rotation =
                Quaternion.Euler(0, transform.eulerAngles.y, 0);
        }
        this.lookForward = lookForward;
    }

    void MatchSlope()
    {
        var newRotation = Quaternion.FromToRotation(upVector, slopeNormal) * transform.rotation; 
        model.rotation = Quaternion.Lerp(model.rotation, newRotation, slopeMatchSpeed * Time.deltaTime);
        if (lookForward)
            characterMotor.RotateToVelocity(slopeLookForwardSpeed, false);
    }
}
