using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour {

    public delegate void JumpDelegate();
    public static event JumpDelegate OnJump;

    public delegate void MoveDelegate(Vector2 movement);
    public static event MoveDelegate OnMove;

    public delegate void GrabDelegate();
    public static event GrabDelegate OnGrab;

    [SerializeField] static float movementDeadZone = .1f;

    void Update() => GetPlayerInput();

    static void GetPlayerInput() {
        if (Input.GetButtonDown("Jump")) OnJump?.Invoke();
        if (Input.GetButtonDown("Grab")) OnGrab?.Invoke();

        var movementValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        OnMove?.Invoke(movementValue.magnitude > movementDeadZone ? movementValue : Vector2.zero);
    }


}
