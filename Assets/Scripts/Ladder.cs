using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour {
    [SerializeField] Collider col;

    public Vector3 MinHeight { get { return col.bounds.min; } }
    public Vector3 MaxHeight { get { return col.bounds.max; } }

    void OnTriggerEnter(Collider other) {
        other.GetComponent<PlayerLadderInteraction>().ToggleLadder(true, this);
    }

    void OnTriggerExit(Collider other) {
        other.GetComponent<PlayerLadderInteraction>().ToggleLadder(false);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(transform.position.x, col.bounds.min.y, transform.position.z), .3f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(transform.position.x, col.bounds.max.y, transform.position.z), .3f);
    }
}
