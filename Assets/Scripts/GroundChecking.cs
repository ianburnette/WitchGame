using JetBrains.Annotations;
using UnityEngine;

static class GroundChecking {
    [CanBeNull]
    public static GroundHitInfo GetGroundHitInfo(Transform checker, float distanceToCheck, LayerMask groundMask) {
        RaycastHit hit;
        Debug.DrawRay(checker.position, Vector3.down * distanceToCheck, Color.red);
        return Physics.Raycast(checker.position, Vector3.down, out hit, distanceToCheck, groundMask)
                   ? new GroundHitInfo(hit.point, hit.normal, hit.transform, hit.transform.GetComponent<Rigidbody>(),
                                       hit.transform.CompareTag("SolidCloud")
                                           ? GroundType.Cloud
                                           : GroundType.Ground)
                   : null;
    }
}

public class GroundHitInfo {
    public Vector3 position;
    public Vector3 normal;
    public Transform transform;
    public Rigidbody rigid;
    public GroundType groundType;

    public GroundHitInfo(Vector3 pos, Vector3 norm, Transform tran, [CanBeNull] Rigidbody rb, GroundType type) {
        position = pos;
        normal = norm;
        transform = tran;
        rigid = rb;
        groundType = type;
    }
}

public enum GroundType {
    Ground,
    Cloud,
    None
}
