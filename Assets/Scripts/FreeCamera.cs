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

    void OnEnable() => SetCamPositionRelativeToTarget();//transform.position = TargetPosition();

    public override void LateUpdate() {
        SetRigRotation(cameraInput.x);
        SetPosition();
        SetRotation();
    }

    void SetRigRotation(float rotationInput) =>
        transform.RotateAround(toFollow.position, toFollow.up, rotationSpeed * rotationInput * Time.deltaTime);

    Vector3 CurrentFreeCameraOffset() {
        zoomValue += -cameraInput.y * zoomSpeed;
        zoomValue = Mathf.Clamp(zoomValue, 0, 1);

        if (Math.Abs(cameraInput.x) > .01f) SetCamPositionRelativeToTarget();

        return new Vector3(0,
            Mathf.Lerp(cameraOffset.y * minOffsetMultiplier.y, cameraOffset.y * maxOffsetMultiplier.y, zoomValue),
            Mathf.Lerp(cameraOffset.z * minOffsetMultiplier.z, cameraOffset.z * maxOffsetMultiplier.z, zoomValue));
    }

    void SetCamPositionRelativeToTarget() {
        cameraPositionRelativeToTarget = Vector3.Normalize(toFollow.position - transform.position);
        cameraPositionRelativeToTarget.y = 0;
    }

    public override Vector3 TargetPosition() =>
        toFollow.position +
        toFollow.up * CurrentFreeCameraOffset().y -
        cameraPositionRelativeToTarget * CurrentFreeCameraOffset().z;
}

