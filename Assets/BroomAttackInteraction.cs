using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomAttackInteraction : MonoBehaviour
{
    [SerializeField] PlayerAttack atk;

    void OnTriggerEnter(Collider other) => atk.HitOther(other);
}
