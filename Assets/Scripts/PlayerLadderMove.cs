using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

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

    [FormerlySerializedAs("ladderTopJumpLeniancyDistance")] [Header("Passive Behavior")]
    public float ladderTopJumpLeniencyDistance = 2f;

    [Header("Reference Variables")]
    public Vector3 amt;
    public Ladder ladder;
    public Ladder mostRecentLadder;

    [Header("Class References")]
    [SerializeField] PlayerMoveBase MoveBase;
    [SerializeField] PlayerWalkMove playerWalkMove;
    [SerializeField] float positionCorrectionSpeed;

    void OnEnable() {
        PlayerInput.OnMove += Move;
        PlayerInput.OnJump += Jump;
        PlayerInput.OnGrab += Drop;
        MoveBase.rigid.isKinematic = true;
        MoveBase.camReferenceTransform.LockToPlayer = true;
    }

    void Update()
    {
        var lad = CurrentOrMostRecentLadder().transform;
        transform.rotation = RotateAngle180(lad.rotation);
        var ladPos = lad.position;
        transform.position = Vector3.Lerp(transform.position,
            new Vector3(ladPos.x, transform.position.y, ladPos.z),
            positionCorrectionSpeed * Time.deltaTime);
    }

    void OnDisable() {
        PlayerInput.OnMove -= Move;
        PlayerInput.OnJump -= Jump;
        PlayerInput.OnGrab -= Drop;
        MoveBase.rigid.isKinematic = false;
        MoveBase.camReferenceTransform.LockToPlayer = false;
    }

    void Move(Vector2 movementInput)
    {
        var lad = CurrentOrMostRecentLadder();
        if ((movementInput.y > 0 && transform.position.y < lad.MaxHeight.y))
            MoveBase.characterMotor.MoveVertical(Vector3.up * movementInput.y * climbSpeed * Time.deltaTime);
        if((movementInput.y < 0 && transform.position.y >lad.MinHeight.y))
            MoveBase.characterMotor.MoveVertical(Vector3.up * movementInput.y * climbSpeed * Time.deltaTime);
        if (movementInput.y > 0 && transform.position.y >= lad.MaxHeight.y)
            JumpToLedge();
        if (movementInput.y < 0 && transform.position.y <= lad.MinHeight.y)
            Drop();
    }

    void Jump()
    {
        var tempLadder = CurrentOrMostRecentLadder();

        if (Vector3.Distance(transform.position, tempLadder.MaxHeight) <
            ladderTopJumpLeniencyDistance)
            JumpToLedge();
        else
            JumpOff();
    }

    private Ladder CurrentOrMostRecentLadder()
    {
        Ladder tempLadder;
        tempLadder = ladder != null ? ladder : mostRecentLadder;
        return tempLadder;
    }

    void Drop() {
        MoveBase.movementStateMachine.NormalMovement();
        JumpInDirection(new Vector2(ladderDropForwardMultiplier, ladderDropJumpMultiplier));
    }

    void JumpToLedge() {
        MoveBase.movementStateMachine.NormalMovement();
        JumpInDirection(new Vector2(ledgeForwardMultiplier, ledgeJumpMultiplier));
    }

    void JumpOff() {
        MoveBase.movementStateMachine.NormalMovement();
        transform.rotation = RotateAngle180(transform.rotation);
        JumpInDirection(new Vector2(offLadderForwardMultiplier, offLaddJumpMultiplier));
    }

    void JumpInDirection(Vector2 mult)
    {
        playerWalkMove.JumpInDirection(Vector3.forward * mult.x, mult.y);
    }

    static Quaternion RotateAngle180(Quaternion angleToRotate) {
        var newRotation = angleToRotate.eulerAngles;
        newRotation = new Vector3(newRotation.x, newRotation.y + 180, newRotation.z);
        return Quaternion.Euler(newRotation);
    }

}
