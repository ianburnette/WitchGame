using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerLadderMove : MonoBehaviour {

    [Header("Movement Behavior")]
    [SerializeField] float climbSpeed;

    [Header("Ledge Interaction Behavior")]
    [SerializeField] float ledgeJumpMultiplier = .5f;
    [SerializeField] float ledgeForwardMultiplier = 1f;

    [Header("Jump Off Behavior")]
    [SerializeField] float offLaddJumpMultiplier = 1f;
    [SerializeField] float offLadderForwardMultiplier = 2f;

    [Header("Drop Behavior")]
    [SerializeField] float ladderDropForwardMultiplier = .4f;
    [SerializeField] float ladderDropJumpMultiplier = -.4f;

    [Header("Passive Behavior")]
    public float ladderTopJumpLeniancyDistance = 2f;

    [Header("Reference Variables")]
    public Vector3 amt;
    public Ladder ladder;

    [Header("Class References")]
    [SerializeField] PlayerMoveBase MoveBase;
    [SerializeField] PlayerWalkMove playerWalkMove;

    void OnEnable() {
        PlayerInput.OnMove += Move;
        PlayerInput.OnJump += Jump;
        PlayerInput.OnGrab += Grab;
        MoveBase.rigid.isKinematic = true;
    }

    void Update() {
        transform.rotation = RotateAngle180(ladder.transform.rotation);
    }

    void OnDisable() {
        PlayerInput.OnMove -= Move;
        PlayerInput.OnJump -= Jump;
        PlayerInput.OnGrab -= Grab;
        MoveBase.rigid.isKinematic = false;
    }

    void Move(Vector2 movementInput) {
        if ((movementInput.y > 0 && transform.position.y < ladder.transform.position.y + ladder.MaxHeight))
            MoveBase.characterMotor.MoveVertical(Vector3.up * movementInput.y * climbSpeed * Time.deltaTime);
        if((movementInput.y < 0 && transform.position.y > ladder.transform.position.y + ladder.MinHeight))
            MoveBase.characterMotor.MoveVertical(Vector3.up * movementInput.y * climbSpeed * Time.deltaTime);
    }

    void Jump() {
        if (Vector3.Distance(transform.position, ladder.transform.position + Vector3.up * ladder.MaxHeight) <
            ladderTopJumpLeniancyDistance)
            JumpToLedge();
        else
            JumpOff();
    }

    void Grab() {
        MoveBase.movementStateMachine.NormalMovement();
        playerWalkMove.JumpInDirection(-ladder.transform.forward * ladderDropForwardMultiplier, ladderDropJumpMultiplier);
    }

    void JumpToLedge() {
        MoveBase.movementStateMachine.NormalMovement();
        playerWalkMove.JumpInDirection(-ladder.transform.forward * ledgeForwardMultiplier, ledgeJumpMultiplier);
    }

    void JumpOff() {
        MoveBase.movementStateMachine.NormalMovement();
        transform.rotation = RotateAngle180(transform.rotation);
        playerWalkMove.JumpInDirection(-ladder.transform.forward * offLadderForwardMultiplier, offLaddJumpMultiplier);
    }

    static Quaternion RotateAngle180(Quaternion angleToRotate) {
        var newRotation = angleToRotate.eulerAngles;
        newRotation = new Vector3(newRotation.x, newRotation.y + 180, newRotation.z);
        return Quaternion.Euler(newRotation);
    }

}
