using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(DealDamage))]
public class PlayerMoveBase : MonoBehaviour {
    public Transform mainCam, floorChecks;
    public Animator animator;
    public AudioClip jumpSound;
    public AudioClip landSound;
    MovementStateMachine movementStateMachine;

    public Transform[] floorCheckers;
    public GroundHitInfo[] groundInfo;

    public Vector3 slopeNormal;

    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckOffset = .05f;

    public Rigidbody rigid;
    Collider col;

    Quaternion screenMovementSpace;
    Vector3 screenMovementForward, screenMovementRight;

    [SerializeField] DealDamage dealDamage;
    EnemyAI currentEnemyAI;

    void OnEnable() {
        col = GetComponent<Collider>();
        groundInfo = new GroundHitInfo[floorCheckers.Length];
    }

    void Update() => rigid.WakeUp();

    public bool IsGrounded()
    {
        for (var i = 0; i < floorCheckers.Length; i++)
            groundInfo[i] = GetGroundHitInfo(floorCheckers[i]);
        if (EnemyBounceHit() != null) {
            var enemyTransform = EnemyBounceHit().transform;
            currentEnemyAI = enemyTransform.GetComponent<EnemyAI>();
            currentEnemyAI.BouncedOn();
            dealDamage.Attack(enemyTransform.gameObject, 1, 0f, 0f);
        }
        slopeNormal = AverageContactNormal();
        return PointsOfContact() != 0;
    }

    public int PointsOfContact() => groundInfo.Count(t => t != null);

    [CanBeNull] GroundHitInfo EnemyBounceHit() =>
        groundInfo.Where(info => info != null).FirstOrDefault(info => info.transform.CompareTag("Enemy"));

    [CanBeNull] GroundHitInfo GetGroundHitInfo(Transform checker) {
        RaycastHit hit;
        var distanceToCheck = col.bounds.extents.y;
        return Physics.Raycast(checker.position, Vector3.down * 1, out hit, distanceToCheck + groundCheckOffset, groundMask) ?
                   new GroundHitInfo(hit.point, hit.normal, hit.transform, hit.transform.GetComponent<Rigidbody>()) :
                   null;
    }

    public Vector3 MovementRelativeToCamera(Vector2 input) {
        screenMovementSpace = Quaternion.Euler(0, mainCam.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;
        return (screenMovementForward * input.y) + (screenMovementRight * input.x);
    }


    public float SlopeAngle() => Vector3.Angle (slopeNormal, Vector3.up);


    static Vector3 CalculateSlopeNormal(Vector3 totalNormal, int rayCount) =>
        new Vector3(Mathf.Abs(totalNormal.x) > 0 ? totalNormal.x / rayCount : 0,
                    Mathf.Abs(totalNormal.y) > 0 ? totalNormal.y / rayCount : 0,
                    Mathf.Abs(totalNormal.z) > 0 ? totalNormal.z / rayCount : 0);

    Vector3 AverageContactNormal() {
        var summedNormal = groundInfo.Where(t => t != null).Aggregate(Vector3.zero, (current, t) => current + t.normal);
        return CalculateSlopeNormal(summedNormal, PointsOfContact());
    }
}

public class GroundHitInfo {
    public Vector3 position;
    public Vector3 normal;
    public Transform transform;
    public Rigidbody rigid;

    public GroundHitInfo(Vector3 pos, Vector3 norm, Transform tran, [CanBeNull] Rigidbody rb) {
        position = pos;
        normal = norm;
        transform = tran;
        rigid = rb;
    }
}
