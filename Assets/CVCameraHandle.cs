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
    [SerializeField] private float biasChangeSpeed;
    [SerializeField] private float targetBias;
    private float biasPlaceholder = 0;
    private Vector2 xAxisBiasRange = new Vector2(0, 90);

    private void OnEnable()
    {
        instance = this;
        freeLook = GetComponent<CinemachineFreeLook>();
    }

    private void Update()
    {
        if (targetBias != freeLook.m_XAxis.Value)
        freeLook.m_XAxis.Value = Mathf.Lerp(freeLook.m_XAxis.Value, targetBias, biasChangeSpeed * Time.deltaTime);
    }

    public void SetBias(float biasOverride)
    {
        biasPlaceholder = freeLook.m_XAxis.Value;
        freeLook.m_XAxis.ValueRangeLocked = true;
        targetBias = biasOverride;
    }

    public void ResetBias()
    {
        freeLook.m_XAxis.ValueRangeLocked = false;
        targetBias = biasPlaceholder;
    }
}
