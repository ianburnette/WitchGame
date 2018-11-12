using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum UmbrellaState { Loose, Closed, Open, OpenDirOne, OpenDirTwo, Falling }
public enum UmbrellaType { Unidirectional, Bidirectional }

public class Umbrella : MonoBehaviour, IInteractable
{
    [SerializeField] private float force;
    [FormerlySerializedAs("currentState")] [SerializeField] UmbrellaState currentState;
    [SerializeField] UmbrellaType thisUmbrellaType;
    [SerializeField] Animator anim;
    
    [Header("Bidirectionality")]
    [SerializeField] Vector2 secondStateAngledifference;
    [SerializeField] float stateChangeTime;
    [SerializeField] GoEaseType directionChangeEaseType;
    
    [Header("Dislodging")]
    [SerializeField] Vector3 postDislodgePosition;
    [SerializeField] Vector3 halfWay;
    [FormerlySerializedAs("dislogdeEaseType")] [SerializeField] GoEaseType dislodgeEaseType;
    [SerializeField] float dislodgeTime;
    [SerializeField] float halfwayYHeightAboveStart;
    [SerializeField] Vector3 spinDirection;

    void Start()
    {
        if (currentState != UmbrellaState.Closed && currentState != UmbrellaState.Loose)
            anim.SetTrigger("Open");
    }

    public void SomethingEnteredTrigger(Collider other)
    {
        var rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        rb.AddForce(force * transform.up, ForceMode.Impulse);
        anim.SetTrigger("Bounce");
        if (!other.CompareTag("Player")) return;
        var movementStateMachine = other.GetComponent<MovementStateMachine>();
        if (movementStateMachine.CurrentMovementState == MoveState.Glide)
            movementStateMachine.CurrentMovementState = MoveState.Walk;
        other.GetComponent<PlayerObjectInteraction>().AccidentallyLetGoOfPickup();
        other.GetComponent<PlayerMoveBase>().LockCamAndResetOnGround();
        other.GetComponent<PlayerWalkMove>().CanReleaseToResetVelocity = false;
        if (thisUmbrellaType == UmbrellaType.Bidirectional)
            ChangeDirection();
    }

    public void Interact()
    {
        switch (currentState)
        {
            case UmbrellaState.Loose: Dislodge();             break;
            case UmbrellaState.Closed: Open();                break;
            case UmbrellaState.Open:                          break;
            case UmbrellaState.OpenDirOne: ChangeDirection(); break;
            case UmbrellaState.OpenDirTwo: ChangeDirection(); break;
            case UmbrellaState.Falling:                       break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    void ChangeDirection()
    {
        var config = new GoTweenConfig();
        config.eulerAngles(currentState==UmbrellaState.OpenDirOne ? 
            secondStateAngledifference : -secondStateAngledifference, true);
        config.easeType = directionChangeEaseType;
        var state = currentState;
        config.onComplete(c =>
            currentState = state == UmbrellaState.OpenDirOne
                ? UmbrellaState.OpenDirTwo
                : UmbrellaState.OpenDirOne);
        Go.to(transform, stateChangeTime, config);
        currentState = UmbrellaState.Falling;
    }

    void Open()
    {
        currentState = thisUmbrellaType == UmbrellaType.Unidirectional ? 
            UmbrellaState.Open : UmbrellaState.OpenDirOne;
        anim.SetTrigger("Open");
    }

    void Dislodge()
    {
        var pos = transform.position;
        halfWay = (pos + (pos + postDislodgePosition)) / 2;
        halfWay.y = pos.y + halfwayYHeightAboveStart;
        var points = new[]
        {
            pos,
            halfWay,
            transform.position + postDislodgePosition
        };
        var path = new GoSpline(points);
        var config = new GoTweenConfig();
        config.positionPath(path, false);
        // TODO: put in directional sensitivity here
        config.eulerAngles(spinDirection * 360, true);
        config.easeType = dislodgeEaseType;
        config.onComplete(_=>
        {
            currentState = UmbrellaState.Closed;
            Open();
        });
        Go.to(transform, dislodgeTime * Vector3.Distance(pos, postDislodgePosition), config);
        currentState = UmbrellaState.Falling;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + postDislodgePosition, .3f);
    }
}

public interface IInteractable
{
    void Interact();
}
