using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour {

    public delegate void JumpDelegate();
    public static event JumpDelegate OnJump;

    public delegate void MoveDelegate(Vector2 movement);
    public static event MoveDelegate OnMove;
    [SerializeField] const float MovementThreshold = .1f;

    void Update() => GetPlayerInput();

    static void GetPlayerInput() {
        if (Input.GetButtonDown("Jump")) OnJump?.Invoke();

        var movementValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (movementValue.magnitude > MovementThreshold) OnMove?.Invoke(movementValue);
    }


}
