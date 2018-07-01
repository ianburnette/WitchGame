using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudColumn : MonoBehaviour {

    public bool cloudColumnIsCurrentlyActive = false;
    public bool staticCloudColumnExists = false;

    public bool touchingGround;
    Vector3 originPosition;
    [SerializeField] PlayerCloudColumn playerCloudColumn;
    [SerializeField] CloudColumnGeneration cloudGeneration;

    public bool AttemptToToggleNewColumn() {
        if (cloudColumnIsCurrentlyActive) {
            StopCloudColumn();
            return false;
        }
        CreateCloudColumn();
        return true;
    }

    void CreateCloudColumn() {
        PrepareCloudColumn();
        cloudColumnIsCurrentlyActive = true;
    }

    void PrepareCloudColumn() {
        if (staticCloudColumnExists)
            Reset();
    }

    void Reset() {
        staticCloudColumnExists = false;
        cloudGeneration.StartOver();
        //TODO fold the column back up here and release any particles
    }

    void StopCloudColumn() {
        cloudColumnIsCurrentlyActive = false;
        staticCloudColumnExists = true;
    }

    public void HitWorld() {
        StopCloudColumn();
    }

    void Update() {
        if (!cloudColumnIsCurrentlyActive) return;
        if (!cloudGeneration.maxLengthReached)
            UpdateCloudHeadPosition();
        else
            StopCloudColumn();
    }

    void UpdateCloudHeadPosition() {
        transform.position = playerCloudColumn.CloudOffsetFromPlayer;
    }
}
