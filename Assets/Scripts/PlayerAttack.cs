using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] Animator anim;
    [SerializeField] DealDamage damage;

    [SerializeField] int pushHeight;
    [SerializeField] int pushForce;

    void OnEnable() {
        PlayerInput.OnAttack += AttackPressed;
    }

    void OnDisable() {
        PlayerInput.OnAttack -= AttackPressed;
    }

    public void AttackPressed() {
        anim.SetTrigger("Attack");
    }

    public void HitOther(Collider other) {
        damage.Attack(other.gameObject, 1, pushHeight, pushForce);
    }
}
