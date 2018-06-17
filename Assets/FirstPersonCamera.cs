using UnityEngine;

public class FirstPersonCamera : CameraBase {
    [Header("First Person Cam Variables")]
    [SerializeField] float camRotSpeed = 2f;
    [SerializeField] Vector2 rotationSpeed = new Vector2(2f,2f);
    [SerializeField] Vector2 xRotationBounds, yRotationBounds;
    Vector2 cameraInput;
    Vector2 calculatedCamRot;

    void Update() {
        cameraInput = new Vector2(Input.GetAxis("RightStickX"), Input.GetAxis("RightStickY"));
        SetToFollowRotation();
    }

    public override Vector3 TargetPosition() => toFollow.position;

    protected override void SetRotation() {
        lens.rotation = Quaternion.Lerp(lens.rotation, toFollow.rotation, camRotSpeed * Time.deltaTime);
    }


    void SetToFollowRotation() {
        calculatedCamRot += new Vector2(cameraInput.x * rotationSpeed.x, -cameraInput.y * rotationSpeed.y);
        calculatedCamRot = new Vector2(Mathf.Clamp(calculatedCamRot.x, xRotationBounds.x, xRotationBounds.y),
                                       Mathf.Clamp(calculatedCamRot.y, yRotationBounds.x, yRotationBounds.y));

        toFollow.localRotation = Quaternion.Euler(calculatedCamRot.y, calculatedCamRot.x, 0);
    }
}
