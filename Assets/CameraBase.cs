using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraBase : MonoBehaviour {

    [SerializeField] Vector3 velocityCamSmoothe = Vector3.zero;
    [SerializeField] float camSmoothDampTime = .1f;
    [SerializeField] public Transform toFollow;

    [SerializeField] protected float distanceAbovePlayer = 3f;
    [SerializeField] protected float distanceBehindPlayer = 10f;

    protected void LookAtTarget() => transform.LookAt(toFollow.position);

    public virtual Vector3 CharacterOffset(float offset) => toFollow.position + Vector3.up * offset;

    public virtual Vector3 LookDirection(float offset)
    {
        Vector3 dir = CharacterOffset(offset) - transform.position;
        dir.y = 0;
        return dir.normalized;
    }

    public virtual void LateUpdate()
    {
        SetPosition();
        LookAtTarget();
    }

    public abstract Vector3 TargetPosition();// => CharacterOffset(offset) + toFollow.up * distanceAbovePlayer - LookDirection(offset) * distanceBehindPlayer;

    protected void SetPosition() => transform.position = SmoothePosition(transform.position, CompensateForWalls(CharacterOffset(distanceAbovePlayer), TargetPosition()));

    protected Vector3 CompensateForWalls(Vector3 from, Vector3 targetPosition)
    {
        Debug.DrawLine(from, targetPosition, Color.cyan);
        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(from, targetPosition, out wallHit))
        {
            Debug.DrawRay(wallHit.point, Vector3.left, Color.red);
            return new Vector3(wallHit.point.x, targetPosition.y, wallHit.point.z);
        }
        return targetPosition;
    }

    protected Vector3 SmoothePosition(Vector3 inputPosition, Vector3 targetPosition) =>
       Vector3.SmoothDamp(inputPosition, targetPosition, ref velocityCamSmoothe, camSmoothDampTime);
}
