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
    
    private EnemyState myState;

    public EnemyState customState;
    
    [SerializeField]
    public EnemyState MyState
    {
        get => myState;
        set
        {
            myState = value;
            switch (myState)
            {
                case EnemyState.Wander:
                    StartCoroutine(WanderWaypointSelection());
                    StartCoroutine(WanderMovement());
                    break;
                case EnemyState.Chase:
                    StartCoroutine(ChaseWaypointUpdate());
                    StartCoroutine(ChaseMovement());
                    break;
                case EnemyState.Stun:
                    break;
                case EnemyState.Held:
                    break;
                case EnemyState.Escape:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    IEnumerator ChaseWaypointUpdate()
    {
        while (MyState == EnemyState.Chase)
        {
            yield return new WaitForSeconds(chaseTargetUpdateTime);
            targetPosition = target.position;

        }
    }

    void Start() => initialPosition = transform.position;
    
    void Move() => 
        rb.AddForce(transform.forward * (MyState == EnemyState.Chase ? chaseSpeed : wanderSpeed), ForceMode.Impulse);

    void Update()
    {
        if (MyState != customState)
            MyState = customState;    
    }

    void FixedUpdate() => LookAtTargetPosition();

    void LookAtTargetPosition()
    {
        var dir = targetPosition - transform.position;
        var toRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation,
            MyState == EnemyState.Chase ? chaseLookSpeed  * Time.deltaTime : wanderLookSpeed * Time.deltaTime);   
    }

    IEnumerator ChaseMovement()
    {
        while (MyState == EnemyState.Chase)
        {
            if (!target)
                MyState = EnemyState.Wander;
            yield return new WaitForSeconds(Random.Range(minTimeBetweenChasePulses, minTimeBetweenChasePulses * 2));
            Move();
        }
    }
    
    IEnumerator WanderMovement()
    {
        while (MyState == EnemyState.Wander)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWanderPulses, minTimeBetweenWanderPulses * 2));
            Move();
        }
    }

    IEnumerator WanderWaypointSelection()
    {
        while (MyState == EnemyState.Wander)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWanderEvents, minTimeBetweenWanderEvents * 2));
            FindNewWanderWaypoint();
        }
    }

    void FindNewWanderWaypoint()
    {
        var previousPosition = targetPosition;
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
