using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBiasZone : MonoBehaviour
{
    [SerializeField] CameraBiasSetting mySettings; 
    [SerializeField] private float myBias;
    [SerializeField] float transitionTime;
    
    private void OnTriggerEnter(Collider other)
    {
        CVCameraHandle.instance.CurrentSetting = mySettings;
    }

    private void OnTriggerExit(Collider other) => CVCameraHandle.instance.CurrentSetting = null;
}

public enum CameraBiasType
{
    RotationOnly,
    ZoomOnly,
    RotationAndZoom,
}

public class CameraBiasSetting
{
    public Transform followOverride;
    public CameraBiasType biasType;
}