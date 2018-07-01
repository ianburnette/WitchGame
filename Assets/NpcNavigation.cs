using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcNavigation : MonoBehaviour {
    [SerializeField] Vector3[] destinations;
    [SerializeField] float updateTime;
    [SerializeField] float distanceMargin;
    [SerializeField] float moveSpeed;

    public Vector3 currentDestination;
    public NavMeshAgent agent;

    void Start() => InvokeRepeating(nameof(CheckState), updateTime, updateTime);

    void CheckState() {
        if (Vector3.Distance(transform.position, currentDestination) < distanceMargin)
            SelectNewDestination();
        agent.destination = currentDestination;
    }

    void SelectNewDestination() {
        if (destinations.Length == 1)
            currentDestination = destinations[0];
        if (destinations.Length == 0)
            currentDestination = transform.position;
        var candidateDestination = currentDestination;
        while (currentDestination == candidateDestination)
            currentDestination = SelectRandomDestination();
    }

    Vector3 SelectRandomDestination() => destinations[Random.Range(0, destinations.Length - 1)];

    void OnTriggerEnter(Collider other) => agent.speed = 0;
    void OnTriggerExit(Collider other) => agent.speed = moveSpeed;

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach (var destination in destinations) Gizmos.DrawCube(destination, Vector3.one / 3);
    }
}
