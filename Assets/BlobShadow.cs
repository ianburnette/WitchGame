using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobShadow : MonoBehaviour
{
    [SerializeField] bool lockRotation;
    [SerializeField] Quaternion baseRotation;
    
    void Start()
    {
        baseRotation = transform.rotation;
    }
    // Update is called once per frame
    void Update()
    {
        if (lockRotation)
            transform.rotation=baseRotation; 
    }
}
