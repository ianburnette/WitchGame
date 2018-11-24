using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using Cinemachine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Serialization;


public class CVCameraHandle : MonoBehaviour
{
    [Header("References")]
    public static CVCameraHandle instance;
    CinemachineFreeLook freeLook;

    [SerializeField] bool overriding;
    CameraBiasSetting currentSetting;

    [SerializeField] float zoomAccuracyThreshold = .01f;
    
    public CameraBiasSetting CurrentSetting { get => currentSetting; 
                                              set { currentSetting = value;
                                                    overriding = currentSetting == null;
                                                    PopulateValuesForSetting(); }}
    public float zoomValue { get => freeLook.m_YAxis.Value;
                              set => freeLook.m_YAxis.Value = value; }

    private void OnEnable()
    {
        instance = this;
        freeLook = GetComponent<CinemachineFreeLook>();
    }
    
    void Update()
    {
        if (!overriding) return;
        
    }

    void PopulateValuesForSetting()
    {
        
    }

    public IEnumerator SetZoomValue(float newValue, float time)
    {
        var transitioning = true;
       // var config = new GoTweenConfig();
       // config.floatProp("YaxisValue", newValue, false);
       // config.onComplete(_ => transitioning = false);
       // Go.to(this, time, config);
       
        while (Mathf.Abs(zoomValue-newValue) < zoomAccuracyThreshold)
            zoomValue = Mathf.Lerp(zoomValue , newValue, time * Time.unscaledDeltaTime);
            yield return new WaitForEndOfFrame();
        yield return true;
    }
    //[Header("Bias Override")] 
    //[FormerlySerializedAs("overriding")]
    //[SerializeField] private bool overridingX;
    //[SerializeField] private bool overridingY;
    //[FormerlySerializedAs("biasChangeSpeed")] [SerializeField] private float XbiasChangeSpeed;
    //[FormerlySerializedAs("biasChangeSpeed")] [SerializeField] private float YbiasChangeSpeed;
    //[SerializeField] private float targetBias;
    //[SerializeField] private float biasPlaceholder = 0;
    //private Vector2 xAxisBiasRange = new Vector2(0, 90);
    //[SerializeField] private float biasTolerance = .01f;
//
    //float yAxisOverride;
    //[SerializeField] float overrideMargin;
    //
    //public bool OverridingY { get { return overridingY; } set { overridingY = value; }}
//
    //[SerializeField] GoEaseType thisEaseType;
    //[SerializeField] float easeSpeed;

//
    //void Update()
    //{
    //    if (overridingX && Math.Abs(targetBias - freeLook.m_XAxis.Value) > biasTolerance)
    //        freeLook.m_XAxis.Value = Mathf.Lerp(freeLook.m_XAxis.Value, targetBias, YbiasChangeSpeed * Time.deltaTime);
    //    if (OverridingY)
    //    {
    //        var temp = freeLook.m_YAxis.Value;
    //        temp = Mathf.Lerp(temp, YAxisOverride, YbiasChangeSpeed * Time.unscaledDeltaTime);
    //        freeLook.m_YAxis.Value = temp;
    //        if (freeLook.m_YAxis.Value >= yAxisOverride - overrideMargin)
    //        {
    //            TransitioningY = false;
    //            OverridingY = false;
    //        }
    //    }
    //}
//
    //public void SetXBias(float biasOverride)
    //{
    //    overridingX = true;
    //    biasPlaceholder = freeLook.m_XAxis.Value;
    //    freeLook.m_XAxis.ValueRangeLocked = true;
    //    targetBias = biasOverride;
    //}
//
    //public void ResetBias()
    //{
    //    overridingX = false;
    //    overridingY = false;
    //    freeLook.m_XAxis.ValueRangeLocked = false;
    //    targetBias = biasPlaceholder;
    //}
}
