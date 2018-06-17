using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : CameraBase {

    [SerializeField] float smootheValue = 1f;
    
    public override void LateUpdate()
    {
        Debugs();
        SetPosition();
        LookAtTarget();
    }

    public override Vector3 TargetPosition() => 
        CharacterOffset(distanceAbovePlayer) + 
        toFollow.up * distanceAbovePlayer - 
        LookDirection(distanceAbovePlayer) * distanceBehindPlayer;

    void Debugs()
    {
        Debug.DrawRay(toFollow.position, Vector3.up * distanceAbovePlayer, Color.red);
        Debug.DrawRay(toFollow.position, -1f * toFollow.forward * distanceBehindPlayer, Color.blue);
        Debug.DrawRay(toFollow.position, toFollow.position - TargetPosition(), Color.magenta);
    }
}
