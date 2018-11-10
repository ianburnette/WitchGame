using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : MonoBehaviour
{
    [SerializeField] private PlayerAttack atk;
    public Animator anim;

    public void FinishedAttack(int attackNumber) => atk.FinishedAtk(attackNumber);

    public void GrabBroom(int state)
    {
        anim.SetLayerWeight(2, state == 1 ? 0 : 1);
    }
}
