using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Crab : Enemy
{
    protected void Move()
    {
        
    }

}

public class Enemy : MonoBehaviour
{
    [Header("General Behavior")]
    [SerializeField] Vector3 targetPosition;
    
    [Header("Physics Behavior")]
    [SerializeField] Rigidbody rb;
    
    [Header("Hover Behavior")]
    [SerializeField] LayerMask hoverOnLayer;
    [SerializeField] float maxGroundDist;

    [SerializeField] AnimationCurve hoverCurve;
    [SerializeField] float hoverForce;
    [SerializeField] float hoverForceSmootheSpeed;
    [SerializeField] float heightDestinationOffset;
    [SerializeField] float minHoverForceThreshold = .01f;
    public Vector3 currentForce;
    public float currentCurveEval;
    public float toEval;
    public float diff;
    public float dist;
    
    
    [Header("Chase Behavior")]
    [SerializeField] float chaseLookSpeed;
    [SerializeField] float chaseTargetUpdateTime;
    [SerializeField] Transform target;
    
    [Header("Movement Behavior")]
    [SerializeField] float chaseSpeed;
    [SerializeField] float wanderSpeed;
    [FormerlySerializedAs("timeBetweenWanderPulses")] [SerializeField] float minTimeBetweenWanderPulses;
    [FormerlySerializedAs("timeBetweenChasePulses")] [SerializeField] float minTimeBetweenChasePulses;
    
    [FormerlySerializedAs("timeBetweenWanderEvents")]
    [Header("Wander Behavior")]
    [SerializeField] internal float minTimeBetweenWanderEvents = 5f;
    [SerializeField] float wanderLookSpeed;
    [SerializeField] float wanderDistanceRadius;
    [SerializeField] Vector3 initialPosition;

    [SerializeField] EnemyStateMachine stateMachine;
    

    IEnumerator ChaseWaypointUpdate()
    {
        while (stateMachine.MyState == EnemyState.Chase)
        {
            yield return new WaitForSeconds(chaseTargetUpdateTime);
            targetPosition = target.position;

        }
    }

    void Start()
    {
        initialPosition = transform.position;
    }

    void Move() => 
        rb.AddForce(transform.forward * (stateMachine.MyState == EnemyState.Chase ? chaseSpeed : wanderSpeed), ForceMode.Impulse);

    public void Wander()
    {
        
        StartCoroutine(WanderWaypointSelection());
        StartCoroutine(WanderMovement());
    }

    public void Chase()
    {
        
        StartCoroutine(ChaseWaypointUpdate());
        StartCoroutine(ChaseMovement());
    }
    
    void Hover()
    {
        var dt = Time.deltaTime;
        dist = DistanceToGround();
        diff = DestinationDifference() - heightDestinationOffset;
        toEval = Mathf.Clamp(diff + dist, 0, 500);
        var force = hoverCurve.Evaluate(toEval);
        currentCurveEval = force;
        var adjustedForce = Vector3.up * (force * hoverForce * dt);
        currentForce = Vector3.Lerp(currentForce, adjustedForce, hoverForceSmootheSpeed * dt);
        rb.AddForce(currentForce, ForceMode.Force);
    }

    float DestinationDifference() => transform.position.y - targetPosition.y; //-1 means target is above

    float DistanceToGround()
    {
        var ray = new Ray(transform.position, Vector3.down * maxGroundDist);
        Debug.DrawRay(ray.origin, ray.direction, Color.red);
        Physics.Raycast(ray, out var hit, maxGroundDist, hoverOnLayer);
        if (hit.transform != null && hit.distance < maxGroundDist)
            return Mathf.Clamp(hit.distance, 0, maxGroundDist);
        return 500;
    }

    void Update()
    {
        if (stateMachine.MyState == EnemyState.Chase || stateMachine.MyState == EnemyState.Wander)
            Hover();
    }

    void FixedUpdate() => LookAtTargetPosition();

    void LookAtTargetPosition()
    {
        var dir = targetPosition - transform.position;
        var toRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation,
            stateMachine.MyState == EnemyState.Chase ? chaseLookSpeed  * Time.deltaTime : wanderLookSpeed * Time.deltaTime);   
    }

    IEnumerator ChaseMovement()
    {
        while (stateMachine.MyState == EnemyState.Chase)
        {
            if (!target)
                stateMachine.MyState = EnemyState.Wander;
            yield return new WaitForSeconds(Random.Range(minTimeBetweenChasePulses, minTimeBetweenChasePulses * 2));
            Move();
        }
    }
    
    IEnumerator WanderMovement()
    {
        while (stateMachine.MyState == EnemyState.Wander)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWanderPulses, minTimeBetweenWanderPulses * 2));
            Move();
        }
    }

    IEnumerator WanderWaypointSelection()
    {
        while (stateMachine.MyState == EnemyState.Wander)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWanderEvents, minTimeBetweenWanderEvents * 2));
            FindNewWanderWaypoint();
        }
    }

    void FindNewWanderWaypoint()
    {
        var potentialNewTargetPoint = initialPosition + new Vector3(RandomPoint(),0, RandomPoint());

        var hitInfo = new RaycastHit();    
        Physics.SphereCast(potentialNewTargetPoint, .2f, Vector3.up, out hitInfo, 1f);
        if (!hitInfo.transform)
            targetPosition = potentialNewTargetPoint;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(targetPosition, .4f);
    }

    float RandomPoint() => Random.Range(-wanderDistanceRadius, wanderDistanceRadius);

    public void Flee()
    {
        
    }
}
