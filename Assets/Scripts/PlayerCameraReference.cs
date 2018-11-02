using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraReference : MonoBehaviour {
    [SerializeField] Transform player;
    [SerializeField] PlayerMoveBase moveBase;
    [SerializeField] Camera camera;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float movementSpeed;
    [SerializeField] float groundHeightOffset;
    [SerializeField] float distanceToCheck = 10f;
    [SerializeField] Vector3 targetPosition;

    GroundHitInfo hitInfo;

    void Update() {
        hitInfo = GroundChecking.GetGroundHitInfo(player, distanceToCheck, groundLayer);
        if (hitInfo != null)
            targetPosition = new Vector3(player.position.x,
                                         hitInfo.position.y + groundHeightOffset,
                                         player.position.z);
        else
            targetPosition = player.transform.position + (Vector3.up * groundHeightOffset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }
}
