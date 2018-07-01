using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNodes : MonoBehaviour {
    [SerializeField] NodeSystem nodeSystem;

    void Update() {
        transform.position = nodeSystem.WeightedPosition;
    }
}
