using System;
using UnityEngine;

public class FreeCamera : CameraBase {

    [Header("Free Cam Variables")]
    [SerializeField] Vector3 maxOffsetMultiplier = new Vector3(0f, 1.5f, 5f);
    [SerializeField] Vector3 minOffsetMultiplier = new Vector3(0f, .2f, .6f);

    Vector3 currentFreeCameraOffset;
    Vector2 cameraInput;

    [SerializeField] float zoomValue;

    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float zoomSpeed = 2f;

    Vector3 cameraPositionRelativeToTarget;

    void Update() {
        cameraInput = new Vector2(Input.GetAxis("RightStickX"), Input.GetAxis("RightStickY"));
    }

    public override void LateUpdate() {
        SetRigPosition();
        SetRigRotation();
        SetPosition();
        SetRotation();
    }

    void SetRigRotation() =>
        transform.RotateAround(toFollow.position, toFollow.up, rotationSpeed * cameraInput.x * Time.deltaTime);

    void SetRigPosition() {
        zoomValue += -cameraInput.y * zoomSpeed;
        zoomValue = Mathf.Clamp(zoomValue, 0, 1);

        if (Math.Abs(cameraInput.x) > .01f) {
            cameraPositionRelativeToTarget = Vector3.Normalize(toFollow.position - transform.position);
            cameraPositionRelativeToTarget.y = 0;
        }

        currentFreeCameraOffset = new Vector3(0,
            Mathf.Lerp(cameraOffset.y * minOffsetMultiplier.y, cameraOffset.y * maxOffsetMultiplier.y, zoomValue),
            Mathf.Lerp(cameraOffset.z * minOffsetMultiplier.z, cameraOffset.z * maxOffsetMultiplier.z, zoomValue));
    }

    public override Vector3 TargetPosition() =>
        toFollow.position +
        toFollow.up * currentFreeCameraOffset.y -
        cameraPositionRelativeToTarget * currentFreeCameraOffset.z;
}

