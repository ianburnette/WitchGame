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

        PlayerInput.OnMove += Move;
    }

    void OnDisable() {
        PlayerInput.OnAttack -= AttackPressed;

        PlayerInput.OnMove -= Move;
    }

    public void AttackPressed() {
        anim.SetTrigger("Attack");
    }

    public void HitOther(Collider other) {
        damage.Attack(other.gameObject, 1, pushHeight, pushForce);
    }

    void Move(Vector2 movement) {

    }
}
