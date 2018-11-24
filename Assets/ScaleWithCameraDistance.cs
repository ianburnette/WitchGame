using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ScaleWithCameraDistance : MonoBehaviour
{
    [SerializeField] Transform camera;
    [SerializeField] AnimationCurve scaleCurve;
    [SerializeField] float scaleTime;
    [field: SerializeField] public Vector3 CurrentTargetScale { get; set;  }
    [field: SerializeField] public float ScaleOverride { get; set; }

    public float dist;

    void Update()
    {
        var vec = GetScaleValue();
        CurrentTargetScale = new Vector3(vec, vec, vec);
        transform.localScale = Vector3.Lerp(transform.localScale, CurrentTargetScale * ScaleOverride, 
            scaleTime * Time.deltaTime);
    }

    float GetScaleValue()
    {
        dist = Vector3.Distance(transform.position, camera.position);
        return scaleCurve.Evaluate(dist);
    }
}
