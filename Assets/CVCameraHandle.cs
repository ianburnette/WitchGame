using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Serialization;


public class CVCameraHandle : MonoBehaviour
{
    public static CVCameraHandle instance;
    private CinemachineFreeLook freeLook;

    [Header("Bias Override")] 
    [FormerlySerializedAs("overriding")]
    [SerializeField] private bool overridingX;
    [SerializeField] private bool overridingY;
    [FormerlySerializedAs("biasChangeSpeed")] [SerializeField] private float XbiasChangeSpeed;
    [FormerlySerializedAs("biasChangeSpeed")] [SerializeField] private float YbiasChangeSpeed;
    [SerializeField] private float targetBias;
    [SerializeField] private float biasPlaceholder = 0;
    private Vector2 xAxisBiasRange = new Vector2(0, 90);
    [SerializeField] private float biasTolerance = .01f;

    float yAxisOverride;
    [SerializeField] float overrideMargin;
    
    public float YAxisOverride
    {
        get => yAxisOverride;
        set
        {
            TransitioningY = true;
            yAxisOverride = value;
        }
    }

    public bool TransitioningY { get; set; }

    public bool OverridingY
    {
        get { return overridingY; }
        set { overridingY = value; }
    }

    [SerializeField] GoEaseType thisEaseType;
    [SerializeField] float easeSpeed;

    private void OnEnable()
    {
        instance = this;
        freeLook = GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        if (overridingX && Math.Abs(targetBias - freeLook.m_XAxis.Value) > biasTolerance)
            freeLook.m_XAxis.Value = Mathf.Lerp(freeLook.m_XAxis.Value, targetBias, YbiasChangeSpeed * Time.deltaTime);
        if (OverridingY)
        {
            var temp = freeLook.m_YAxis.Value;
            temp = Mathf.Lerp(temp, YAxisOverride, YbiasChangeSpeed * Time.unscaledDeltaTime);
            freeLook.m_YAxis.Value = temp;
            if (freeLook.m_YAxis.Value >= yAxisOverride - overrideMargin)
            {
                TransitioningY = false;
                OverridingY = false;
            }
        }
    }

    public void SetBias(float biasOverride)
    {
        overridingX = true;
        biasPlaceholder = freeLook.m_XAxis.Value;
        freeLook.m_XAxis.ValueRangeLocked = true;
        targetBias = biasOverride;
    }

    public void ResetBias()
    {
        overridingX = false;
        freeLook.m_XAxis.ValueRangeLocked = false;
        targetBias = biasPlaceholder;
    }
}
