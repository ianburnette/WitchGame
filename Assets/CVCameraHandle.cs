using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Experimental.PlayerLoop;


public class CVCameraHandle : MonoBehaviour
{
    public static CVCameraHandle instance;
    private CinemachineFreeLook freeLook;



    [Header("Bias Override")] 
    [SerializeField] private bool overriding;
    [SerializeField] private float biasChangeSpeed;
    [SerializeField] private float targetBias;
    private float biasPlaceholder = 0;
    private Vector2 xAxisBiasRange = new Vector2(0, 90);
    [SerializeField] private float biasTolerance = .01f;

    private void OnEnable()
    {
        instance = this;
        freeLook = GetComponent<CinemachineFreeLook>();
    }

    private void Update()
    {
        if (overriding && Math.Abs(targetBias - freeLook.m_XAxis.Value) > biasTolerance)
            freeLook.m_XAxis.Value = Mathf.Lerp(freeLook.m_XAxis.Value, targetBias, biasChangeSpeed * Time.deltaTime);
    }

    public void SetBias(float biasOverride)
    {
        overriding = true;
        biasPlaceholder = freeLook.m_XAxis.Value;
        freeLook.m_XAxis.ValueRangeLocked = true;
        targetBias = biasOverride;
    }

    public void ResetBias()
    {
        overriding = false;
        freeLook.m_XAxis.ValueRangeLocked = false;
        targetBias = biasPlaceholder;
    }
}
