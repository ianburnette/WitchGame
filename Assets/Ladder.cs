using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour {
    Collider col;

    public float MinHeight { get { return col.bounds.min.y; } }
    public float MaxHeight { get { return col.bounds.max.y; } }

    void Awake() {
        col = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other) {
        other.GetComponent<PlayerMove>().ToggleLadder(true, this);
    }

    void OnTriggerExit(Collider other) {
        other.GetComponent<PlayerMove>().ToggleLadder(false);
    }

    void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position + Vector3.up * MinHeight, .3f);
        Gizmos.DrawSphere(transform.position + Vector3.up * MaxHeight, .3f);
    }
}
