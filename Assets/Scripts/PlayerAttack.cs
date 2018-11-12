using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] Animator anim;
    [SerializeField] DealDamage damage;

    [SerializeField] int pushHeight;
    [SerializeField] int pushForce;

    [SerializeField] private PlayerWalkMove walkMove;
    [SerializeField] Collider weaponCollider;

    private bool attacking;
    public bool firstAtkFinished, secondAtkFinished, thirdAtkFinished;
    
    private static readonly int SecondAttack = Animator.StringToHash("SecondAttack");
    private static readonly int ThirdAttack = Animator.StringToHash("ThirdAttack");
    private static readonly int FirstAttack = Animator.StringToHash("FirstAttack");

    void OnEnable()
    {
        PlayerInput.OnAttack += AttackPressed;
        PlayerInput.OnJump += ResetAllAttacks;
        PlayerInput.OnBroom += ResetAllAttacks;
    }

    void OnDisable()
    {
        PlayerInput.OnAttack -= AttackPressed;
        PlayerInput.OnJump -= ResetAllAttacks;
        PlayerInput.OnBroom -= ResetAllAttacks;
    }

    public void AttackPressed()
    {
        attacking = true;
        walkMove.Attacking = true;
        if (thirdAtkFinished)
        {
            anim.SetBool(SecondAttack, false);
            anim.SetBool(ThirdAttack, false);
            anim.SetBool(FirstAttack, true);
        }
        else if (anim.GetBool(SecondAttack) && !secondAtkFinished)
            anim.SetBool(ThirdAttack, true);
        if (anim.GetBool(FirstAttack) && !firstAtkFinished)
            anim.SetBool(SecondAttack, true);
        else if (!firstAtkFinished && !secondAtkFinished && !thirdAtkFinished)
            anim.SetBool(FirstAttack, true);
    }

    public void HitOther(Collider other) {
        if (other.CompareTag("Enemy"))
            damage.Attack(other.gameObject, 1, pushHeight, pushForce);
        else if (other.CompareTag("Interactable"))
            damage.Interact(other.gameObject);
        else if (other.CompareTag("Environment"))
            Debug.Log("hit environment - set up ik?");
        else if (other.CompareTag("Untagged"))
            Debug.Log("hit something untagged");
        else
            Debug.LogError("Not prepared to hit object with tag " + other.tag + "...it's name is " +
                           other.transform.name);
            //throw new System.NotImplementedException();
    }

    public void FinishedAtk(int attackNumber)
    {
        switch (attackNumber)
        {
            case 1:
                firstAtkFinished = true;
                if (!anim.GetBool(SecondAttack))
                    ResetAllAttacks();
                break;
            case 2:
                secondAtkFinished = true;
                if (!anim.GetBool(ThirdAttack))
                    ResetAllAttacks();
                break;
            case 3:
                ResetAllAttacks();
                break;
        }
    }

    private void ResetAllAttacks()
    {
        anim.SetLayerWeight(2, 1);
        anim.SetBool(FirstAttack, false);
        anim.SetBool(SecondAttack, false);
        anim.SetBool(ThirdAttack, false);
        firstAtkFinished = secondAtkFinished = thirdAtkFinished = false;
        walkMove.Attacking = false;
    }
}
