using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudColumnWorldCollider : MonoBehaviour {

    [SerializeField] CloudColumn myCloudColumn;
    public bool detect;

    void OnTriggerEnter(Collider other) {
        if (!detect) return;
        myCloudColumn.HitWorld();
        print(other.transform.name);
    }

    void Update() {
        transform.rotation = Quaternion.identity;
    }
}
