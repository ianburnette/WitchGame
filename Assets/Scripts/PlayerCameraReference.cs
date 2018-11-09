using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraReference : MonoBehaviour {
    [SerializeField] Transform player;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float movementSpeed;
    [SerializeField] float groundHeightOffset;
    [SerializeField] float distanceToCheck = 10f;
    [SerializeField] Vector3 targetPosition;

    [SerializeField] private bool lockToPlayer;

    public bool LockToPlayer
    {
        private get => lockToPlayer;
        set => lockToPlayer = value;
    }

    GroundHitInfo hitInfo;

    void Update()
    {
        targetPosition = !LockToPlayer ? FollowPlayerWithGroundSensitivity() : PlayerPositionWithOffset();
        transform.position = Vector3.Lerp(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    Vector3 FollowPlayerWithGroundSensitivity()
    {
        hitInfo = GroundChecking.GetGroundHitInfo(player, distanceToCheck, groundLayer);
        if (hitInfo != null)
            targetPosition = new Vector3(player.position.x,
                hitInfo.position.y + groundHeightOffset,
                player.position.z);
        else
            targetPosition = PlayerPositionWithOffset();
        return targetPosition;
    }

    Vector3 PlayerPositionWithOffset() => player.transform.position + (Vector3.up * groundHeightOffset);
}
