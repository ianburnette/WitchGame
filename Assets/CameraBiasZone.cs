using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBiasZone : MonoBehaviour
{
    [SerializeField] private float myBias;
    private void OnTriggerEnter(Collider other) => CVCameraHandle.instance.SetBias(myBias);
    private void OnTriggerExit(Collider other) => CVCameraHandle.instance.ResetBias();
}
