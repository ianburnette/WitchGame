using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class CloudColumnGeneration : MonoBehaviour {
    [SerializeField] Transform targetCloud;
    [SerializeField] int piecesToGenerate;
    [SerializeField] float maxSize;
    [SerializeField] float growSpeed;
    [SerializeField] float sizeDivisor;
    [SerializeField] float maxDistanceToNextCloud = 2f;
    [SerializeField] Transform cloudPrefab;
    [SerializeField] Cloud currentCloudPiece;
    [SerializeField] Vector3 startingPosition;
    [SerializeField] List<Cloud> cloudPieces;
    public bool maxLengthReached;

    float DistanceToTarget(Vector3 pos) => Vector3.Distance(pos, targetCloud.position);

    void OnEnable() {
        StartOver();
    }

    public void StartOver() {
        maxLengthReached = false;
        ClearList();
        AddPiece(startingPosition);
    }

    void Update() {

        if (DistanceToTarget(currentCloudPiece.Pos) < maxDistanceToNextCloud) {
            if (currentCloudPiece.Scale < maxSize)
                currentCloudPiece.Grow(growSpeed * (DistanceToTarget(currentCloudPiece.Pos) / sizeDivisor));
            currentCloudPiece.transform.LookAt(targetCloud);
        } else if (cloudPieces.Count < piecesToGenerate)
            AddPiece(targetCloud.position);
        else {
            maxLengthReached = true;
        }
    }

    void AddPiece(Vector3 location) {
        currentCloudPiece = Instantiate(cloudPrefab, location, Quaternion.identity).GetComponent<Cloud>();
        //currentCloudPiece.transform.parent = transform;
        currentCloudPiece.transform.localScale = Vector3.one/90f;
        cloudPieces.Add(currentCloudPiece);
    }

    void ClearList() {
        foreach (var cloud in cloudPieces)
            Destroy(cloud.gameObject);
        cloudPieces.Clear();
    }
}
