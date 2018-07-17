	using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCamera : CameraBase {

	public override Vector3 LookDirection(float offset) => toFollow.forward;

    public override Vector3 TargetPosition() =>
        toFollow.position +
        toFollow.up * cameraOffset.y -
        toFollow.forward * cameraOffset.z;

}
