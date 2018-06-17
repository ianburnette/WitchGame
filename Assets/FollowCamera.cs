using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : CameraBase {

    [Header("Follow Cam Variables")]
    [SerializeField] float smootheValue = 1f;

    public override void LateUpdate()
    {
        //Debugs();
        SetPosition();
        SetRotation();
    }

    public override Vector3 TargetPosition() =>
        toFollow.position +
        toFollow.up * cameraOffset.y -
        LookDirection(cameraOffset.y) * cameraOffset.z;

    void Debugs()
    {
        Debug.DrawRay(toFollow.position, Vector3.up * cameraOffset.y, Color.red);
        Debug.DrawRay(toFollow.position, -1f * toFollow.forward * cameraOffset.z, Color.blue);
        Debug.DrawRay(toFollow.position, toFollow.position - TargetPosition(), Color.magenta);
    }
}
