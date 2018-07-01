using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

    [SerializeField] PlayerAttack attack;

    void OnTriggerEnter(Collider col) {
        if (col.CompareTag("Enemy"))
            attack.HitOther(col);
    }

}
