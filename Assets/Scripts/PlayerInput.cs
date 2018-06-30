using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour {

    public delegate void JumpDelegate();
    public static event JumpDelegate OnJump;

    public delegate void JumpReleaseDelegate();
    public static event JumpReleaseDelegate OnJumpRelease;

    public delegate void MoveDelegate(Vector2 movement);
    public static event MoveDelegate OnMove;

    public delegate void GrabDelegate();
    public static event GrabDelegate OnGrab;

    public delegate void AttackDelegate();
    public static event AttackDelegate OnAttack;

    public delegate void MagicDelegate();
    public static event MagicDelegate OnMagic;

    [SerializeField] static float movementDeadZone = .1f;

    void Update() => GetPlayerInput();

    static void GetPlayerInput() {
        if (Input.GetButtonDown("Jump")) OnJump?.Invoke();
        if (Input.GetButtonUp("Jump")) OnJumpRelease?.Invoke();
        if (Input.GetButtonDown("Grab")) OnGrab?.Invoke();
        if (Input.GetButtonDown("Attack")) OnAttack?.Invoke();
        if (Input.GetButtonDown("Magic")) OnMagic?.Invoke();

        var movementValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        OnMove?.Invoke(movementValue.magnitude > movementDeadZone ? movementValue : Vector2.zero);
    }


}
