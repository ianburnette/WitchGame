using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        other.GetComponent<PlayerMove>().ToggleLadder(true);
    }

    void OnTriggerExit(Collider other) {
        other.GetComponent<PlayerMove>().ToggleLadder(false);
    }
}
