using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : MonoBehaviour
{
    [SerializeField] private PlayerAttack atk;
    public void FinishedAttack(int attackNumber)
    {
        atk.FinishedAtk(attackNumber);
    }
}
