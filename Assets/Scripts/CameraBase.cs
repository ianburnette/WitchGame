using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraBase : MonoBehaviour {

    Vector3 velocityCamSmoothe = Vector3.zero;
    [Header("Global Camera Variables")]
    [SerializeField] float camSmoothDampTime = .1f;
    [SerializeField] public Transform toFollow;
    [SerializeField] public Transform lens;
    [SerializeField] LayerMask cameraHitLayer;

    [SerializeField] protected Vector3 cameraOffset = new Vector3(0f, 3f, 10f);

    protected virtual void SetRotation() => lens.LookAt(toFollow.position);

    public virtual Vector3 LookDirection(float offset) {
        var dir = toFollow.position - transform.position;
        dir.y = 0;
        return dir.normalized;
    }

    public virtual void LateUpdate()
    {
        SetPosition();
        SetRotation();
    }

    public abstract Vector3 TargetPosition();

    protected void SetPosition() => transform.position = SmoothePosition(transform.position, CompensateForWalls(toFollow.position, TargetPosition()));

    protected Vector3 CompensateForWalls(Vector3 from, Vector3 targetPosition)
    {
        var wallHit = new RaycastHit();
        return Physics.Linecast(from, targetPosition, out wallHit, cameraHitLayer) ?
                   new Vector3(wallHit.point.x, targetPosition.y, wallHit.point.z) :
                   targetPosition;
    }

    protected Vector3 SmoothePosition(Vector3 inputPosition, Vector3 targetPosition) =>
       Vector3.SmoothDamp(inputPosition, targetPosition, ref velocityCamSmoothe, camSmoothDampTime);
}
